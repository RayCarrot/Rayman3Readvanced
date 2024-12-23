using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class OptionsMenu : Menu
{
    public OptionsMenu(GbaGame game)
    {
        Game = game;

        GraphicsAdapter adapter = Game.GraphicsDevice.Adapter;
        Vector2 originalRes = Engine.GameViewPort.OriginalGameResolution;
        Vector2 screenRes = new(adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height);
        int windowResCount = Math.Min((int)(screenRes.X / originalRes.X), (int)(screenRes.Y / originalRes.Y));

        Options =
        [
            #region Display

            // DISPLAY
            new HeaderMenuOption("Display"),

            // Display mode
            new MultiSelectionMenuOption<bool>(
                name: "Display mode",
                items:
                [
                    new MultiSelectionMenuOption<bool>.Item("Windowed", false),
                    new MultiSelectionMenuOption<bool>.Item("Fullscreen", true)
                ],
                getData: _ => Engine.Config.IsFullscreen,
                setData: data =>
                {
                    Engine.Config.IsFullscreen = data;
                    Game.SaveWindowState();
                    Game.ApplyDisplayConfig();
                },
                getCustomName: _ => null),

            // TODO: Don't auto-apply
            // Fullscreen resolution
            new MultiSelectionMenuOption<Point>(
                name: "Fullscreen resolution",
                items: adapter.SupportedDisplayModes.
                    Select(x => new MultiSelectionMenuOption<Point>.Item($"{x.Width} x {x.Height}", new Point(x.Width, x.Height))).
                    ToArray(),
                getData: _ => Engine.Config.FullscreenResolution,
                setData: data =>
                {
                    Engine.Config.FullscreenResolution = data;
                    Game.ApplyDisplayConfig();
                },
                getCustomName: data => $"{data.X} x {data.Y}"),

            // Window resolution
            new MultiSelectionMenuOption<float>(
                name: "Window resolution",
                items: Enumerable.Range(1, windowResCount).
                    Select(x => new MultiSelectionMenuOption<float>.Item($"{x}x", x)).
                    ToArray(),
                getData: _ =>
                {
                    Rectangle windowBounds = Engine.Settings.Platform switch
                    {
                        Platform.GBA => Engine.Config.GbaWindowBounds,
                        Platform.NGage => Engine.Config.NGageWindowBounds,
                        _ => throw new UnsupportedPlatformException()
                    };

                    return windowBounds.Size.ToVector2().X / Engine.GameViewPort.RequestedGameResolution.X;
                },
                setData: data =>
                {
                    Point windowRes = (Engine.GameViewPort.RequestedGameResolution * data).ToPoint();

                    switch (Engine.Settings.Platform)
                    {
                        case Platform.GBA:
                            Engine.Config.GbaWindowBounds = Engine.Config.GbaWindowBounds with { Size = windowRes };
                            break;

                        case Platform.NGage:
                            Engine.Config.NGageWindowBounds = Engine.Config.NGageWindowBounds with { Size = windowRes };
                            break;

                        default:
                            throw new UnsupportedPlatformException();
                    }

                    Game.ApplyDisplayConfig();
                    Game.SaveWindowState();
                },
                getCustomName: data => $"{data:0.00}x"),

            #endregion

            #region Game

            // GAME
            new HeaderMenuOption("Game"),

            // Internal resolution
            new MultiSelectionMenuOption<Point?>(
                name: "Internal resolution",
                items:
                [
                    new MultiSelectionMenuOption<Point?>.Item($"Original ({originalRes.X} x {originalRes.Y})", null),
                    new MultiSelectionMenuOption<Point?>.Item("Widescreen (384 x 216)", new Point(384, 216)), // 16:9
                ],
                getData: _ => Engine.Config.InternalGameResolution,
                setData: data =>
                {
                    Rectangle originalWindowBounds = Engine.Settings.Platform switch
                    {
                        Platform.GBA => Engine.Config.GbaWindowBounds,
                        Platform.NGage => Engine.Config.NGageWindowBounds,
                        _ => throw new UnsupportedPlatformException()
                    };

                    float originalWindowScale = originalWindowBounds.Size.ToVector2().X / Engine.GameViewPort.RequestedGameResolution.X;

                    Engine.Config.InternalGameResolution = data;
                    Engine.GameViewPort.SetRequestedResolution(Engine.Config.InternalGameResolution?.ToVector2());

                    Point newWindowRes = (Engine.GameViewPort.RequestedGameResolution * originalWindowScale).ToPoint();

                    switch (Engine.Settings.Platform)
                    {
                        case Platform.GBA:
                            Engine.Config.GbaWindowBounds = Engine.Config.GbaWindowBounds with { Size = newWindowRes };
                            break;

                        case Platform.NGage:
                            Engine.Config.NGageWindowBounds = Engine.Config.NGageWindowBounds with { Size = newWindowRes };
                            break;

                        default:
                            throw new UnsupportedPlatformException();
                    }

                    Game.ApplyDisplayConfig();
                    Game.SaveWindowState();
                },
                getCustomName: data => data == null ? null : $"{data.Value.X} x {data.Value.Y}"),

            #endregion

            // TODO: Control options

            #region Sound

            // SOUND
            new HeaderMenuOption("Sound"),

            // Music volume
            new VolumeSelectionMenuOption(
                name: "Music volume",
                sampleSongName: game.SampleSongs[SoundType.Music],
                restart: false,
                getVolume: () => Engine.Config.MusicVolume,
                setVolume: data => Engine.Config.MusicVolume = data),

            // Sound effects volume
            new VolumeSelectionMenuOption(
                name: "Sound effects volume",
                sampleSongName: game.SampleSongs[SoundType.Sfx],
                restart: true,
                getVolume: () => Engine.Config.SfxVolume,
                setVolume: data => Engine.Config.SfxVolume = data),

            #endregion

            #region Debug

            // DEBUG
            new HeaderMenuOption("Debug"),

            // Serializer log
            new MultiSelectionMenuOption<bool>(
                name: "Serializer log (requires restart)",
                items:
                [
                    new MultiSelectionMenuOption<bool>.Item("Disabled", false),
                    new MultiSelectionMenuOption<bool>.Item("Enabled", true),
                ],
                getData: _ => Engine.Config.WriteSerializerLog,
                setData: data => Engine.Config.WriteSerializerLog = data,
                getCustomName: _ => null),

            #endregion
        ];

        foreach (MenuOption option in Options)
            option.Init();
    }

    private GbaGame Game { get; }
    private MenuOption[] Options { get; }

    public override void OnExit()
    {
        foreach (MenuOption option in Options)
            option.OnExit();
    }

    public override void Update(MenuManager menu)
    {
        menu.SetColumns(1);
        menu.SetHorizontalAlignment(MenuManager.HorizontalAlignment.Center);

        menu.Text("Options");
        menu.Spacing();

        foreach (MenuOption option in Options)
            option.Update(menu);

        menu.SetColumns(1);
        menu.SetHorizontalAlignment(MenuManager.HorizontalAlignment.Center);

        if (menu.Button("Back"))
            menu.GoBack();
    }
}