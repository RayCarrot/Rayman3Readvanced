using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct Action
{
    public Action(byte framesCount, byte[] frames)
    {
        FramesCount = framesCount;
        Frames = frames;
    }

    public byte FramesCount { get; }
    public byte[] Frames { get; }

    public static SerializeInto<Action> SerializeInto = (s, x) =>
    {
        byte framesCount = s.Serialize<byte>(x.FramesCount, name: nameof(FramesCount));
        byte[] frames = s.SerializeArray<byte>(x.Frames, framesCount, name: nameof(Frames));

        return new Action(framesCount, frames);
    };
}