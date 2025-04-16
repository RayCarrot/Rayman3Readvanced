using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public abstract class SoundEventsManager
{
    #region Public Properties

    public static bool IsLoaded => Current != null;

    // Allow a separate GBA and N-Gage implementation due to them having entirely different sound code
    public static SoundEventsManager Current { get; private set; }

    #endregion

    #region Protected Methods

    protected abstract void RefreshEventSetImpl();
    protected abstract void SetCallBacksImpl(CallBackSet callBacks);
    protected abstract void ProcessEventImpl(short soundEventId, object obj);
    protected abstract bool IsSongPlayingImpl(short soundEventId);
    protected abstract void SetSoundPitchImpl(short soundEventId, float pitch);
    protected abstract short ReplaceAllSongsImpl(short soundEventId, float fadeOut);
    protected abstract void FinishReplacingAllSongsImpl();
    protected abstract void StopAllSongsImpl();
    protected abstract void PauseAllSongsImpl();
    protected abstract void ResumeAllSongsImpl();
    protected abstract float GetVolumeForTypeImpl(SoundType type);
    protected abstract void SetVolumeForTypeImpl(SoundType type, float newVolume);

    protected abstract void ForcePauseAllSongsImpl();
    protected abstract void ForceResumeAllSongsImpl();
    protected abstract void DrawDebugLayoutImpl();

    #endregion

    #region Public Methods

    public static void Load(SoundEventsManager manager)
    {
        SoundEngineInterface.Load();
        Current = manager;
    }

    public static void RefreshEventSet() => Current.RefreshEventSetImpl();

    public static void SetCallBacks(CallBackSet callBacks) => Current.SetCallBacksImpl(callBacks);

    public static void ProcessEvent(Enum soundEventId) => ProcessEvent(soundEventId, null);
    public static void ProcessEvent(short soundEventId) => ProcessEvent(soundEventId, null);
    public static void ProcessEvent(Enum soundEventId, object obj) => ProcessEvent((short)(object)soundEventId, obj);
    public static void ProcessEvent(short soundEventId, object obj) => Current.ProcessEventImpl(soundEventId, obj);

    public static bool IsSongPlaying(Enum soundEventId) => IsSongPlaying((short)(object)soundEventId);
    public static bool IsSongPlaying(short soundEventId) => Current.IsSongPlayingImpl(soundEventId);

    public static void SetSoundPitch(Enum soundEventId, float pitch) => SetSoundPitch((short)(object)soundEventId, pitch);
    public static void SetSoundPitch(short soundEventId, float pitch) => Current.SetSoundPitchImpl(soundEventId, pitch);

    public static short ReplaceAllSongs(Enum soundEventId, float fadeOut) => ReplaceAllSongs((short)(object)soundEventId, fadeOut);
    public static short ReplaceAllSongs(short soundEventId, float fadeOut) => Current.ReplaceAllSongsImpl(soundEventId, fadeOut);

    public static void FinishReplacingAllSongs() => Current.FinishReplacingAllSongsImpl();

    public static void StopAllSongs() => Current.StopAllSongsImpl();
    public static void PauseAllSongs() => Current.PauseAllSongsImpl();
    public static void ResumeAllSongs() => Current.ResumeAllSongsImpl();

    public static float GetVolumeForType(SoundType type) => Current.GetVolumeForTypeImpl(type);
    public static void SetVolumeForType(SoundType type, float newVolume) => Current.SetVolumeForTypeImpl(type, newVolume);

    // Custom
    public static void ForcePauseAllSongs() => Current.ForcePauseAllSongsImpl();
    public static void ForceResumeAllSongs() => Current.ForceResumeAllSongsImpl();
    public static void DrawDebugLayout() => Current.DrawDebugLayoutImpl();

    #endregion
}