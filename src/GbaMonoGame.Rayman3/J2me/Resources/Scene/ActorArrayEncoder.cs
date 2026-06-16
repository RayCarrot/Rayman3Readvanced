using System;
using System.IO;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class ActorArrayEncoder : IStreamEncoder
{
    public ActorArrayEncoder(int actorsCount)
    {
        ActorsCount = actorsCount;
    }

    public int ActorsCount { get; }
    public string Name => "ActorArray";

    public void DecodeStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[ActorsCount * 7];
        input.Read(buffer, 0, buffer.Length);

        int offset = 0;
        for (int i = 0; i < ActorsCount; i++)
        {
            byte paramsCount = buffer[offset + ActorsCount * 0];
            output.WriteByte(paramsCount);
            output.WriteByte(buffer[offset + ActorsCount * 1]);
            output.WriteByte(buffer[offset + ActorsCount * 2]);
            output.WriteByte(buffer[offset + ActorsCount * 4]); // Swap to big endian
            output.WriteByte(buffer[offset + ActorsCount * 3]);
            output.WriteByte(buffer[offset + ActorsCount * 6]); // Swap to big endian
            output.WriteByte(buffer[offset + ActorsCount * 5]);
            offset++;

            for (int j = 0; j < paramsCount; j++)
                output.WriteByte((byte)input.ReadByte());
        }
    }

    public void EncodeStream(Stream input, Stream output)
    {
        throw new NotImplementedException();
    }
}