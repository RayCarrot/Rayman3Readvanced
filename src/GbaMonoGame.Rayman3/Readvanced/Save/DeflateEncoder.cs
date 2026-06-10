using System.IO;
using System.IO.Compression;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class DeflateEncoder : IStreamEncoder
{
    public DeflateEncoder(CompressionLevel compressionLevel)
    {
        CompressionLevel = compressionLevel;
    }

    public CompressionLevel CompressionLevel { get; }
    public string Name => "Deflate";

    public void DecodeStream(Stream input, Stream output)
    {
        using DeflateStream deflateStream = new(input, CompressionMode.Decompress, leaveOpen: true);
        deflateStream.CopyTo(output);
    }

    public void EncodeStream(Stream input, Stream output)
    {
        using DeflateStream deflateStream = new(output, CompressionLevel, leaveOpen: true);
        input.CopyTo(deflateStream);
    }
}