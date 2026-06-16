using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class DirectTwinVQResource : ArchiveResource
{
    public byte BlockWidth { get; set; }
    public byte BlockHeight { get; set; }
    public byte BlocksCount { get; set; }
    public byte BlockSize { get; set; } // BlockWidth x BlockHeight
    public byte Width { get; set; }
    public byte Height { get; set; }
    public byte ValuesCount { get; set; }
    public byte[] Data { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        BlockWidth = s.Serialize<byte>(BlockWidth, name: nameof(BlockWidth));
        BlockHeight = s.Serialize<byte>(BlockHeight, name: nameof(BlockHeight));
        BlocksCount = s.Serialize<byte>(BlocksCount, name: nameof(BlocksCount));
        BlockSize = s.Serialize<byte>(BlockSize, name: nameof(BlockSize));
        Width = s.Serialize<byte>(Width, name: nameof(Width));
        Height = s.Serialize<byte>(Height, name: nameof(Height));
        ValuesCount = s.Serialize<byte>(ValuesCount, name: nameof(ValuesCount));
        Data = s.SerializeArray<byte>(Data, Pre_HeaderEntry.DataSize - 7, name: nameof(Data));
    }
}