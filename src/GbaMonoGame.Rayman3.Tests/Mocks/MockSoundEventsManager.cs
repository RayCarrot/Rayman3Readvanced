using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Tests;

public class MockSoundEventsManager : SoundEventsManager
{
    public override void RefreshEventSet() { }
    public override void SetCallBacks(CallBackSet callBacks) { }
    public override void ProcessEvent(short soundEventId, object readvancedObject, object originalObject) { }
    public override bool IsSongPlaying(short soundEventId) => false;
    public override void SetSoundPitch(short soundEventId, float pitch) { }
    public override short ReplaceAllSongs(short soundEventId, float fadeOut) => -1;
    public override void FinishReplacingAllSongs() { }
    public override void StopAllSongs() { }
    public override void PauseAllSongs() { }
    public override void ResumeAllSongs() { }
    public override float GetVolumeForType(SoundType type) => SoundEngineInterface.MaxVolume;
    public override void SetVolumeForType(SoundType type, float newVolume) { }
    public override void ForcePauseAllSongs() { }
    public override void ForceResumeAllSongs() { }
    public override void DrawDebugLayout() { }
}