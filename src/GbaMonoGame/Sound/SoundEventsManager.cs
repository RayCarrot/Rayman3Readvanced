using System;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public abstract class SoundEventsManager
{
    // Implementation
    public abstract void RefreshEventSet();
    public abstract void SetCallBacks(CallBackSet callBacks);
    public abstract void ProcessEvent(short soundEventId, object readvancedObject, object originalObject);
    public abstract bool IsSongPlaying(short soundEventId);
    public abstract void SetSoundPitch(short soundEventId, float pitch);
    public abstract short ReplaceAllSongs(short soundEventId, float fadeOut);
    public abstract void FinishReplacingAllSongs();
    public abstract void StopAllSongs();
    public abstract void PauseAllSongs();
    public abstract void ResumeAllSongs();
    public abstract float GetVolumeForType(SoundType type);
    public abstract void SetVolumeForType(SoundType type, float newVolume);

    // Custom
    public abstract void ForcePauseAllSongs();
    public abstract void ForceResumeAllSongs();
    public abstract void DrawDebugLayout();
    public abstract void Unload();

    // Helpers
    public void ProcessEvent<T>(T soundEventId) 
        where T : Enum => 
        ProcessEvent(soundEventId, null, null);
    public void ProcessEvent(short soundEventId) => 
        ProcessEvent(soundEventId, null, null);
    public void ProcessEvent<T>(T soundEventId, object readvancedObject) 
        where T : Enum => 
        ProcessEvent(soundEventId, readvancedObject, null);
    public void ProcessEvent(short soundEventId, object readvancedObject) => 
        ProcessEvent(soundEventId, readvancedObject, null);
    public void ProcessEvent<T>(T soundEventId, object readvancedObject, object originalObject) 
        where T : Enum => 
        ProcessEvent(CastTo<short>.From(soundEventId), readvancedObject, originalObject);

    public bool IsSongPlaying<T>(T soundEventId) 
        where T : Enum => 
        IsSongPlaying(CastTo<short>.From(soundEventId));
    public void SetSoundPitch<T>(T soundEventId, float pitch) 
        where T : Enum => 
        SetSoundPitch(CastTo<short>.From(soundEventId), pitch);
    public short ReplaceAllSongs<T>(T soundEventId, float fadeOut) 
        where T : Enum => 
        ReplaceAllSongs(CastTo<short>.From(soundEventId), fadeOut);
}