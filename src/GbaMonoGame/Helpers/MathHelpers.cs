using System;
using System.Runtime.CompilerServices;

namespace GbaMonoGame;

public static class MathHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Mod(float x, float m)
    {
        float r = x % m;
        return r < 0 ? r + m : r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle256ToRadians(float angle)
    {
        return 2 * MathF.PI * angle / 256f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sin256(float x)
    {
        return x switch
        {
            0 => 0,
            64 => 1,
            128 => 0,
            192 => -1,
            _ => MathF.Sin(2 * MathF.PI * x / 256f)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cos256(float x)
    {
        return x switch
        {
            0 => 1,
            64 => 0,
            128 => -1,
            192 => 0,
            _ => MathF.Cos(2 * MathF.PI * x / 256f)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 DirectionalVector256(float angle)
    {
        return new Vector2(
            x: Cos256(angle),
            y: Sin256(angle));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Rotate256(Vector2 vector, float angle)
    {
        float cos = Cos256(angle);
        float sin = Sin256(angle);

        return new Vector2(
            x: vector.X * cos - vector.Y * sin,
            y: vector.X * sin + vector.Y * cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Atan2_256(float x, float y)
    {
        return MathF.Atan2(y, x) / (2 * MathF.PI) * 256;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Atan2_256(Vector2 vector)
    {
        return Atan2_256(vector.X, vector.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FromFixedPoint(int x)
    {
        return (float)x / 0x10000;
    }
}