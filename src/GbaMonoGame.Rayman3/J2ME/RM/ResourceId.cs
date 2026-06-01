using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2ME;

// TODO: Use this everywhere it's needed
// Custom helpers
public static class ResourceId
{
    // 00_08: Array index
    // 08_02: Type
    // 10_04: Archive index
    // 14-29: Empty
    // 29_03: Validation

    public static int Create(int arrayIndexGeneral, RESOURCE_TYPE type, int archiveIndex)
    {
        int ResourceID = 0;
        ResourceID = BitHelpers.SetBits(ResourceID, arrayIndexGeneral, 8, 0);
        ResourceID = BitHelpers.SetBits(ResourceID, (int)type, 2, 8);
        ResourceID = BitHelpers.SetBits(ResourceID, archiveIndex, 4, 10);
        ResourceID = BitHelpers.SetBits(ResourceID, 3, 3, 29);
        return ResourceID;
    }

    public static void GetValues(int Param_ResourceID, out int Array_Index_General, out RESOURCE_TYPE Type, out int Archive_Index, out int Validation)
    {
        Array_Index_General = BitHelpers.ExtractBits(Param_ResourceID, 8, 0);
        Type = (RESOURCE_TYPE)BitHelpers.ExtractBits(Param_ResourceID, 2, 8);
        Archive_Index = BitHelpers.ExtractBits(Param_ResourceID, 4, 10);
        Validation = BitHelpers.ExtractBits(Param_ResourceID, 3, 29);
    }
}