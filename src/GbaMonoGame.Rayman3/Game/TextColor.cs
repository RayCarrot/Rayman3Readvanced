using BinarySerializer;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public static class TextColor
{
    public static Color LevelSelect { get; } = new RGB555Color(0xffff).ToColor();
    public static Color LevelName { get; } = new RGB555Color(0x73be).ToColor();
    public static Color LevelNameComplete { get; } = new RGB555Color(0x03ff).ToColor();
    public static Color WorldName { get; } = new RGB555Color(0x77ff).ToColor();
    public static Color FullWorldName { get; } = new RGB555Color(0xffff).ToColor();
    public static Color Story { get; } = new RGB555Color(0x8aa).ToColor();
    public static Color TextBox { get; } = new RGB555Color(0x889).ToColor();
    public static Color SleepMode { get; } = new RGB555Color(0x28).ToColor();
    public static Color Menu { get; } = new RGB555Color(0x2fd).ToColor();
    public static Color RaceWrongWayText { get; } = new RGB555Color(0x7fff).ToColor();
    public static Color GameCubeMenu { get; } = new RGB555Color(0xe1f).ToColor();
    public static Color GameCubeMenuFaded { get; } = new RGB555Color(0x553).ToColor();
}