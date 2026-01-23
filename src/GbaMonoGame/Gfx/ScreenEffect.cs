namespace GbaMonoGame;

public abstract class ScreenEffect
{
    public RenderOptions RenderOptions { get; set; }

    public RenderContext RenderContext
    {
        get => RenderOptions.RenderContext;
        set => RenderOptions = RenderOptions with { RenderContext = value };
    }
    public BlendMode BlendMode
    {
        get => RenderOptions.BlendMode;
        set => RenderOptions = RenderOptions with { BlendMode = value };
    }

    public abstract void Draw(GfxRenderer renderer);
}