using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;
using SoLoud;

namespace GbaMonoGame;

public class NGageSoundEventsManager : SoundEventsManager
{
    #region Constructor

    public NGageSoundEventsManager(Dictionary<int, string> songResourceFileNames, Dictionary<int, string> songPhysicalFileNames, NGageSoundEvent[] soundEvents)
    {
        _soloud = new Soloud();
        _soloud.init();

        _soundEvents = soundEvents;

        _musicTable = new Dictionary<int, Music>();
        _soundEffectsTable = new Dictionary<int, SoundEffect>();

        _currentMusicVolume = 1;
        _musicFadeVolume = 0;
        _doesCurrentMusicLoop = false;
        _prevMusicSoundResId = -1;
        _prevMusicInstrumentsResId = -1;

        MusicVolume = SoundEngineInterface.MaxVolume;
        SoundEffectsVolume = SoundEngineInterface.MaxVolume;

        Stopwatch sw = Stopwatch.StartNew();

        LoadSongs(songResourceFileNames, songPhysicalFileNames, soundEvents);

        sw.Stop();

        Logger.Info("Loaded songs in {0} ms", sw.ElapsedMilliseconds);
    }

    #endregion

    #region Private Fields

    // Change volume factor to make it match the GBA version. Also the balance between the music and sfx
    // sounds is wrong by default. These are not exact values, but give the closest result.
    private const float MusicVolumeFactor = 2f;
    private const float SfxVolumeFactor = 0.297f;

    private readonly Soloud _soloud;
    private readonly NGageSoundEvent[] _soundEvents;

    private readonly Dictionary<int, Music> _musicTable;
    private readonly Dictionary<int, SoundEffect> _soundEffectsTable;

    private readonly Dictionary<int, SoundEffectInstance> _soundEffectInstances = new(); // On N-Gage this is max 64 songs, but we don't need that limit

    private uint _musicVoiceHandle;
    private bool _doesCurrentMusicLoop;
    private float _currentMusicVolume;
    private float _musicFadeVolume;
    
    private int _prevMusicSoundResId;
    private int _prevMusicInstrumentsResId;
    
    private int _currentMusicSoundResId;
    private int _currentMusicInstrumentsResId;
    
    private int _nextMusicSoundResId;
    private int _nextMusicInstrumentsResId;

    #endregion

    #region Private Properties

    private bool _isMusicInGamePaused;
    private bool _isMusicInEnginePaused;

    private bool IsMusicInGamePaused
    {
        get => _isMusicInGamePaused;
        set
        {
            _isMusicInGamePaused = value;

            if (_soloud.isValidVoiceHandle(_musicVoiceHandle))
                _soloud.setPause(_musicVoiceHandle, value || IsMusicInEnginePaused);
        }
    }
    private bool IsMusicInEnginePaused
    {
        get => _isMusicInEnginePaused;
        set
        {
            _isMusicInEnginePaused = value;

            if (_soloud.isValidVoiceHandle(_musicVoiceHandle))
                _soloud.setPause(_musicVoiceHandle, value || IsMusicInGamePaused);
        }
    }

    #endregion

    #region Public Properties

    public float MusicVolume { get; set; }
    public float SoundEffectsVolume { get; set; }

    #endregion

    #region Private Methods

    private void LoadSongs(Dictionary<int, string> songResourceFileNames, Dictionary<int, string> songPhysicalFileNames, NGageSoundEvent[] soundEvents)
    {
        HashSet<int> loadedSounds = new();
        Dictionary<int, byte[]> loadedInstruments = new();
        foreach (NGageSoundEvent evt in soundEvents)
        {
            if (!evt.IsValid)
                continue;

            if (loadedSounds.Add(evt.SoundResourceId))
            {
                // Load music from XM and instruments
                if (evt.IsMusic)
                {
                    Music music = new()
                    {
                        XmSound = new Openmpt(),
                        FileName = songResourceFileNames[evt.SoundResourceId]
                    };

                    // Load the instruments data
                    if (!loadedInstruments.TryGetValue(evt.InstrumentsResourceId, out byte[] instruments))
                    {
                        RawResource instrumentsResource = Rom.LoadResource<RawResource>(evt.InstrumentsResourceId);
                        instruments = instrumentsResource.RawData;
                        loadedInstruments[evt.InstrumentsResourceId] = instruments;
                    }

                    // Load the XM data
                    RawResource xmResource = Rom.LoadResource<RawResource>(evt.SoundResourceId);
                    byte[] xm = xmResource.RawData;

                    IntPtr xmPtr = IntPtr.Zero;
                    try
                    {
                        int combinedXmLength = xm.Length + instruments.Length - 2;
                        xmPtr = Marshal.AllocHGlobal(combinedXmLength);
                        Marshal.Copy(xm, 0, xmPtr, xm.Length);
                        Marshal.Copy(instruments, 2, xmPtr + xm.Length, instruments.Length - 2);

                        music.XmSound.loadMem(xmPtr, (uint)combinedXmLength, true, false);
                    }
                    finally
                    {
                        if (xmPtr != IntPtr.Zero)
                            Marshal.FreeHGlobal(xmPtr);
                    }

                    _musicTable[evt.SoundResourceId] = music;
                }
                // Load sound effects WAV
                else
                {
                    // Check if we have a physical file for the sound, and if so load from the file
                    if (songPhysicalFileNames.TryGetValue(evt.SoundResourceId, out string physicalFileName))
                    {
                        SoundEffect soundEffect = new()
                        {
                            WavSound = new Wav(),
                            FileName = physicalFileName
                        };

                        soundEffect.WavSound.load($"Assets/Rayman3/{physicalFileName}.wav");

                        _soundEffectsTable[evt.SoundResourceId] = soundEffect;
                    }
                    // Otherwise load from resources
                    else
                    {
                        SoundEffect soundEffect = new()
                        {
                            WavSound = new Wav(),
                            FileName = songResourceFileNames[evt.SoundResourceId]
                        };

                        RawResource resource = Rom.LoadResource<RawResource>(evt.SoundResourceId);
                        byte[] rawData = resource.RawData;

                        IntPtr resourcePtr = IntPtr.Zero;
                        try
                        {
                            resourcePtr = Marshal.AllocHGlobal(rawData.Length);
                            Marshal.Copy(rawData, 0, resourcePtr, rawData.Length);

                            soundEffect.WavSound.loadMem(resourcePtr, (uint)rawData.Length, true, false);
                        }
                        finally
                        {
                            if (resourcePtr != IntPtr.Zero)
                                Marshal.FreeHGlobal(resourcePtr);
                        }

                        _soundEffectsTable[evt.SoundResourceId] = soundEffect;
                    }
                }
            }
        }
    }

    private void SetNextMusic(int soundResId, int instrumentsResId, float volume, bool loop)
    {
        if (soundResId >= 0)
        {
            // If the new music does not loop then we want to continue playing the current music
            // after this new one. For example the jingle when you place the spheres on a base.
            if (_doesCurrentMusicLoop && !loop)
            {
                _prevMusicSoundResId = _nextMusicSoundResId;
                _prevMusicInstrumentsResId = _nextMusicInstrumentsResId;
            }

            // Set the next music to be played
            _nextMusicSoundResId = soundResId;
            _nextMusicInstrumentsResId = instrumentsResId;
            
            // Set parameters
            _doesCurrentMusicLoop = loop;
            _currentMusicVolume = volume;
        }
    }

    private void PlayMusic(int soundResId, int soundInstrumentsResId)
    {
        // Stop previously playing music since we only want one playing at a time
        if (_soloud.isValidVoiceHandle(_musicVoiceHandle))
            _soloud.stop(_musicVoiceHandle);

        Music music = _musicTable[soundResId];

        // Play, but start paused so we can first set the parameters
        _musicVoiceHandle = _soloud.play(music.XmSound, aPaused: true);

        // Set sound parameters
        //_soloud.setLooping(_musicVoiceHandle, _doesCurrentMusicLoop); // Doesn't work, so ignore
        _soloud.setVolume(_musicVoiceHandle, _currentMusicVolume * (_musicFadeVolume / SoundEngineInterface.MaxVolume) * Engine.LocalConfig.Sound.MusicVolume * MusicVolumeFactor);

        // Un-pause
        _soloud.setPause(_musicVoiceHandle, false);

        _currentMusicSoundResId = soundResId;
        _currentMusicInstrumentsResId = soundInstrumentsResId;
        _nextMusicSoundResId = soundResId;
        _nextMusicInstrumentsResId = soundInstrumentsResId;
    }

    private bool IsMusicPlaying(int soundResId)
    {
        return soundResId == _nextMusicSoundResId && _soloud.isValidVoiceHandle(_musicVoiceHandle);
    }

    private void PlaySoundEffect(int soundResId, float volume, bool loop)
    {
        if (SoundEffectsVolume > 0 && !IsSoundEffectPlaying(soundResId))
        {
            SoundEffect soundEffect = _soundEffectsTable[soundResId];

            // Play, but start paused so we can first set the parameters
            uint handle = _soloud.play(soundEffect.WavSound, aPaused: true);

            // Create a new sound effect instance
            _soundEffectInstances[soundResId] = new SoundEffectInstance()
            {
                SoundResourceId = soundResId,
                Loop = loop,
                Volume = volume,
                Soloud = _soloud,
                SoundEffect = soundEffect,
                VoiceHandle = handle,
            };

            // Set sound parameters
            _soloud.setLooping(handle, loop);
            _soloud.setVolume(handle, volume * (SoundEffectsVolume / SoundEngineInterface.MaxVolume) * Engine.LocalConfig.Sound.SfxVolume * SfxVolumeFactor);

            // Un-pause
            _soloud.setPause(handle, false);
        }
    }

    private bool IsSoundEffectPlaying(int soundResId)
    {
        return _soundEffectInstances.TryGetValue(soundResId, out SoundEffectInstance soundEffectInstance) &&
               soundEffectInstance.IsValid;
    }

    private void StopSoundEffect(int soundResId)
    {
        if (_soundEffectInstances.TryGetValue(soundResId, out SoundEffectInstance soundEffectInstance))
        {
            _soloud.stop(soundEffectInstance.VoiceHandle);
            _soundEffectInstances.Remove(soundResId);
        }
    }

    #endregion

    #region Protected Methods

    protected override void RefreshEventSetImpl()
    {
        // Fade in music if there's no new music to be played
        if (_currentMusicSoundResId == _nextMusicSoundResId && _currentMusicSoundResId >= 0)
        {
            // Increase until we reach the music volume
            _musicFadeVolume += 6;
            if (MusicVolume < _musicFadeVolume)
                _musicFadeVolume = MusicVolume;

            // If the music has stopped playing, it does not loop, and we previously played music, then we go back to playing that music
            if (!_doesCurrentMusicLoop && _prevMusicSoundResId >= 0 && !IsMusicPlaying(_currentMusicSoundResId))
            {
                _nextMusicSoundResId = _prevMusicSoundResId;
                _nextMusicInstrumentsResId = _prevMusicInstrumentsResId;
                _doesCurrentMusicLoop = true;
                _musicFadeVolume = 0;
            }
            // Custom code - Openmpt doesn't support looping for XM audio it seems, so we have to manually loop
            else if (_doesCurrentMusicLoop && !IsMusicPlaying(_currentMusicSoundResId))
            {
                PlayMusic(_nextMusicSoundResId, _nextMusicInstrumentsResId);
            }
        }
        // Fade out
        else
        {
            _musicFadeVolume -= 6;

            // Finished fading out, play next music
            if (_musicFadeVolume <= 0)
            {
                _musicFadeVolume = 0;
                PlayMusic(_nextMusicSoundResId, _nextMusicInstrumentsResId);
            }
        }

        // Update music volume
        _soloud.setVolume(_musicVoiceHandle, _currentMusicVolume * (_musicFadeVolume / SoundEngineInterface.MaxVolume) * Engine.LocalConfig.Sound.MusicVolume * MusicVolumeFactor);

        // Update sound effect volumes
        foreach (SoundEffectInstance soundEffectInstance in _soundEffectInstances.Values.ToArray())
        {
            if (soundEffectInstance.IsValid)
            {
                float volume = soundEffectInstance.Volume *
                               (SoundEffectsVolume / SoundEngineInterface.MaxVolume) *
                               Engine.LocalConfig.Sound.SfxVolume;
                _soloud.setVolume(soundEffectInstance.VoiceHandle, volume * SfxVolumeFactor);
            }
            else
            {
                _soundEffectInstances.Remove(soundEffectInstance.SoundResourceId);
            }
        }
    }

    protected override void SetCallBacksImpl(CallBackSet callBacks) { }

    protected override void ProcessEventImpl(short soundEventId, object obj)
    {
        if (soundEventId < 0 || soundEventId >= _soundEvents.Length)
            return;

        NGageSoundEvent evt = _soundEvents[soundEventId];

        if (!evt.IsValid)
            return;

        if (evt.IsMusic)
        {
            if (evt.PlaySong)
                SetNextMusic(evt.SoundResourceId, evt.InstrumentsResourceId, evt.Volume / 7f, evt.Loop);
        }
        else
        {
            if (evt.PlaySong)
                PlaySoundEffect(evt.SoundResourceId, evt.Volume / 7f, evt.Loop);
            else
                StopSoundEffect(evt.SoundResourceId);
        }
    }

    protected override bool IsSongPlayingImpl(short soundEventId)
    {
        if (soundEventId < 0 || soundEventId >= _soundEvents.Length)
            return false;

        NGageSoundEvent evt = _soundEvents[soundEventId];

        if (!evt.IsValid)
            return false;

        if (evt.IsMusic)
            return IsMusicPlaying(soundEventId);
        else
            return IsSoundEffectPlaying(soundEventId);
    }

    protected override void SetSoundPitchImpl(short soundEventId, float pitch) { }
    
    protected override short ReplaceAllSongsImpl(short soundEventId, float fadeOut)
    {
        ProcessEventImpl(soundEventId, null);
        return 0;
    }

    protected override void FinishReplacingAllSongsImpl() { }

    protected override void StopAllSongsImpl() { }

    protected override void PauseAllSongsImpl()
    {
        if (Engine.LocalConfig.Sound.PlayMusicWhenPaused == false)
            IsMusicInGamePaused = true;
    }

    protected override void ResumeAllSongsImpl()
    {
        IsMusicInGamePaused = false;
    }

    protected override float GetVolumeForTypeImpl(SoundType type) => SoundEngineInterface.MaxVolume;

    protected override void SetVolumeForTypeImpl(SoundType type, float newVolume) { }

    protected override void ForcePauseAllSongsImpl()
    {
        IsMusicInEnginePaused = true;

        foreach (SoundEffectInstance soundEffectInstance in _soundEffectInstances.Values)
            soundEffectInstance.InEnginePaused = true;
    }

    protected override void ForceResumeAllSongsImpl()
    {
        IsMusicInEnginePaused = false;

        foreach (SoundEffectInstance soundEffectInstance in _soundEffectInstances.Values)
            soundEffectInstance.InEnginePaused = false;
    }

    protected override void DrawDebugLayoutImpl()
    {
        ImGui.Text($"Music loop: {_doesCurrentMusicLoop}");
        ImGui.Text($"Music volume: {_currentMusicVolume}");
        ImGui.Text($"Music fade volume: {_musicFadeVolume}");
        ImGui.Text($"Previous music: {(!_musicTable.TryGetValue(_prevMusicSoundResId, out Music prevMusic) ? String.Empty : prevMusic.FileName)}");
        ImGui.Text($"Current music: {(!_musicTable.TryGetValue(_currentMusicSoundResId, out Music currentMusic) ? String.Empty : currentMusic.FileName)}");
        ImGui.Text($"Next music: {(!_musicTable.TryGetValue(_nextMusicSoundResId, out Music nextMusic) ? String.Empty : nextMusic.FileName)}");

        if (ImGui.BeginTable("_soundEffects", 4))
        {
            ImGui.TableSetupColumn("Resource", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Volume", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Loop", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            foreach (SoundEffectInstance soundEffectInstance in _soundEffectInstances.Values)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{soundEffectInstance.SoundResourceId}");

                ImGui.TableNextColumn();
                ImGui.Text($"{soundEffectInstance.SoundEffect.FileName}");

                ImGui.TableNextColumn();
                ImGui.Text($"{soundEffectInstance.Volume}");

                ImGui.TableNextColumn();
                ImGui.Text($"{soundEffectInstance.Loop}");
            }

            ImGui.EndTable();
        }
    }

    protected override void UnloadImpl()
    {
        foreach (Music music in _musicTable.Values)
            music.XmSound.Dispose();

        foreach (SoundEffect soundEffect in _soundEffectsTable.Values)
            soundEffect.WavSound.Dispose();

        _soloud.deinit();
        _soloud.Dispose();
    }

    #endregion

    #region Public Methods

    public void PauseLoopingSoundEffects()
    {
        foreach (SoundEffectInstance soundEffectInstance in _soundEffectInstances.Values)
        {
            if (soundEffectInstance.Loop)
                soundEffectInstance.InGamePaused = true;
        }
    }

    public void ResumeLoopingSoundEffects()
    {
        foreach (SoundEffectInstance soundEffectInstance in _soundEffectInstances.Values)
        {
            if (soundEffectInstance.Loop)
                soundEffectInstance.InGamePaused = false;
        }
    }

    #endregion

    #region Data Types

    private class Music
    {
        public Openmpt XmSound { get; init; }
        public string FileName { get; init; }
    }

    private class SoundEffect
    {
        public Wav WavSound { get; init; }
        public string FileName { get; init; }
    }

    private class SoundEffectInstance
    {
        private bool _inGamePaused;
        private bool _inEnginePaused;

        // N-Gage
        public int SoundResourceId { get; init; }
        public bool Loop { get; init; }
        public float Volume { get; init; }

        // Custom
        public bool InGamePaused
        {
            get => _inGamePaused;
            set
            {
                _inGamePaused = value;
                Soloud.setPause(VoiceHandle, value || InEnginePaused);
            }
        }
        public bool InEnginePaused
        {
            get => _inEnginePaused;
            set
            {
                _inEnginePaused = value;
                Soloud.setPause(VoiceHandle, value || InGamePaused);
            }
        }

        // Soloud
        public bool IsValid => Soloud.isValidVoiceHandle(VoiceHandle);
        public Soloud Soloud { get; init; }
        public SoundEffect SoundEffect { get; init; }
        public uint VoiceHandle { get; init; }
    }

    #endregion
}