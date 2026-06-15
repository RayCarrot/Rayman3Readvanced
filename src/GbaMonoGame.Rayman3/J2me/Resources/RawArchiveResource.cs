using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class RawArchiveResource : ArchiveResource
{
    public byte[] Data { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Data = s.SerializeArray<byte>(Data, Pre_HeaderEntry.DataSize, name: nameof(Data));
    }
}