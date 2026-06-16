using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct AnimationModule
{
    public AnimationModule(byte xPosition, byte yPosition, byte width, byte height)
    {
        XPosition = xPosition;
        YPosition = yPosition;
        Width = width;
        Height = height;
    }

    public byte XPosition { get; }
    public byte YPosition { get; }
    public byte Width { get; }
    public byte Height { get; }

    public static SerializeInto<AnimationModule> SerializeInto = (s, x) =>
    {
        byte xPosition = s.Serialize<byte>(x.XPosition, name: nameof(XPosition));
        byte yPosition = s.Serialize<byte>(x.YPosition, name: nameof(YPosition));
        byte width = s.Serialize<byte>(x.Width, name: nameof(Width));
        byte height = s.Serialize<byte>(x.Height, name: nameof(Height));

        return new AnimationModule(xPosition, yPosition, width, height);
    };
}