using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Rewrite most text and re-order options
public static class GameOptions
{
    public static GameOptionsGroup[] Create()
    {
        // Get graphics properties
        GraphicsAdapter adapter = Engine.GbaGame.GraphicsDevice.Adapter;
        Vector2 originalRes = Rom.OriginalResolution;
        Vector2 modernRes = Resolution.Modern;
        Vector2 screenRes = new(adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height);
        int windowResCount = Math.Min((int)(screenRes.X / originalRes.X), (int)(screenRes.Y / originalRes.Y));

        // TODO: Finish setting up the options
        return
        [
            new GameOptionsGroup("DISPLAY",
            [
                new MultiSelectionOptionsMenuOption<Language>(
                    text: "LANGUAGE",
                    infoText: "The language to use for any localized text.",
                    items: Localization.GetLanguages().
                        Select(x => new MultiSelectionOptionsMenuOption<Language>.Item(x.EnglishName.ToUpper(), x)).
                        ToArray(),
                    getData: _ => Localization.Language,
                    setData: data =>
                    {
                        Localization.SetLanguage(data);
                        Engine.Config.Display.Language = data.Locale;
                    },
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<DisplayMode>(
                    text: "DISPLAY MODE",
                    infoText: "In borderless mode the resolution can not be changed as it will always use the screen resolution.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<DisplayMode>.Item("WINDOWED", DisplayMode.Windowed),
                        new MultiSelectionOptionsMenuOption<DisplayMode>.Item("FULLSCREEN", DisplayMode.Fullscreen),
                        new MultiSelectionOptionsMenuOption<DisplayMode>.Item("BORDERLESS", DisplayMode.Borderless)
                    ],
                    getData: _ => Engine.GameWindow.DisplayMode,
                    setData: data => Engine.GameWindow.DisplayMode = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<Point>(
                    text: "FULLSCREEN RESOLUTION",
                    infoText: "The resolution to use when in fullscreen mode.",
                    items: adapter.SupportedDisplayModes.
                        Select(x => new MultiSelectionOptionsMenuOption<Point>.Item($"{x.Width}x{x.Height}", new Point(x.Width, x.Height))).
                        ToArray(),
                    getData: _ => Engine.GameWindow.FullscreenResolution,
                    setData: data => Engine.GameWindow.FullscreenResolution = data,
                    getCustomName: data => $"{data.X}x{data.Y}"),
                new MultiSelectionOptionsMenuOption<float>(
                    text: "WINDOW RESOLUTION",
                    infoText: "The resolution factor, based on the internal resolution, to use when in windowed mode. You can also freely change the window resolution by resizing the window.",
                    items: Enumerable.Range(1, windowResCount).
                        Select(x => new MultiSelectionOptionsMenuOption<float>.Item($"{x}x", x)).
                        ToArray(),
                    getData: _ => Engine.GameWindow.WindowResolution.ToVector2().X / Engine.InternalGameResolution.X,
                    setData: data => Engine.GameWindow.WindowResolution = (Engine.InternalGameResolution * data).ToPoint(),
                    getCustomName: data => $"{data:0.00}x"),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "LOCK WINDOW ASPECT RATIO",
                    infoText: "Determines if the window, in windowed mode, should automatically resize to fit the game's internal resolution's aspect ratio.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true)
                    ],
                    getData: _ => Engine.Config.Display.LockWindowAspectRatio,
                    setData: data => Engine.Config.Display.LockWindowAspectRatio = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "DISABLE CAMERA SHAKE",
                    infoText: "Disables the camera shaking effect which is used in some parts of the game to indicate a big impact. This can help for people who suffer from motion sickness.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true)
                    ],
                    getData: _ => Engine.Config.Display.DisableCameraShake,
                    setData: data => Engine.Config.Display.DisableCameraShake = data,
                    getCustomName: _ => null),
            ]),
            new GameOptionsGroup("CONTROLS",
            [
                new ControlOptionsMenuOption(
                    text: "UP",
                    input: Input.Gba_Up),
                new ControlOptionsMenuOption(
                    text: "DOWN",
                    input: Input.Gba_Down),
                new ControlOptionsMenuOption(
                    text: "RIGHT",
                    input: Input.Gba_Right),
                new ControlOptionsMenuOption(
                    text: "LEFT",
                    input: Input.Gba_Left),
                new ControlOptionsMenuOption(
                    text: "JUMP/CONFIRM (A)",
                    input: Input.Gba_A),
                new ControlOptionsMenuOption(
                    text: "ATTACK/BACK (B)",
                    input: Input.Gba_B),
                new ControlOptionsMenuOption(
                    text: "BODYSHOT/RIGHT (R)",
                    input: Input.Gba_R),
                new ControlOptionsMenuOption(
                    text: "WALL CLIMB/LEFT (L)",
                    input: Input.Gba_L),
                new ControlOptionsMenuOption(
                    text: "PAUSE/SELECT (START)",
                    input: Input.Gba_Start),
                new ControlOptionsMenuOption(
                    text: "SPECIAL/BACK (SELECT)",
                    input: Input.Gba_Select),
                // TODO: Add other inputs
            ]),
            new GameOptionsGroup("SOUND",
            [
                new VolumeSelectionOptionsMenuOption(
                    text: "MUSIC VOLUME",
                    infoText: "The volume for music.",
                    getVolume: () => Engine.Config.Sound.MusicVolume,
                    setVolume: data => Engine.Config.Sound.MusicVolume = data),
                new VolumeSelectionOptionsMenuOption(
                    text: "SOUND FX VOLUME",
                    infoText: "The volume for sound effects.",
                    getVolume: () => Engine.Config.Sound.SfxVolume,
                    setVolume: data => Engine.Config.Sound.SfxVolume = data),
            ]),
            // TODO: Look into how these work when changed while in a level
            // TODO: Add option to keep all objects enabled, and force it on when in a custom resolution
            new GameOptionsGroup("TWEAKS",
            [
                new PresetSelectionOptionsMenuOption(
                    text: "PRESET",
                    infoText: "ORIGINAL: Tweaks are disabled, making the game behave like the original game.\n" +
                              "READVANCED: Tweaks are enabled, making the game use modern enhancements to improve the experience.",
                    presetItems:
                    [
                        new PresetSelectionOptionsMenuOption.PresetItem("ORIGINAL", TweaksPreset.Original),
                        new PresetSelectionOptionsMenuOption.PresetItem("READVANCED", TweaksPreset.Readvanced),
                    ]),
                new MultiSelectionOptionsMenuOption<Vector2>(
                    text: "INTERNAL RESOLUTION",
                    infoText: "Determines the game's aspect ratio and scale. A higher resolution will result in a higher FOV for the sidescroller levels. For Mode7 levels only the aspect ratio is changed. Note that this does not effect the resolution the game renders at.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<Vector2>.Item($"ORIGINAL ({originalRes.X}x{originalRes.Y})", originalRes, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<Vector2>.Item($"WIDESCREEN ({modernRes.X}x{modernRes.Y})", modernRes, TweaksPreset.Readvanced), // 16:9
                    ],
                    getData: _ => Engine.InternalGameResolution,
                    setData: data =>
                    {
                        Engine.InternalGameResolution = data;
                        Engine.Config.Tweaks.InternalGameResolution = data == originalRes ? null : data;
                        Engine.GameViewPort.UpdateRenderBox();
                    },
                    getCustomName: data => $"{data.X}x{data.Y}"),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "USE EXTENDED BACKGROUNDS",
                    infoText: "Replaces the backgrounds of some levels with extended ones to better fit higher resolution. Doesn't go into effect until the level is restarted.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.UseExtendedBackgrounds,
                    setData: data => Engine.Config.Tweaks.UseExtendedBackgrounds = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "USE GBA EFFECTS ON N-GAGE",
                    infoText: "By default the N-Gage version has fewer visual effects than the GBA version. Using this option you can restore them. Some visual effects won't be changed until a new level is loaded.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.UseGbaEffectsOnNGage,
                    setData: data => Engine.Config.Tweaks.UseGbaEffectsOnNGage = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "PAUSE MENU",
                    infoText: "Determines if the game should use the original or updated pause menu. The updated one provides access to the game options and the ability to exit a level.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("ORIGINAL", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("UPDATED", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.UseModernPauseDialog,
                    setData: data => Engine.Config.Tweaks.UseModernPauseDialog = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "GAME LOGO",
                    infoText: "Determines the game logo used during the intro sequence and menu.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("ORIGINAL", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("READVANCED", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.UseReadvancedLogo,
                    setData: data => Engine.Config.Tweaks.UseReadvancedLogo = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "ALLOW SKIPPING TEXTBOXES",
                    infoText: "If enabled then you can skip textboxes instead of pausing.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.CanSkipTextBoxes,
                    setData: data => Engine.Config.Tweaks.CanSkipTextBoxes = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "FIX BUGS",
                    infoText: "Indicates if you want to play with the bugs in the game fixed or not. Some bugs are always fixed.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.FixBugs,
                    setData: data => Engine.Config.Tweaks.FixBugs = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "ADD PROJECTILES WHEN NEEDED",
                    infoText: "If enabled then new projectile objects will be created in the level if there aren't enough available to show on screen. This helps avoid enemy shots not firing if playing in a higher internal resolution.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.AddProjectilesWhenNeeded,
                    setData: data => Engine.Config.Tweaks.AddProjectilesWhenNeeded = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "SHOW MODE7 WALLS",
                    infoText: "Adds 3D walls to the Mode7 bumper-car levels.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true, TweaksPreset.Readvanced),
                    ],
                    getData: _ => Engine.Config.Tweaks.ShowMode7Walls,
                    setData: data => Engine.Config.Tweaks.ShowMode7Walls = data,
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "ALLOW PROTOTYPE CHEATS",
                    infoText: "The game has various cheat codes that were only available in the prototype builds. If enabled then those cheat codes will be accessible.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false, TweaksPreset.Original),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true),
                    ],
                    getData: _ => Engine.Config.Tweaks.AllowPrototypeCheats,
                    setData: data => Engine.Config.Tweaks.AllowPrototypeCheats = data,
                    getCustomName: _ => null),
            ]),
            // TODO: Add presets (Original, Rebalanced/Readvanced, Custom)
            new GameOptionsGroup("DIFFICULTY",
            [
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "Infinite lives",
                    infoText: "Gives you infinite lives and makes white lums fully restore health instead.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("OFF", false),
                        new MultiSelectionOptionsMenuOption<bool>.Item("ON", true),
                    ],
                    getData: _ => Engine.Config.Difficulty.InfiniteLives,
                    setData: data => Engine.Config.Difficulty.InfiniteLives = data,
                    getCustomName: _ => null),
            ]),
        ];
    }

    public class GameOptionsGroup(string name, OptionsMenuOption[] options)
    {
        public string Name { get; } = name;
        public OptionsMenuOption[] Options { get; } = options;
    }

    public enum TweaksPreset
    {
        Original,
        Readvanced,
    }
}