using BinarySerializer;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public static class EngineConversionExtensions
{
    public static Vector2 ToVector2(this BinarySerializer.Ubisoft.GbaEngine.Vector2 vector2) => new(vector2.X, vector2.Y);
    public static Color ToColor(this SerializableColor color) => new(color.Red, color.Green, color.Blue);
}