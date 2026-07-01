using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3.J2me;

public class GameMidlet : Frame
{
    private const float Framerate = 1 / 0.045f;

    private static readonly GbaInput[] _allInputs = Enum.GetValues<GbaInput>();

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

        if (Rom.Platform == Platform.GBA)
            Engine.Settings.Local.General.LastPlayedGbaSaveSlot = -1;
        else if (Rom.Platform == Platform.NGage)
            Engine.Settings.Local.General.LastPlayedNGageSaveSlot = -1;
        else
            throw new UnsupportedPlatformException();
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
        foreach (GbaInput input in _allInputs)
        {
            if (Engine.JoyPad.IsButtonJustPressed(input))
                Instance_Game.keyPressed(input);
            else if (Engine.JoyPad.IsButtonJustReleased(input))
                Instance_Game.keyReleased(input);
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

    public override void Pause()
    {
        foreach (MidiSoundInstance soundInstance in Instance_Game.SoundInstances)
            soundInstance.Pause();
    }

    public override void Resume()
    {
        foreach (MidiSoundInstance soundInstance in Instance_Game.SoundInstances)
            soundInstance.Resume();
    }
}