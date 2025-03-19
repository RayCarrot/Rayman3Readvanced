using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public static class GameOptions
{
    public static GameOptionsGroup[] Create()
    {
        // Get graphics properties
        GraphicsAdapter adapter = Engine.GbaGame.GraphicsDevice.Adapter;
        Vector2 originalRes = Rom.OriginalResolution;
        Vector2 screenRes = new(adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height);
        int windowResCount = Math.Min((int)(screenRes.X / originalRes.X), (int)(screenRes.Y / originalRes.Y));

        // TODO: Finish setting up the options
        return
        [
            new GameOptionsGroup("DISPLAY",
            [
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
                        Select(x => new MultiSelectionOptionsMenuOption<Point>.Item($"{x.Width} x {x.Height}", new Point(x.Width, x.Height))).
                        ToArray(),
                    getData: _ => Engine.GameWindow.FullscreenResolution,
                    setData: data => Engine.GameWindow.FullscreenResolution = data,
                    getCustomName: data => $"{data.X} x {data.Y}"),
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
                    getData: _ => Engine.Config.LockWindowAspectRatio,
                    setData: data => Engine.Config.LockWindowAspectRatio = data,
                    getCustomName: _ => null),
            ]),
            new GameOptionsGroup("GAME",
            [
                // TODO: Add more game options
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
                        Engine.Config.Language = data.Locale;
                    },
                    getCustomName: _ => null),
                new MultiSelectionOptionsMenuOption<Vector2>(
                    text: "INTERNAL RESOLUTION",
                    infoText: "Determines the game's aspect ratio and scale. A higher resolution will result in a higher FOV for the sidescroller levels. For Mode7 levels only the aspect ratio is changed. Note that this does not effect the resolution the game renders at.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<Vector2>.Item($"ORIGINAL ({originalRes.X} x {originalRes.Y})", originalRes),
                        new MultiSelectionOptionsMenuOption<Vector2>.Item("MODERN (384 x 216)", new Vector2(384, 216)), // 16:9
                    ],
                    getData: _ => Engine.InternalGameResolution,
                    setData: data =>
                    {
                        Engine.InternalGameResolution = data;
                        Engine.Config.InternalGameResolution = data == originalRes ? null : data;
                        Engine.GameViewPort.UpdateRenderBox();
                    },
                    getCustomName: data => $"{data.X} x {data.Y}"),
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "GAME LOGO",
                    infoText: "Determines the game logo used during the intro sequence and menu.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("ORIGINAL", false),
                        new MultiSelectionOptionsMenuOption<bool>.Item("READVANCED", true),
                    ],
                    getData: _ => Engine.Config.UseReadvancedLogo,
                    setData: data => Engine.Config.UseReadvancedLogo = data,
                    getCustomName: _ => null),
                // TODO: If the user changes this while in a level then the pause dialog should be re-created?
                new MultiSelectionOptionsMenuOption<bool>(
                    text: "PAUSE MENU",
                    infoText: "Determines if the game should use the original or updated pause menu. The updated one provides access to the game options and the ability to exit a level.",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<bool>.Item("ORIGINAL", false),
                        new MultiSelectionOptionsMenuOption<bool>.Item("READVANCED", true),
                    ],
                    getData: _ => Engine.Config.UseModernPauseDialog,
                    setData: data => Engine.Config.UseModernPauseDialog = data,
                    getCustomName: _ => null),
            ]),
            new GameOptionsGroup("CONTROLS",
            [
                // TODO: Implement
                new MultiSelectionOptionsMenuOption<object>(
                    text: "TEMP",
                    infoText: "TEMP",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<object>.Item("TEMP", null),
                    ],
                    getData: _ => null,
                    setData: _ => { },
                    getCustomName: _ => null),
            ]),
            new GameOptionsGroup("SOUND",
            [
                new VolumeSelectionOptionsMenuOption(
                    text: "MUSIC VOLUME",
                    infoText: "The volume for music.",
                    getVolume: () => Engine.Config.MusicVolume,
                    setVolume: data => Engine.Config.MusicVolume = data),
                new VolumeSelectionOptionsMenuOption(
                    text: "SOUND FX VOLUME",
                    infoText: "The volume for sound effects.",
                    getVolume: () => Engine.Config.SfxVolume,
                    setVolume: data => Engine.Config.SfxVolume = data),
            ]),
            new GameOptionsGroup("DEBUG",
            [
                // TODO: Implement
                new MultiSelectionOptionsMenuOption<object>(
                    text: "TEMP",
                    infoText: "TEMP",
                    items:
                    [
                        new MultiSelectionOptionsMenuOption<object>.Item("TEMP", null),
                    ],
                    getData: _ => null,
                    setData: _ => { },
                    getCustomName: _ => null),
            ]),
        ];
    }

    public class GameOptionsGroup(string name, OptionsMenuOption[] options)
    {
        public string Name { get; } = name;
        public OptionsMenuOption[] Options { get; } = options;
    }
}