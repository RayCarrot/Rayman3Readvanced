using System;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public static class Vector2Extensions
{
    public static Point ToRoundedPoint(this Vector2 point) => new((int)Math.Round(point.X), (int)Math.Round(point.Y));
    public static Point ToFloorPoint(this Vector2 point) => new((int)Math.Floor(point.X), (int)Math.Floor(point.Y));
    public static Point ToCeilingPoint(this Vector2 point) => new((int)Math.Ceiling(point.X), (int)Math.Ceiling(point.Y));

    public static Vector2 ExtendToAspectRatio(this Vector2 vector, Vector2 aspectRatio)
    {
        float oldRatio = vector.X / vector.Y;
        float newRatio = aspectRatio.X / aspectRatio.Y;

        if (newRatio > oldRatio)
            return new Vector2(newRatio * vector.Y, vector.Y);
        else
            return new Vector2(vector.X, 1 / newRatio * vector.X);
    }
}