using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct ArchiveHeaderEntry
{
    public ArchiveHeaderEntry(short dataSize, short dataSizeCompressionDelta)
    {
        DataSize = dataSize;
        DataSizeCompressionDelta = dataSizeCompressionDelta;
    }

    public short DataSize { get; }
    public short DataSizeCompressionDelta { get; }

    public static SerializeInto<ArchiveHeaderEntry> SerializeInto = (s, x) =>
    {
        short dataSize = s.Serialize<short>(x.DataSize, name: nameof(DataSize));
        short dataSizeCompressionDelta = s.Serialize<short>(x.DataSizeCompressionDelta, name: nameof(DataSizeCompressionDelta));

        return new ArchiveHeaderEntry(dataSize, dataSizeCompressionDelta);
    };
}