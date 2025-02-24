using Microsoft.Xna.Framework;

namespace GbaMonoGame.Engine2d;

public static class DebugBoxColor
{
    // Colors source: https://materialui.co/colors
    public static Color PositionPoint { get; } = new(233, 30, 99); // Pink 500
    public static Color CaptorBox { get; } = new(255, 152, 0); // Orange 500
    public static Color DetectionBox { get; } = new(255, 193, 7); // Amber 500
    public static Color ActionBox { get; } = new(63, 81, 181); // Indigo 500
    public static Color AttackBox { get; } = new(244, 67, 54); // Red 500
    public static Color VulnerabilityBox { get; } = new(76, 175, 80); // Green 500
}