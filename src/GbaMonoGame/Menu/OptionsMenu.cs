using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class OptionsMenu : Menu
{
    public OptionsMenu()
    {
        GraphicsAdapter adapter = Engine.GbaGame.GraphicsDevice.Adapter;
        Vector2 originalRes = Rom.OriginalResolution;
        Vector2 screenRes = new(adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height);
        int windowResCount = Math.Min((int)(screenRes.X / originalRes.X), (int)(screenRes.Y / originalRes.Y));

        Options =
        [
            #region Display

            // DISPLAY
            new HeaderMenuOption("Display"),

            // Display mode
            new MultiSelectionMenuOption<DisplayMode>(
                name: "Display mode",
                items:
                [
                    new MultiSelectionMenuOption<DisplayMode>.Item("Windowed", DisplayMode.Windowed),
                    new MultiSelectionMenuOption<DisplayMode>.Item("Fullscreen", DisplayMode.Fullscreen),
                    new MultiSelectionMenuOption<DisplayMode>.Item("Borderless", DisplayMode.Borderless)
                ],
                getData: _ => Engine.GameWindow.DisplayMode,
                setData: data => Engine.GameWindow.DisplayMode = data,
                getCustomName: _ => null),

            // TODO: Don't auto-apply
            // Fullscreen resolution
            new MultiSelectionMenuOption<Point>(
                name: "Fullscreen resolution",
                items: adapter.SupportedDisplayModes.
                    Select(x => new MultiSelectionMenuOption<Point>.Item($"{x.Width} x {x.Height}", new Point(x.Width, x.Height))).
                    ToArray(),
                getData: _ => Engine.GameWindow.FullscreenResolution,
                setData: data => Engine.GameWindow.FullscreenResolution = data,
                getCustomName: data => $"{data.X} x {data.Y}"),

            // Window resolution
            WindowResolutionMenuOption = new MultiSelectionMenuOption<float>(
                name: "Window resolution",
                items: Enumerable.Range(1, windowResCount).
                    Select(x => new MultiSelectionMenuOption<float>.Item($"{x}x", x)).
                    ToArray(),
                getData: _ => Engine.GameWindow.WindowResolution.ToVector2().X / Engine.Config.InternalGameResolution.X,
                setData: data => Engine.GameWindow.WindowResolution = (Engine.Config.InternalGameResolution * data).ToPoint(),
                getCustomName: data => $"{data:0.00}x"),

            // Window resolution
            new MultiSelectionMenuOption<bool>(
                name: "Lock window aspect ratio",
                items:
                [
                    new MultiSelectionMenuOption<bool>.Item("Off", false),
                    new MultiSelectionMenuOption<bool>.Item("On", true)
                ],
                getData: _ => Engine.Config.LockWindowAspectRatio,
                setData: data => Engine.Config.LockWindowAspectRatio = data,
                getCustomName: _ => null),

            #endregion

            #region Game

            // GAME
            new HeaderMenuOption("Game"),

            // Internal resolution
            new MultiSelectionMenuOption<Vector2>(
                name: "Internal resolution",
                items:
                [
                    new MultiSelectionMenuOption<Vector2>.Item($"Original ({originalRes.X} x {originalRes.Y})", originalRes),
                    new MultiSelectionMenuOption<Vector2>.Item("Modern (384 x 216)", new Vector2(384, 216)), // 16:9
                ],
                getData: _ => Engine.Config.InternalGameResolution,
                setData: data =>
                {
                    float originalWindowScale = Engine.GameWindow.WindowResolution.ToVector2().X / Engine.Config.InternalGameResolution.X;

                    Engine.Config.InternalGameResolution = data;

                    Engine.GameWindow.WindowResolution = (Engine.Config.InternalGameResolution * originalWindowScale).ToPoint();
                    WindowResolutionMenuOption.Init();
                },
                getCustomName: data => $"{data.X} x {data.Y}"),

            #endregion

            // TODO: Control options

            #region Sound

            // SOUND
            new HeaderMenuOption("Sound"),

            // Music volume
            new VolumeSelectionMenuOption(
                name: "Music volume",
                sampleSongName: Engine.GbaGame.SampleSongs[SoundType.Music],
                restart: false,
                getVolume: () => Engine.Config.MusicVolume,
                setVolume: data => Engine.Config.MusicVolume = data),

            // Sound effects volume
            new VolumeSelectionMenuOption(
                name: "Sound effects volume",
                sampleSongName: Engine.GbaGame.SampleSongs[SoundType.Sfx],
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

    private MenuOption[] Options { get; }

    private MultiSelectionMenuOption<float> WindowResolutionMenuOption { get; }

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