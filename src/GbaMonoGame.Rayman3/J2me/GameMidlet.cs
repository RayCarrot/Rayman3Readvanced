using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3.J2me;

// TODO: Add achievements (complete all levels, 100% all levels and a third one?)
// TODO: Option to use normal controls since Up sucks on analog stick for jump
public class GameMidlet : Frame
{
    private const float Framerate = 1 / 0.045f;

    // Map GBA inputs to J2ME key codes
    private FrozenDictionary<GbaInput, JAVA_KEY_CODE> InputMapping { get; } = new Dictionary<GbaInput, JAVA_KEY_CODE>()
    {
        [GbaInput.A] = JAVA_KEY_CODE.SOFTKEY3,
        [GbaInput.B] = JAVA_KEY_CODE.SOFTKEY3,
        [GbaInput.Select] = JAVA_KEY_CODE.STAR,
        [GbaInput.Start] = JAVA_KEY_CODE.SOFTKEY2,
        [GbaInput.Right] = JAVA_KEY_CODE.RIGHT_ARROW,
        [GbaInput.Left] = JAVA_KEY_CODE.LEFT_ARROW,
        [GbaInput.Up] = JAVA_KEY_CODE.UP_ARROW,
        [GbaInput.Down] = JAVA_KEY_CODE.DOWN_ARROW,
        [GbaInput.R] = JAVA_KEY_CODE.SOFTKEY2,
        [GbaInput.L] = JAVA_KEY_CODE.SOFTKEY1,
    }.ToFrozenDictionary();

    private float _oldFramerate;
    private Vector2 _oldResolution;

    public static Game Instance_Game { get; set; }
    public static bool bSuspended { get; set; }

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

        // Initialize the rom
        J2meRom.Init(Rayman3J2meVersion.Rayman3_1_0_3_SonyEricssonS700_240x320);

        // Create the game
        Instance_Game = new Game();
        
        // Preload all images (they get cached after loaded once)
        Instance_Game.RM.LoadAllImages();
        Instance_Game.RM.Synchronize();
        Instance_Game.RM.FreeAllImages();

        // Start the game
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

        // Uninitialize the rom
        J2meRom.UnInit();
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