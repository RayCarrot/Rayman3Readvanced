using System.Diagnostics;

namespace GbaMonoGame;

[DebuggerDisplay("{Value,nq}")]
public readonly struct AlphaCoefficient
{
    public AlphaCoefficient(float value)
    {
        Value = value;
    }

    public const float MaxValue = 1;
    public const float MaxGbaValue = 16;

    public static AlphaCoefficient None { get; } = new(0);
    public static AlphaCoefficient Max { get; } = new(MaxValue);

    public float Value { get; }

    public static AlphaCoefficient FromGbaValue(float coefficient)
    {
        return new AlphaCoefficient(coefficient / MaxGbaValue);
    }

    public static implicit operator AlphaCoefficient(float value) => new(value);
    public static implicit operator float(AlphaCoefficient angle) => angle.Value;
}