using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;
using Microsoft.Xna.Framework.Audio;

namespace GbaMonoGame;

// TODO: Read XM music from data instead of using GBA music
// TODO: Implement the full sound engine code, such as fading in/out songs
public class NGageSoundEventsManager : SoundEventsManager
{
    #region Constructor

    public NGageSoundEventsManager(Dictionary<int, string> songTable, NGageSoundEvent[] soundEvents)
    {
        // Load the sound resources
        Dictionary<int, SoundEffect> loadedSounds = new();
        foreach (NGageSoundEvent evt in soundEvents)
        {
            if (!evt.IsValid)
                continue;

            if (loadedSounds.TryGetValue(evt.SoundResourceId, out SoundEffect snd))
            {
                _soundResources[evt.SoundResourceId] = snd;
            }
            else
            {
                // Load music from extracted files
                if (evt.IsMusic)
                {
                    snd = SoundEffect.FromFile($"{songTable[evt.SoundResourceId]}.wav");
                }
                // Load sound effects from game data since it's already .wav data there and the N-Gage version has a few exclusive sounds
                else
                {
                    using Stream sndStream = Rom.LoadResourceStream(evt.SoundResourceId);
                    snd = SoundEffect.FromStream(sndStream);
                }

                snd.Name = songTable[evt.SoundResourceId];
                loadedSounds[evt.SoundResourceId] = snd;
                _soundResources[evt.SoundResourceId] = snd;
            }
        }
    }

    #endregion

    #region Private Fields

    private readonly Dictionary<int, SoundEffect> _soundResources = new();
    private ActiveSong _activeMusic;
    private readonly Dictionary<int, ActiveSong> _activeSoundEffects = new(); // On N-Gage this is max 64 songs, but we don't need that limit

    #endregion

    #region Public Properties

    public float MusicVolume { get; set; } = SoundEngineInterface.MaxVolume;
    public float SfxVolume { get; set; } = SoundEngineInterface.MaxVolume;

    #endregion

    #region Private Methods

    private void CreateSong(NGageSoundEvent evt)
    {
        // TODO: If song does not loop and prev song loops then the game saves it and continues playing when current song stops (see spheres in bad dreams)

        SoundEffect sndEffect = _soundResources[evt.SoundResourceId];
        SoundEffectInstance sndEffectInstance = sndEffect.CreateInstance();

        ActiveSong song = new()
        {
            SoundResourceId = evt.SoundResourceId,
            Volume = (float)evt.Volume / 7,
            IsMusic = evt.IsMusic,
            Loop = evt.Loop,
            SoundEffect = sndEffect,
            SoundInstance = sndEffectInstance
        };

        sndEffectInstance.IsLooped = evt.Loop;

        // Only one music track can play at a time
        if (evt.IsMusic)
        {
            _activeMusic?.SoundInstance.Dispose();
            _activeMusic = song;
        }
        // Only one sound effect of the same type can play at a time
        else
        {
            if (_activeSoundEffects.TryGetValue(evt.SoundResourceId, out ActiveSong existingSong))
                existingSong.SoundInstance.Dispose();

            _activeSoundEffects[evt.SoundResourceId] = song;
        }

        UpdateVolume(song);
        sndEffectInstance.Play();
    }

    private void StopSong(NGageSoundEvent evt)
    {
        if (evt.IsMusic)
        {
            if (_activeMusic != null && _activeMusic.SoundResourceId == evt.SoundResourceId)
            {
                _activeMusic.SoundInstance.Dispose();
                _activeMusic = null;
            }
        }
        else
        {
            if (_activeSoundEffects.TryGetValue(evt.SoundResourceId, out ActiveSong sfx))
            {
                sfx.SoundInstance.Dispose();
                _activeSoundEffects.Remove(evt.SoundResourceId);
            }
        }
    }

    private void UpdateVolume(ActiveSong song)
    {
        float vol = song.Volume;

        if (song.IsMusic)
        {
            vol *= Engine.Config.MusicVolume;
            vol *= MusicVolume / SoundEngineInterface.MaxVolume;
        }
        else
        {
            vol *= Engine.Config.SfxVolume;
            vol *= SfxVolume / SoundEngineInterface.MaxVolume;
        }

        song.SoundInstance.Volume = vol;
    }

    #endregion

    #region Protected Methods

    protected override void RefreshEventSetImpl()
    {
        if (_activeMusic != null)
        {
            UpdateVolume(_activeMusic);

            if (_activeMusic.SoundInstance.State == SoundState.Stopped && !_activeMusic.Loop)
            {
                _activeMusic.SoundInstance.Dispose();
                _activeMusic = null;
            }
        }

        foreach (ActiveSong sfx in _activeSoundEffects.Values.ToArray())
        {
            UpdateVolume(sfx);

            if (sfx.SoundInstance.State == SoundState.Stopped && !sfx.Loop)
            {
                sfx.SoundInstance.Dispose();
                _activeSoundEffects.Remove(sfx.SoundResourceId);
            }
        }
    }

    protected override void SetCallBacksImpl(CallBackSet callBacks) { }

    protected override void ProcessEventImpl(short soundEventId, object obj)
    {
        if (soundEventId < 0 || soundEventId >= Rom.Loader.NGage_SoundEvents.Length)
            return;

        NGageSoundEvent evt = Rom.Loader.NGage_SoundEvents[soundEventId];

        if (evt.PlaySong)
            CreateSong(evt);
        else
            StopSong(evt);
    }

    protected override bool IsSongPlayingImpl(short soundEventId)
    {
        if (soundEventId < 0 || soundEventId >= Rom.Loader.NGage_SoundEvents.Length)
            return false;

        NGageSoundEvent evt = Rom.Loader.NGage_SoundEvents[soundEventId];

        if (evt.IsMusic)
            return _activeMusic != null && _activeMusic.SoundResourceId == evt.SoundResourceId;
        else
            return _activeSoundEffects.ContainsKey(evt.SoundResourceId);
    }

    protected override void SetSoundPitchImpl(short soundEventId, float pitch) { }
    
    protected override short ReplaceAllSongsImpl(short soundEventId, float fadeOut)
    {
        ProcessEventImpl(soundEventId, null);
        return 0;
    }

    protected override void FinishReplacingAllSongsImpl() { }

    protected override void StopAllSongsImpl() { }

    protected override void PauseAllSongsImpl() { }
    
    protected override void ResumeAllSongsImpl() { }

    protected override float GetVolumeForTypeImpl(SoundType type) => SoundEngineInterface.MaxVolume;

    protected override void SetVolumeForTypeImpl(SoundType type, float newVolume) { }

    protected override void ForcePauseAllSongsImpl()
    {
        _activeMusic?.SoundInstance.Pause();

        foreach (ActiveSong sfx in _activeSoundEffects.Values)
            sfx.SoundInstance.Pause();
    }

    protected override void ForceResumeAllSongsImpl()
    {
        _activeMusic?.SoundInstance.Resume();

        foreach (ActiveSong sfx in _activeSoundEffects.Values)
            sfx.SoundInstance.Resume();
    }

    protected override void DrawDebugLayoutImpl()
    {
        if (ImGui.BeginTable("_songs", 4))
        {
            ImGui.TableSetupColumn("Resource", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("State", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            if (_activeMusic != null)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{_activeMusic.SoundResourceId}");

                ImGui.TableNextColumn();
                ImGui.Text($"{_activeMusic.SoundEffect.Name}");

                ImGui.TableNextColumn();
                ImGui.Text($"{_activeMusic.SoundInstance.State}");

                ImGui.TableNextColumn();
                ImGui.Text($"{_activeMusic.SoundEffect.Duration.TotalSeconds:F}");
            }

            foreach (ActiveSong playingSong in _activeSoundEffects.Values)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{playingSong.SoundResourceId}");

                ImGui.TableNextColumn();
                ImGui.Text($"{playingSong.SoundEffect.Name}");

                ImGui.TableNextColumn();
                ImGui.Text($"{playingSong.SoundInstance.State}");

                ImGui.TableNextColumn();
                ImGui.Text($"{playingSong.SoundEffect.Duration.TotalSeconds:F}");
            }

            ImGui.EndTable();
        }
    }

    #endregion

    #region Public Methods

    public void PauseLoopingSoundEffects()
    {
        // TODO: Implement
    }

    public void ResumeLoopingSoundEffects()
    {
        // TODO: Implement
    }

    #endregion

    #region Data Types

    private class ActiveSong
    {
        public int SoundResourceId { get; init; }
        public float Volume { get; init; }
        public bool IsMusic { get; init; }
        public bool Loop { get; set; }

        // MonoGame
        public SoundEffectInstance SoundInstance { get; init; }
        public SoundEffect SoundEffect { get; init; }
    }

    #endregion
}