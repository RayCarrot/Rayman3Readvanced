namespace GbaMonoGame;

// This is the original class used for managing the multiplayer data, like the JoyPad. We don't
// use this with an actual connection, instead it's here to preserve the original code structure.
public abstract class MultiplayerManager
{
    public abstract int MachineId { get; }
    public abstract int PlayersCount { get; }

    public void Init()
    {
        // NOTE: The game allocates the MultiplayerManager here and calls the ctor on MultiJoyPad
        ReInit();
    }

    public abstract void ReInit();
    public abstract bool Step();
    public abstract bool HasReadJoyPads();
    public abstract void FrameProcessed();
    public abstract uint GetMachineTimer();
    public abstract uint GetElapsedTime();
}