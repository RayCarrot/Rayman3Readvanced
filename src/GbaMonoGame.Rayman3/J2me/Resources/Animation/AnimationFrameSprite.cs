using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct AnimationFrameSprite
{
    public AnimationFrameSprite(byte module, bool flipX, sbyte xPosition, sbyte yPosition)
    {
        Module = module;
        FlipX = flipX;
        XPosition = xPosition;
        YPosition = yPosition;
    }

    public byte Module { get; }
    public bool FlipX { get; }
    public sbyte XPosition { get; }
    public sbyte YPosition { get; }

    public static SerializeInto<AnimationFrameSprite> SerializeInto = (s, x) =>
    {
        byte module = x.Module;
        bool flipX = x.FlipX;
        s.DoBits<byte>(b =>
        {
            module = b.SerializeBits<byte>(x.Module, 7, name: nameof(Module));
            flipX = b.SerializeBits<bool>(x.FlipX, 1, name: nameof(FlipX));
        });
        sbyte xPosition = s.Serialize<sbyte>(x.XPosition, name: nameof(XPosition));
        sbyte yPosition = s.Serialize<sbyte>(x.YPosition, name: nameof(YPosition));

        return new AnimationFrameSprite(module, flipX, xPosition, yPosition);
    };
}