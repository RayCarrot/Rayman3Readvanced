using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

// 00_08: Array index
// 08_02: Type
// 10_04: Archive index
// 14-29: Empty
// 29_03: Validation
public readonly struct ResourceId
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

    public int GetValue()
    {
        int value = 0;
        value = BitHelpers.SetBits(value, ResourceIndex, 8, 0);
        value = BitHelpers.SetBits(value, (int)ResourceType, 2, 8);
        value = BitHelpers.SetBits(value, ArchiveIndex, 4, 10);
        value = BitHelpers.SetBits(value, Validation, 3, 29);
        return value;
    }
}