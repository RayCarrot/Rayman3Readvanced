using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Action = System.Action;

namespace GbaMonoGame;

/// <summary>
/// Manages the currently active frame
/// </summary>
public class FrameManager : IDisposable
{
    private readonly List<IDisposable> _resources = [];
    private readonly List<Action> _stepActions = [];
    private bool _wasActive;

    public Frame CurrentFrame { get; private set; }
    public Frame NextFrame { get; private set; }

    private void DisposeResources()
    {
        foreach (IDisposable disposable in _resources)
            disposable.Dispose();

        _resources.Clear();
    }

    /// <summary>
    /// Sets the next frame to be made active. This will go into effect at the start of the next game frame.
    /// </summary>
    /// <param name="frame">The new frame</param>
    public void SetNextFrame(Frame frame)
    {
        NextFrame = frame;
    }

    /// <summary>
    /// Reloads the current frame. In the original game this is the equivalent of setting the next frame to the current one.
    /// </summary>
    public void ReloadCurrentFrame()
    {
        NextFrame = CurrentFrame;
    }

    public void RegisterDisposableResource(IDisposable resource)
    {
        _resources.Add(resource);
    }

    public void AddStepAction(Action action)
    {
        _stepActions.Add(action);
    }

    public void RemoveStepAction(Action action)
    {
        _stepActions.Remove(action);
    }

    /// <summary>
    /// Steps the active frame and changes the active frame if scheduled to do so.
    /// </summary>
    public void Step()
    {
        // Scan for new button inputs
        Engine.JoyPad.Scan();

        // The game doesn't clear sprites here, but rather in places such as the animation player.
        // For us this however makes more sense, so we always start each frame fresh.
        Gfx.ClearSprites();

        if (NextFrame != null)
        {
            Stopwatch sw = Stopwatch.StartNew();

            CurrentFrame?.UnInit();

            // Clear all screens and effects for the new frame. The game doesn't do this, but it
            // makes more sense with how this code is structured.
            Gfx.ClearScreens();
            Gfx.ClearScreenEffect();
            Gfx.Color = Color.White;
            Gfx.ClearColor = Color.Black;

            // If loading a new frame...
            if (CurrentFrame != NextFrame)
            {
                // Clear cache from previous frame
                Engine.Assets.UnloadFrameCache();

                // Clear serializer cache
                if (Rom.IsLoaded)
                    Rom.Context.Cache.Clear();

                // Dispose resources
                DisposeResources();

                // De-reference previous frame
                CurrentFrame = null;

                // Force clear garbage collection. This is a bit slow (around 15 ms in debug mode), but should help with
                // memory usage and fragmentation for long play sessions.
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }

            // Revert the rich presence to the default idle state (might get overriden when we initialize the new frame)
            Engine.RichPresence.SetIdlePresence();

            // Initializing a new frame might take longer than 1/60th of a second, so we mark it as a load
            Engine.App.BeginLoad();

            CurrentFrame = NextFrame;
            NextFrame.Init();
            NextFrame = null;

            if (Rom.IsLoaded && Rom.Platform == Platform.NGage)
                CurrentFrame.EndOfFrame = false;

            sw.Stop();

            Logger.Info("Loaded new frame of type {0} in {1} ms", CurrentFrame.GetType().Name, sw.ElapsedMilliseconds);

            // The game doesn't return here, but it always calls VSync in the init function, so this
            // will basically do the same thing. And this way we limit the loading to a single
            // update cycle and have the next continue on as normal.
            return;
        }

        // Check if the game was deactivated (window losing focus) and if it should auto-pause
        if (Engine.Settings.Active.Tweaks.PauseOnDeactivation && Engine.App.IsActive != _wasActive)
        {
            _wasActive = Engine.App.IsActive;
            if (!Engine.App.IsActive && !RSMultiplayer.IsActive && !Frame.Current.BlockAutoPause)
                Frame.Current.PendingAutoPause = true;
        }

        // Refresh sound events
        Engine.Sem?.RefreshEventSet();

        if (CurrentFrame == null)
            throw new Exception("No current frame to step into");

        // Step the currently active frame
        CurrentFrame.Step();

        // Invoke custom step actions
        foreach (Action action in _stepActions)
            action();

        // Update the game time by one game frame
        GameTime.Update();
    }

    public void Dispose()
    {
        CurrentFrame = null;
        NextFrame = null;
        DisposeResources();
        _stepActions.Clear();
    }
}