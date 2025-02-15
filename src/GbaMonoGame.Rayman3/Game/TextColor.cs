using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public static class TextColor
{
    public static Color LevelSelect { get; } = ColorHelpers.FromRGB555(0xffff);
    public static Color LevelName { get; } = ColorHelpers.FromRGB555(0x73be);
    public static Color LevelNameComplete { get; } = ColorHelpers.FromRGB555(0x03ff);
    public static Color WorldName { get; } = ColorHelpers.FromRGB555(0x77ff);
    public static Color FullWorldName { get; } = ColorHelpers.FromRGB555(0xffff);
    public static Color Story { get; } = ColorHelpers.FromRGB555(0x8aa);
    public static Color TextBox { get; } = ColorHelpers.FromRGB555(0x889);
    public static Color SleepMode { get; } = ColorHelpers.FromRGB555(0x28);
    public static Color Menu { get; } = ColorHelpers.FromRGB555(0x2fd);
    public static Color RaceWrongWayText { get; } = ColorHelpers.FromRGB555(0x7fff);
    public static Color GameCubeMenu { get; } = ColorHelpers.FromRGB555(0xe1f);
    public static Color GameCubeMenuFaded { get; } = ColorHelpers.FromRGB555(0x553);
}