using Microsoft.Xna.Framework;

namespace GbaMonoGame;

/// <summary>
/// Manages the currently active frame.
/// </summary>
public static class FrameManager
{
    internal static Frame CurrentFrame { get; private set; }
    internal static Frame NextFrame { get; private set; }

    /// <summary>
    /// Sets the next frame to be made active. This will go into effect at the start of the next game frame.
    /// </summary>
    /// <param name="frame">The new frame</param>
    public static void SetNextFrame(Frame frame)
    {
        NextFrame = frame;
    }

    /// <summary>
    /// Reloads the current frame. In the original game this is the equivalent of setting the next frame to the current one.
    /// </summary>
    public static void ReloadCurrentFrame()
    {
        NextFrame = CurrentFrame;
    }

    /// <summary>
    /// Steps the active frame and changes the active frame if scheduled to do so.
    /// </summary>
    public static void Step()
    {
        // Scan for new button inputs
        JoyPad.Scan();

        // The game doesn't clear sprites here, but rather in places such as the animation player.
        // For us this however makes more sense, so we always start each frame fresh.
        Gfx.ClearSprites();

        if (NextFrame != null)
        {
            CurrentFrame?.UnInit();

            // Clear all screens and effects for the new frame. The game doesn't do this, but it
            // makes more sense with how this code is structured.
            Gfx.ClearScreens();
            Gfx.ClearScreenEffect();
            Gfx.Color = Color.White;

            // Remove the resolution bounds
            Engine.GameViewPort.SetResolutionBounds(null, null);

            // TODO: Add option not to clear cache? Makes loading faster, but uses more memory.
            // Clear cache if loading a new frame
            if (CurrentFrame != NextFrame)
            {
                Engine.TextureCache.Clear();
                Engine.PaletteCache.Clear();
            }

            // Initializing a new frame might take longer than 1/60th of a second, so we mark it as a load
            Engine.BeginLoad();

            CurrentFrame = NextFrame;
            NextFrame.Init();
            NextFrame = null;

            // The game doesn't return here, but it always calls VSync in the init function, so this
            // will basically do the same thing. And this way we limit the loading to a single
            // update cycle and have the next continue on as normal.
            return;
        }

        // Refresh sound events
        if (SoundEventsManager.IsLoaded)
            SoundEventsManager.RefreshEventSet();

        // Step the currently active frame
        CurrentFrame?.Step();
        
        // Update the game time by one game frame
        GameTime.Update();
    }
}