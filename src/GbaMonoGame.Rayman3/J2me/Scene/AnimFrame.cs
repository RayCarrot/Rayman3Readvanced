namespace GbaMonoGame.Rayman3.J2me;

public readonly struct AnimFrame
{
    public byte SpritesCount { get; init; }
    public byte FrameDuration { get; init; }
    public Box Box { get; init; }
    public AnimFrameSprite[] Frames { get; init; }
}