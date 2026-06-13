using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.J2me;

// TODO: Add cheat menu for debug collision and other cheats
// TODO: Add achievements (complete all levels, 100% all levels and a third one?)
public class GameMidlet : Frame
{
    private const float Framerate = 1 / 0.045f;

    // Map GBA inputs to J2ME key codes
    private FrozenDictionary<GbaInput, JAVA_KEY_CODE> InputMapping { get; } = new Dictionary<GbaInput, JAVA_KEY_CODE>()
    {
        [GbaInput.A] = JAVA_KEY_CODE.SOFTKEY3,
        [GbaInput.B] = JAVA_KEY_CODE.SOFTKEY3,
        [GbaInput.Select] = JAVA_KEY_CODE.SOFTKEY2,
        [GbaInput.Start] = JAVA_KEY_CODE.SOFTKEY1,
        [GbaInput.Right] = JAVA_KEY_CODE.RIGHT_ARROW,
        [GbaInput.Left] = JAVA_KEY_CODE.LEFT_ARROW,
        [GbaInput.Up] = JAVA_KEY_CODE.UP_ARROW,
        [GbaInput.Down] = JAVA_KEY_CODE.DOWN_ARROW,
        [GbaInput.R] = JAVA_KEY_CODE.SOFTKEY2,
        [GbaInput.L] = JAVA_KEY_CODE.SOFTKEY1,
    }.ToFrozenDictionary();

    private float _oldFramerate;
    private Vector2 _oldResolution;

    public static Vector2 OriginalResolution { get; } = Resolution.J2me;
    public static Point OriginalIntegerResolution { get; } = Resolution.J2me.ToPoint();
    public static Vector2 ModernResolution { get; } = Resolution.J2meModern;
    public static Point ModernIntegerResolution { get; } = Resolution.J2meModern.ToPoint();

    public static string UserDataDirectoryName => "J2me";

    public static Game Instance_Game { get; set; }
    public static bool bSuspended { get; set; }

    public JavaArchive JavaArchive { get; set; }

    public override void Init()
    {
        // Set rich presence
        Engine.RichPresence.SetPresence("Mobile (J2ME)");

        // Reset previous game state
        Gfx.FadeControl = FadeControl.None;
        Gfx.Fade = AlphaCoefficient.None;
        Engine.Sem.StopAllSongs();

        // Override the resolution
        _oldResolution = Engine.ViewPort.InternalGameResolution;
        Engine.ViewPort.SetInternalGameResolution(Engine.Settings.Local.J2me.InternalGameResolution);

        // Override the framerate
        _oldFramerate = Engine.App.Framerate;
        Engine.App.Framerate = Framerate;

        // Read the game file
        JavaArchive = new JavaArchive(Path.Combine(Engine.UserData.GetDirectory(UserDataDirectoryName), "rayman3.jar"), cache: true);
        Engine.FrameMngr.RegisterDisposableResource(JavaArchive);

        // TODO: Validate the manifest values (version etc.)

        // Start the game
        Instance_Game = new Game(JavaArchive);
        Instance_Game.start();
    }

    public override void UnInit()
    {
        // Stop the game
        Instance_Game.StopSound();
        Instance_Game = null;

        // Clear static values
        Array.Clear(Actor.aniData);
        Array.Clear(Actor.fist_energy);

        // Restore the resolution
        Engine.ViewPort.SetInternalGameResolution(_oldResolution);

        // Restore the framerate
        Engine.App.Framerate = _oldFramerate;
    }

    public override void Step()
    {
        // Update inputs
        foreach (KeyValuePair<GbaInput, JAVA_KEY_CODE> keyValuePair in InputMapping)
        {
            if (Engine.JoyPad.IsButtonJustPressed(keyValuePair.Key))
                Instance_Game.keyPressed(keyValuePair.Value);
            else if (Engine.JoyPad.IsButtonJustReleased(keyValuePair.Key))
                Instance_Game.keyReleased(keyValuePair.Value);
        }

        // Auto-pause if requested
        if (PendingAutoPause)
        {
            PendingAutoPause = false;

            if (Instance_Game.m_gameFrame_curLevel >= Game.LEVEL_WORLD_MAP && 
                Instance_Game.curState == SYS_FRAME_STATE.GAME && 
                !Instance_Game.m_gameFrame_paused)
            {
                Instance_Game.m_gameFrame_paused = true;
                Instance_Game.Menu_SetCurrentPage(MENU_PAGE.PAUSE);
                Instance_Game.pressedKey = GAME_KEY.NONE;
                Instance_Game.StopSound();
            }
        }

        // Update game
        Instance_Game.repaint();

        // Update sounds
        Instance_Game.UpdateSounds();

        // Check if exiting
        if (Instance_Game.m_chGameState == GAME_STATE.EXITING)
            Engine.FrameMngr.SetNextFrame(new ModernMenuAll(InitialMenuPage.Bonus));
    }
}