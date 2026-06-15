using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public abstract class ArchiveResource : BinarySerializable
{
    public ArchiveHeaderEntry Pre_HeaderEntry { get; set; }
}