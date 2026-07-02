using System;
using System.IO;
using System.Text;
using BinarySerializer;

namespace GbaMonoGame;

public static class WavHelpers
{
    public static double GetLoopPointInSeconds(string fileName)
    {
        Encoding encoding = Encoding.ASCII;

        using FileStream stream = File.OpenRead(fileName);
        using Reader reader = new(stream);

        // Verify header
        if (reader.ReadString(4, encoding) != "RIFF")
            throw new Exception("Invalid sound file format. Not RIFF type.");

        uint length = reader.ReadUInt32();
        length += 8; // For the RIFF header

        if (reader.ReadString(4, encoding) != "WAVE")
            throw new Exception("Invalid sound file format. Not WAVE type.");

        // Enumerate WAVE chunks
        uint sampleRate = 0;
        uint loopPoint = 0;
        while (stream.Position < length)
        {
            string chunkId = reader.ReadString(4, encoding);
            uint chunkSize = reader.ReadUInt32();
            long pos = stream.Position;

            if (chunkId == "fmt ")
            {
                stream.Position += 4;
                sampleRate = reader.ReadUInt32();
            }
            else if (chunkId == "smpl")
            {
                stream.Position += 44;
                loopPoint = reader.ReadUInt32();
            }

            stream.Position = pos + chunkSize;
        }

        if (sampleRate == 0)
            throw new Exception("Invalid sound file format. No sample rate found.");

        return loopPoint / (double)sampleRate;
    }
}