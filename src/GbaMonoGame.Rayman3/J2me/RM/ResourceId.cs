using System;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

// 00_08: Array index
// 08_02: Type
// 10_04: Archive index
// 14-29: Empty
// 29_03: Validation
public readonly struct ResourceId : IEquatable<ResourceId>, ISerializerShortLog
{
    public ResourceId(int value)
    {
        ResourceIndex = (byte)BitHelpers.ExtractBits(value, 8, 0);
        ResourceType = (RESOURCE_TYPE)BitHelpers.ExtractBits(value, 2, 8);
        ArchiveIndex = (byte)BitHelpers.ExtractBits(value, 4, 10);
        Validation = (byte)BitHelpers.ExtractBits(value, 3, 29);
    }

    public ResourceId(byte resourceIndex, RESOURCE_TYPE resourceType, byte archiveIndex)
    {
        ResourceIndex = resourceIndex;
        ResourceType = resourceType;
        ArchiveIndex = archiveIndex;
        Validation = 3;
    }

    public byte ArchiveIndex { get; } // Archive_Index
    public byte ResourceIndex { get; } // Array_Index_General
    public RESOURCE_TYPE ResourceType { get; } // Type
    public byte Validation { get; }

    public bool IsNull => ArchiveIndex == 0xF && ResourceIndex == 0xFF && ResourceType == RESOURCE_TYPE.INVALID && Validation == 0x7; // 0xFFFFFFFF

    public static SerializeInto<ResourceId> SerializeInto = (s, x) =>
    {
        int value = s.Serialize<int>(x.GetValue(), name: "Value");
        return new ResourceId(value);
    };

    public string ShortLog => ToString();

    public int GetValue()
    {
        int value = 0;
        value = BitHelpers.SetBits(value, ResourceIndex, 8, 0);
        value = BitHelpers.SetBits(value, (int)ResourceType, 2, 8);
        value = BitHelpers.SetBits(value, ArchiveIndex, 4, 10);
        value = BitHelpers.SetBits(value, Validation, 3, 29);
        return value;
    }

    public bool Equals(ResourceId other)
    {
        return ArchiveIndex == other.ArchiveIndex && ResourceIndex == other.ResourceIndex && ResourceType == other.ResourceType && Validation == other.Validation;
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ArchiveIndex, ResourceIndex, (int)ResourceType, Validation);
    }

    public static bool operator ==(ResourceId left, ResourceId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceId left, ResourceId right)
    {
        return !left.Equals(right);
    }

    public override string ToString() => IsNull 
        ? "ResourceId(NULL)"
        : $"ResourceId(Archive: {ArchiveIndex}, Resource: {ResourceIndex}, Type: {ResourceType})";
}