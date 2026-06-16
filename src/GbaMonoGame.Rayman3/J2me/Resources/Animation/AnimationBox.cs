using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct AnimationBox
{
    public AnimationBox(sbyte left, sbyte top, sbyte right, sbyte bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public sbyte Left { get; }
    public sbyte Top { get; }
    public sbyte Right { get; }
    public sbyte Bottom { get; }

    public static SerializeInto<AnimationBox> SerializeInto = (s, x) =>
    {
        sbyte left = s.Serialize<sbyte>(x.Left, name: nameof(Left));
        sbyte top = s.Serialize<sbyte>(x.Top, name: nameof(Top));
        sbyte right = s.Serialize<sbyte>(x.Right, name: nameof(Right));
        sbyte bottom = s.Serialize<sbyte>(x.Bottom, name: nameof(Bottom));

        return new AnimationBox(left, top, right, bottom);
    };
}