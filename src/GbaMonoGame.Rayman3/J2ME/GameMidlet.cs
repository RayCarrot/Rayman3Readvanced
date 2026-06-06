using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.J2ME;

// TODO: Remove magic values
// TODO: Add debug layout for it
// TODO: Widescreen
// TODO: Remove game's exception handling?
// TODO: Fix sprite rendering by using separate textures for each sprite
// TODO: Look into other versions of the game to see differences, check code etc.
// TODO: Implement auto-pause on lost focus
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

    public static Vector2 OriginalResolution => new(240, 320);
    public static Point OriginalIntegerResolution => new(240, 320);

    public static Game Instance_Game { get; set; }
    public static bool bSuspended { get; set; }

    public override void Init()
    {
        // Set rich presence
        Engine.RichPresence.SetPresence("J2ME");

        // Reset previous game state
        Gfx.FadeControl = FadeControl.None;
        Gfx.Fade = AlphaCoefficient.None;
        Engine.Sem.StopAllSongs();

        // Override the resolution
        _oldResolution = Engine.ViewPort.InternalGameResolution;
        Engine.ViewPort.SetInternalGameResolution(OriginalResolution);

        // Override the framerate
        _oldFramerate = Engine.App.Framerate;
        Engine.App.Framerate = Framerate;

        // Start the game
        Instance_Game = new Game();
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

        // Update game
        Instance_Game.repaint();

        // Update sounds
        Instance_Game.UpdateSounds();
    }
}