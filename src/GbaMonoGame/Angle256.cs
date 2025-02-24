using System.Diagnostics;

namespace GbaMonoGame;

[DebuggerDisplay("{Value,nq}")]
public readonly struct Angle256
{
    public Angle256(float value)
    {
        Value = MathHelpers.Mod(value, Max);
    }

    public const float Max = 256;

    public static Angle256 Zero { get; } = new(0);
    public static Angle256 Half { get; } = new(Max / 2);
    public static Angle256 Quarter { get; } = new(Max / 4);

    public float Value { get; }

    public Angle256 Inverse() => new(Max - Value);
    public Vector2 ToDirectionalVector() => MathHelpers.DirectionalVector256(Value);

    public static Angle256 FromVector(Vector2 vector) => new(MathHelpers.Atan2_256(vector));

    public static Angle256 operator +(Angle256 a, Angle256 b) => new(a.Value + b.Value);
    public static Angle256 operator -(Angle256 a, Angle256 b) => new(a.Value - b.Value);
    public static Angle256 operator +(Angle256 a, float b) => new(a.Value + b);
    public static Angle256 operator -(Angle256 a, float b) => new(a.Value - b);
    public static Angle256 operator +(Angle256 a, int b) => new(a.Value + b);
    public static Angle256 operator -(Angle256 a, int b) => new(a.Value - b);

    public static implicit operator Angle256(float value) => new(value);
    public static implicit operator float(Angle256 angle) => angle.Value;

}