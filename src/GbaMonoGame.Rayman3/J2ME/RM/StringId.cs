using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2ME;

// TODO: Use this everywhere it's needed
// Custom helpers
public static class StringId
{
    // 00_16: Offset
    // 16_16: Data array index

    public static int Create(int Offset, int DataArray_Index)
    {
        int StringID = 0;
        StringID = BitHelpers.SetBits(StringID, Offset, 16, 0);
        StringID = BitHelpers.SetBits(StringID, DataArray_Index, 16, 16);
        return StringID;
    }

    public static void GetValues(int Param_StringID, out int Offset, out int DataArray_Index)
    {
        Offset = BitHelpers.ExtractBits(Param_StringID, 16, 0);
        DataArray_Index = BitHelpers.ExtractBits(Param_StringID, 16, 16);
    }
}