using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct AnimationFrame
{
    public AnimationFrame(byte spritesCount, byte frameDuration, AnimationBox box, AnimationFrameSprite[] sprites)
    {
        SpritesCount = spritesCount;
        FrameDuration = frameDuration;
        Box = box;
        Sprites = sprites;
    }

    public byte SpritesCount { get; }
    public byte FrameDuration { get; }
    public AnimationBox Box { get; }
    public AnimationFrameSprite[] Sprites { get; }

    public static SerializeInto<AnimationFrame> SerializeInto = (s, x) =>
    {
        byte spritesCount = s.Serialize<byte>(x.SpritesCount, name: nameof(SpritesCount));
        byte frameDuration = s.Serialize<byte>(x.FrameDuration, name: nameof(FrameDuration));
        AnimationBox box = s.SerializeInto<AnimationBox>(x.Box, AnimationBox.SerializeInto, name: nameof(Box));
        AnimationFrameSprite[] sprites = s.SerializeIntoArray<AnimationFrameSprite>(x.Sprites, spritesCount, AnimationFrameSprite.SerializeInto, name: nameof(Sprites));

        return new AnimationFrame(spritesCount, frameDuration, box, sprites);
    };
}