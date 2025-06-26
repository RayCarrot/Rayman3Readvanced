namespace GbaMonoGame;

/// <summary>
/// A frame is the most important object in this engine, determining how the game loop is handled. Only one frame
/// instance can be active at once and is used for the current state of the game, such as a level, menu, cutscene etc.
/// </summary>
public abstract class Frame
{
    /// <summary>
    /// Gets the currently active frame.
    /// </summary>
    public static Frame Current => FrameManager.CurrentFrame;

    /// <summary>
    /// Indicates if the frame is scheduled to end.
    /// </summary>
    public bool EndOfFrame { get; set; }

    // N-Gage only - for automatically pausing from outside the game
    public bool BlockPauseFrame { get; set; }
    public bool ForcePauseFrame { get; set; }

    /// <summary>
    /// Initializes the frame. This is called once when the frame is made active.
    /// </summary>
    public virtual void Init() { }

    /// <summary>
    /// UnInitializes the frame. This is called once when a new frame is made active.
    /// </summary>
    public virtual void UnInit() { }

    /// <summary>
    /// Steps the frame. This is called once every game frame when the frame is active.
    /// </summary>
    public abstract void Step();
}