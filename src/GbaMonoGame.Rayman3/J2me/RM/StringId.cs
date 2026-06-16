using System;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

// 00_16: Offset
// 16_16: Data array index
public readonly struct StringId : IEquatable<StringId>
{
    public StringId(int value)
    {
        Offset = (short)BitHelpers.ExtractBits(value, 16, 0);
        DataIndex = (short)BitHelpers.ExtractBits(value, 16, 16);
    }

    public StringId(short offset, short dataIndex)
    {
        Offset = offset;
        DataIndex = dataIndex;
    }

    public static StringId Null => new(-1, -1);

    public short DataIndex { get; } // DataArray_Index
    public short Offset { get; } // Offset

    public bool IsNull => DataIndex == -1 && Offset == -1; // 0xFFFFFFFF

    public int GetValue()
    {
        int value = 0;
        value = BitHelpers.SetBits(value, Offset, 16, 0);
        value = BitHelpers.SetBits(value, DataIndex, 16, 16);
        return value;
    }

    public bool Equals(StringId other)
    {
        return DataIndex == other.DataIndex && Offset == other.Offset;
    }

    public override bool Equals(object obj)
    {
        return obj is StringId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DataIndex, Offset);
    }

    public static bool operator ==(StringId left, StringId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StringId left, StringId right)
    {
        return !left.Equals(right);
    }
}