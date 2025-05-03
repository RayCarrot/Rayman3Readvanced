using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
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

            // TODO: Add option not to clear cache? Makes loading faster, but uses more memory.
            // Clear cache if loading a new frame
            if (CurrentFrame != NextFrame)
            {
                Engine.TextureCache.Clear();
                Engine.PaletteCache.Clear();
            }

            // Unload contents loaded by the previous frame
            Engine.FrameContentManager.Unload();

            // Initializing a new frame might take longer than 1/60th of a second, so we mark it as a load
            Engine.BeginLoad();

            Stopwatch sw = Stopwatch.StartNew();

            CurrentFrame = NextFrame;
            NextFrame.Init();
            NextFrame = null;

            if (Rom.IsLoaded && Rom.Platform == Platform.NGage)
                CurrentFrame.EndOfFrame = false;

            sw.Stop();

            Logger.Info("Loaded new frame in {0} ms", sw.ElapsedMilliseconds);

            // The game doesn't return here, but it always calls VSync in the init function, so this
            // will basically do the same thing. And this way we limit the loading to a single
            // update cycle and have the next continue on as normal.
            return;
        }

        // Refresh sound events
        if (SoundEventsManager.IsLoaded)
            SoundEventsManager.RefreshEventSet();

        if (CurrentFrame == null)
            throw new Exception("No current frame to step into");

        // Step the currently active frame
        CurrentFrame.Step();
        
        // Update the game time by one game frame
        GameTime.Update();
    }
}