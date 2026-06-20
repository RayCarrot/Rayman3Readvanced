namespace GbaMonoGame.Rayman3.Tests;

public class MockMultiplayerManager : MultiplayerManager
{
    public override int MachineId => 0;
    public override int PlayersCount => 1;

    public override void ReInit() { }
    public override bool Step() => false;
    public override bool HasReadJoyPads() => false;
    public override void FrameProcessed() { }
    public override uint GetMachineTimer() => 0;
    public override uint GetElapsedTime() => 0;
}