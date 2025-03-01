namespace GbaMonoGame;

/// <summary>
/// Manages tracking elapsed game time.
/// </summary>
public static class GameTime
{
    /// <summary>
    /// Indicates if tracking the game time is currently paused.
    /// </summary>
    public static bool IsPaused { get; set; }

    /// <summary>
    /// The number of elapsed game frames since the game started.
    /// </summary>
    public static uint ElapsedFrames { get; private set; }

    /// <summary>
    /// The total number of elapsed game frames since the game started.
    /// </summary>
    public static uint ElapsedTotalFrames { get; private set; } // N-Gage only

    /// <summary>
    /// Updates the tracked game time by one frame if not paused.
    /// </summary>
    public static void Update()
    {
        if (!IsPaused)
            ElapsedFrames++;

        ElapsedTotalFrames++;
    }

    public static void Reset()
    {
        ElapsedFrames = 0;
        ElapsedTotalFrames = 0;
    }

    public static void Pause()
    {
        IsPaused = true;
    }

    public static void Resume()
    {
        IsPaused = false;
    }
}