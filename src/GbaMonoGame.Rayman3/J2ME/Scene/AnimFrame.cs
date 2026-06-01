namespace GbaMonoGame.Rayman3.J2ME;

public readonly struct AnimFrame
{
    public sbyte SpritesCount { get; init; }
    public sbyte FrameDuration { get; init; }
    public Box Box { get; init; }
    public AnimFrameSprite[] Frames { get; init; }
}