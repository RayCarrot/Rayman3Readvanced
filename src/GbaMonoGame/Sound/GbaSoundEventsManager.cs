using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;
using SoLoud;

namespace GbaMonoGame;

public class GbaSoundEventsManager : SoundEventsManager
{
    #region Constructor

    public GbaSoundEventsManager(Dictionary<int, string> songFileNames, SoundBank soundBank)
    {
        _soloud = new Soloud();
        _soloud.init();

        _songTable = new Dictionary<int, Song>();
        _soundBank = soundBank;

        _volumePerType = new float[8];
        Array.Fill(_volumePerType, SoundEngineInterface.MaxVolume);
        
        _songInstances = new List<SongInstance>();

        LoadSongs(songFileNames);
    }

    #endregion

    #region Private Fields

    private readonly Soloud _soloud; // TODO: Deinit
    private readonly Dictionary<int, Song> _songTable;
    private readonly SoundBank _soundBank;
    private readonly float[] _volumePerType;
    private readonly List<SongInstance> _songInstances; // On GBA this is max 4 songs, but we don't need that limit
    private CallBackSet _callBacks;
    
    private readonly int[] _rollOffTable =
    [
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x7F, 0x7F, 0x7E, 0x7E, 0x7E, 0x7D, 0x7D, 0x7C,
        0x7C, 0x7C, 0x7B, 0x7B, 0x7A, 0x7A, 0x7A, 0x79, 0x79, 0x78, 0x78, 0x78, 0x77, 0x77, 0x76, 0x76,
        0x76, 0x75, 0x75, 0x74, 0x74, 0x74, 0x73, 0x73, 0x72, 0x72, 0x72, 0x71, 0x71, 0x70, 0x70, 0x70,
        0x6F, 0x6F, 0x6E, 0x6E, 0x6E, 0x6D, 0x6D, 0x6C, 0x6C, 0x6C, 0x6B, 0x6B, 0x6A, 0x6A, 0x6A, 0x69,
        0x69, 0x68, 0x68, 0x68, 0x67, 0x67, 0x66, 0x66, 0x66, 0x65, 0x65, 0x64, 0x64, 0x64, 0x63, 0x63,
        0x62, 0x62, 0x62, 0x61, 0x61, 0x60, 0x60, 0x60, 0x5F, 0x5F, 0x5E, 0x5E, 0x5E, 0x5D, 0x5D, 0x5C,
        0x5C, 0x5C, 0x5B, 0x5B, 0x5A, 0x5A, 0x5A, 0x59, 0x59, 0x58, 0x58, 0x58, 0x57, 0x57, 0x56, 0x56,
        0x56, 0x55, 0x55, 0x54, 0x54, 0x54, 0x53, 0x53, 0x52, 0x52, 0x52, 0x51, 0x51, 0x50, 0x50, 0x50,
        0x4F, 0x4F, 0x4E, 0x4E, 0x4E, 0x4D, 0x4D, 0x4C, 0x4C, 0x4C, 0x4B, 0x4B, 0x4A, 0x4A, 0x4A, 0x49,
        0x49, 0x48, 0x48, 0x48, 0x47, 0x47, 0x46, 0x46, 0x46, 0x45, 0x45, 0x44, 0x44, 0x44, 0x43, 0x43,
        0x42, 0x42, 0x42, 0x41, 0x41, 0x40, 0x40, 0x40, 0x3F, 0x3F, 0x3E, 0x3E, 0x3E, 0x3D, 0x3D, 0x3C,
        0x3C, 0x3C, 0x3B, 0x3B, 0x3A, 0x3A, 0x3A, 0x39, 0x39, 0x38, 0x38, 0x38, 0x37, 0x37, 0x36, 0x36,
        0x36, 0x35, 0x35, 0x34, 0x34, 0x34, 0x33, 0x33, 0x32, 0x32, 0x32, 0x31, 0x31, 0x30, 0x30, 0x30,
        0x2F, 0x2F, 0x2E, 0x2E, 0x2E, 0x2D, 0x2D, 0x2C, 0x2C, 0x2C, 0x2B, 0x2B, 0x2A, 0x2A, 0x2A, 0x29,
        0x29, 0x28, 0x28, 0x28, 0x27, 0x27, 0x26, 0x26, 0x26, 0x25, 0x25, 0x24, 0x24, 0x24, 0x23, 0x23,
        0x22, 0x22, 0x22, 0x21, 0x21, 0x20, 0x20, 0x20, 0x1F, 0x1F, 0x1E, 0x1E, 0x1E, 0x1D, 0x1D, 0x1C,
        0x1C, 0x1C, 0x1B, 0x1B, 0x1A, 0x1A, 0x1A, 0x19, 0x19, 0x18, 0x18, 0x18, 0x17, 0x17, 0x16, 0x16,
        0x16, 0x15, 0x15, 0x14, 0x14, 0x14, 0x13, 0x13, 0x12, 0x12, 0x12, 0x11, 0x11, 0x10, 0x10, 0x10,
        0x0F, 0x0F, 0x0E, 0x0E, 0x0E, 0x0D, 0x0D, 0x0C, 0x0C, 0x0C, 0x0B, 0x0B, 0x0A, 0x0A, 0x0A, 0x09,
        0x09, 0x08, 0x08, 0x08, 0x07, 0x07, 0x06, 0x06, 0x06, 0x05, 0x05, 0x04, 0x04, 0x04, 0x03, 0x00
    ];

    #endregion

    #region Private Methods

    private void LoadSongs(Dictionary<int, string> songFileNames)
    {
        Dictionary<string, Song> loadedSounds = new();
        foreach (var songTableEntry in songFileNames)
        {
            if (songTableEntry.Value == null)
                continue;

            // Check if already loaded
            if (loadedSounds.TryGetValue(songTableEntry.Value, out Song song))
            {
                _songTable[songTableEntry.Key] = song;
            }
            else
            {
                song = new Song
                {
                    WavSound = new Wav(),
                    FileName = songTableEntry.Value
                };

                string fileName = $"{songTableEntry.Value}.wav";

                song.WavSound.load(fileName);
                song.WavSound.setLoopPoint(GetLoopPointInSeconds(fileName));

                loadedSounds[songTableEntry.Value] = song;
                _songTable[songTableEntry.Key] = song;
            }
        }
    }

    private double GetLoopPointInSeconds(string fileName)
    {
        Encoding encoding = Encoding.ASCII;

        using FileStream stream = File.OpenRead(fileName);
        using Reader reader = new(stream);

        // Verify header
        if (reader.ReadString(4, encoding) != "RIFF")
            throw new Exception("Invalid sound file format. Not RIFF type.");

        uint length = reader.ReadUInt32();
        length += 8; // For the RIFF header

        if (reader.ReadString(4, encoding) != "WAVE")
            throw new Exception("Invalid sound file format. Not WAVE type.");

        // Enumerate WAVE chunks
        uint sampleRate = 0;
        uint loopPoint = 0;
        while (stream.Position < length)
        {
            string chunkId = reader.ReadString(4, encoding);
            uint chunkSize = reader.ReadUInt32();
            long pos = stream.Position;

            if (chunkId == "fmt ")
            {
                stream.Position += 4;
                sampleRate = reader.ReadUInt32();
            }
            else if (chunkId == "smpl")
            {
                stream.Position += 44;
                loopPoint = reader.ReadUInt32();
            }

            stream.Position = pos + chunkSize;
        }

        if (sampleRate == 0)
            throw new Exception("Invalid sound file format. No sample rate found.");

        return loopPoint / (double)sampleRate;
    }

    private SoundEvent GetEventFromId(short soundEventId)
    {
        return _soundBank.Events[soundEventId];
    }

    private SoundResource GetSoundResource(ushort resourceId)
    {
        SoundResource res = _soundBank.Resources[resourceId];

        switch (res.Type)
        {
            case SoundResource.ResourceType.Song:
                return res;

            // Unused in Rayman 3
            case SoundResource.ResourceType.Switch:
                throw new InvalidOperationException("Switch resources types are not supported");

            case SoundResource.ResourceType.Random:
                ushort? resId = GetRandomResourceId(res);

                if (resId == null)
                    return null;

                return GetSoundResource(resId.Value);

            default:
                throw new Exception($"Invalid resource type {res.Type}");
        }
    }

    private ushort? GetRandomResourceId(SoundResource res)
    {
        // TODO: Use Random.Shared or game's random implementation? It matters less here than in the game code though.
        int rand = Random.Shared.Next(100);

        for (int i = 0; i < res.ResourceIdsCount; i++)
        {
            if (rand < res.ResourceIdConditions[i])
                return res.ResourceIds[i];
        }

        return null;
    }

    private void CreateSong(short soundEventId, SoundResource res, SoundEvent evt, object obj)
    {
        // NOTE: On GBA only 4 songs can play at once. It checks if there's an available one, or one with lower priority. We however don't need that.

        Song song = _songTable[res.SongTableIndex];

        // Play, but start paused so we can first set the parameters
        uint handle = _soloud.play(song.WavSound, aPaused: true);

        // Create a new song instance
        SongInstance songInstance = new()
        {
            Obj = obj,
            EventId = soundEventId,
            NextSoundEventId = -1,
            Priority = evt.Priority,
            SoundType = evt.SoundType,
            Volume = -1,
            Pan = -1,
            IsPlaying = true,
            IsRollOffEnabled = evt.EnableRollOff,
            IsPanEnabled = evt.EnablePan,
            IsFadingOut = false,
            StopIfNotLooping = false,
            Loop = res.Loop,
            IsMusic = res.IsMusic,
            Soloud = _soloud,
            Song = song,
            VoiceHandle = handle,
        };
        _songInstances.Add(songInstance);

        // Set sound parameters
        _soloud.setLooping(handle, res.Loop);
        UpdateVolumeAndPan(songInstance);

        // Un-pause
        _soloud.setPause(handle, false);
    }

    private void ReplaceSong(short soundEventId, short nextEventId, float fadeOut, object obj)
    {
        bool foundSong = false;

        foreach (SongInstance song in _songInstances)
        {
            if (song.IsPlaying && !song.IsFadingOut && song.EventId == soundEventId && song.Obj == obj)
            {
                if (song.Volume != SoundEngineInterface.MaxVolume)
                    fadeOut = 0;

                song.StopIfNotLooping = false;

                song.Loop = false;
                _soloud.setLooping(song.VoiceHandle, false);

                if (fadeOut == 0)
                {
                    _soloud.stop(song.VoiceHandle);
                }
                else
                {
                    _soloud.fadeVolume(song.VoiceHandle, 0, fadeOut * (1 / 60f)); // TODO: Correctly convert time
                    _soloud.scheduleStop(song.VoiceHandle, fadeOut);
                }

                song.IsFadingOut = true;

                if (!foundSong)
                {
                    foundSong = true;

                    if (nextEventId != -1)
                        song.NextSoundEventId = nextEventId;
                }
            }
        }

        if (!foundSong && nextEventId != -1)
            ProcessEvent(nextEventId, obj);
    }

    private void CalculateRollOffAndPan(Vector2 mikePos, out float rollOffLvl, out float dx, object obj)
    {
        Vector2 objPos = _callBacks.GetObjectPosition(obj);

        Vector2 dist = mikePos - objPos;
        Vector2 absDist = new(Math.Abs(dist.X), Math.Abs(dist.Y));

        float largestDist = absDist.Y < absDist.X ? absDist.X : absDist.Y;
        float rollOffIndex = largestDist + absDist.Y + absDist.X / 2;

        if (rollOffIndex > _rollOffTable.Length - 1)
            rollOffIndex = _rollOffTable.Length - 1;

        rollOffLvl = _rollOffTable[(int)rollOffIndex];
        dx = Math.Clamp(-dist.X / 2, SoundEngineInterface.MinPan, SoundEngineInterface.MaxPan);
    }

    private void UpdateVolumeAndPan(SongInstance songInstance)
    {
        float vol;
        float pan;

        if (songInstance.Obj == null && (songInstance.IsRollOffEnabled || songInstance.IsPanEnabled))
            throw new Exception("Song has roll-off or pan enabled, but no object is set!");

        if (songInstance.IsRollOffEnabled || songInstance.IsPanEnabled)
        {
            Vector2 mikePos = _callBacks.GetMikePosition(songInstance.Obj);
            CalculateRollOffAndPan(mikePos, out vol, out pan, songInstance.Obj);

            if (!songInstance.IsRollOffEnabled)
                vol = SoundEngineInterface.MaxVolume;
            if (!songInstance.IsPanEnabled)
                pan = 0;
        }
        else
        {
            vol = SoundEngineInterface.MaxVolume;
            pan = 0;
        }

        vol *= GetVolumeForType(songInstance.SoundType) / SoundEngineInterface.MaxVolume;

        if (songInstance.SoundType == SoundType.Sfx)
            vol *= Engine.Config.SfxVolume;
        else if (songInstance.SoundType == SoundType.Music)
            vol *= Engine.Config.MusicVolume;

        if (songInstance.Volume != vol || songInstance.Pan != pan)
        {
            _soloud.setVolume(songInstance.VoiceHandle, vol / SoundEngineInterface.MaxVolume);
            _soloud.setPan(songInstance.VoiceHandle, pan switch
            {
                > 0 => pan / SoundEngineInterface.MaxPan,
                < 0 => -pan / SoundEngineInterface.MinPan,
                _ => 0
            });

            songInstance.Volume = vol;
            songInstance.Pan = pan;
        }
    }

    #endregion

    #region Protected Methods

    protected override void RefreshEventSetImpl()
    {
        foreach (SongInstance song in _songInstances.ToArray())
        {
            // Do not refresh songs if they are paused in the engine since that's outside the game's code
            if (song.InEnginePaused)
                continue;

            if (!song.IsPlaying)
                continue;

            if (!_soloud.isValidVoiceHandle(song.VoiceHandle) && (!song.StopIfNotLooping || !song.Loop))
            {
                song.IsPlaying = false;

                if (song.NextSoundEventId != -1)
                    ProcessEvent(song.NextSoundEventId, song.Obj);
            }
            else if (!song.IsFadingOut)
            {
                UpdateVolumeAndPan(song);
            }

            if (!song.IsPlaying)
                _songInstances.Remove(song);
        }
    }
    
    protected override void SetCallBacksImpl(CallBackSet callBacks)
    {
        _callBacks = callBacks;
    }

    protected override void ProcessEventImpl(short soundEventId, object obj)
    {
        SoundEvent evt = GetEventFromId(soundEventId);

        // This should ideally never happen, but it seems that the animation for when Rocky lands tries and play a sound which doesn't exist
        if (evt == null)
            return;

        switch (evt.Type)
        {
            case SoundEvent.SoundEventType.Play:
                SoundResource res = GetSoundResource(evt.ResourceId);

                if (res != null)
                    CreateSong(soundEventId, res, evt, obj);
                break;

            case SoundEvent.SoundEventType.Stop:
                ReplaceSong(evt.StopEventId, -1, evt.FadeOutTime, obj);
                break;

            case SoundEvent.SoundEventType.StopAndGo:
                ReplaceSong(evt.StopEventId, evt.NextEventId, evt.FadeOutTime, obj);
                break;
        }
    }

    protected override bool IsSongPlayingImpl(short soundEventId)
    {
        return _songInstances.Any(x => x.IsPlaying && x.EventId == soundEventId);
    }

    protected override void SetSoundPitchImpl(short soundEventId, float pitch)
    {
        foreach (SongInstance songInstance in _songInstances)
        {
            if (songInstance.IsPlaying && songInstance.EventId == soundEventId)
            {
                // 1 is default in SoLoud, while 0 is default in mp2k. Not sure about the 4096 scaling, but seems to sound correct.
                _soloud.setRelativePlaySpeed(songInstance.VoiceHandle, 1 + pitch / 4096);
            }
        }
    }

    protected override short ReplaceAllSongsImpl(short soundEventId, float fadeOut)
    {
        bool firstSong = true;
        short firstEventId = -1;

        foreach (SongInstance song in _songInstances)
        {
            if (song.IsPlaying && song.Priority == 100)
            {
                if (firstSong)
                {
                    if (!song.IsFadingOut)
                    {
                        ReplaceSong(song.EventId, soundEventId, fadeOut, null);
                        firstEventId = song.EventId;
                    }
                    else
                    {
                        firstEventId = song.NextSoundEventId;
                        song.NextSoundEventId = soundEventId;
                    }

                    firstSong = false;
                }
                else
                {
                    ReplaceSong(song.EventId, -1, fadeOut, null);
                }
            }
        }

        return firstEventId;
    }

    protected override void FinishReplacingAllSongsImpl()
    {
        foreach (SongInstance songInstance in _songInstances.ToArray())
        {
            if (songInstance.IsPlaying && songInstance.IsFadingOut && songInstance.NextSoundEventId != -1)
            {
                _soloud.stop(songInstance.VoiceHandle);
                _songInstances.Remove(songInstance);

                ProcessEvent(songInstance.NextSoundEventId, songInstance.Obj);
            }
        }
    }

    protected override void StopAllSongsImpl()
    {
        foreach (SongInstance songInstance in _songInstances)
            _soloud.stop(songInstance.VoiceHandle);

        _songInstances.Clear();
    }

    protected override void PauseAllSongsImpl()
    {
        foreach (SongInstance playingSong in _songInstances)
        {
            playingSong.StopIfNotLooping = true; // Not actually set here, but always set alongside PauseAll, so might as well do it here
            playingSong.InGamePaused = true;
        }
    }

    protected override void ResumeAllSongsImpl()
    {
        // NOTE: In the original game it seems to not resume sound effects, unless they're looping, in which case they play from the beginning
        foreach (SongInstance playingSong in _songInstances)
        {
            playingSong.StopIfNotLooping = false; // Not actually set here, but always set alongside ResumeAll, so might as well do it here
            playingSong.InGamePaused = false;
        }
    }

    protected override float GetVolumeForTypeImpl(SoundType type)
    {
        if ((byte)type > 7)
            throw new ArgumentOutOfRangeException(nameof(type), type, "Type must be a value between 0-7");

        return _volumePerType[(byte)type];
    }

    protected override void SetVolumeForTypeImpl(SoundType type, float newVolume)
    {
        if ((byte)type > 7)
            throw new ArgumentOutOfRangeException(nameof(type), type, "Type must be a value between 0-7");

        if (newVolume is < 0 or > SoundEngineInterface.MaxVolume)
            throw new ArgumentOutOfRangeException(nameof(newVolume), newVolume, "Volume must be a value between 0-128");

        _volumePerType[(byte)type] = newVolume;
    }

    protected override void ForcePauseAllSongsImpl()
    {
        foreach (SongInstance playingSong in _songInstances)
            playingSong.InEnginePaused = true;
    }

    protected override void ForceResumeAllSongsImpl()
    {
        foreach (SongInstance playingSong in _songInstances)
            playingSong.InEnginePaused = false;
    }
    
    protected override void DrawDebugLayoutImpl()
    {
        if (ImGui.BeginTable("_songs", 4))
        {
            ImGui.TableSetupColumn("Event", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Paused", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Next");
            ImGui.TableHeadersRow();

            foreach (SongInstance songInstance in _songInstances)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{songInstance.EventId}");

                ImGui.TableNextColumn();
                ImGui.Text($"{songInstance.Song.FileName}");

                ImGui.TableNextColumn();
                ImGui.Text($"{_soloud.getPause(songInstance.VoiceHandle)}");

                ImGui.TableNextColumn();
                ImGui.Text($"{songInstance.NextSoundEventId}");
            }

            ImGui.EndTable();
        }
    }

    #endregion

    #region Data Types

    private class Song
    {
        public Wav WavSound { get; init; }
        public string FileName { get; init; }
    }

    private class SongInstance
    {
        private bool _inGamePaused;
        private bool _inEnginePaused;

        public object Obj { get; init; }
        public short EventId { get; init; }
        public short NextSoundEventId { get; set; }
        public int Priority { get; init; }
        public SoundType SoundType { get; init; }

        public float Volume { get; set; }
        public float Pan { get; set; }

        // Flags
        public bool IsPlaying { get; set; }
        public bool IsRollOffEnabled { get; init; }
        public bool IsPanEnabled { get; init; }
        public bool IsFadingOut { get; set; }

        // Music player
        public bool StopIfNotLooping { get; set; }
        public bool Loop { get; set; }
        public bool IsMusic { get; init; }

        // Soloud
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

        public Soloud Soloud { get; init; }
        public Song Song { get; init; }
        public uint VoiceHandle { get; init; }
    }

    #endregion
}