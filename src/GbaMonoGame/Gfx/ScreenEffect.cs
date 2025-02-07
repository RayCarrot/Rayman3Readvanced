namespace GbaMonoGame;

public abstract class ScreenEffect
{
    public RenderOptions RenderOptions { get; } = new();
    public RenderContext RenderContext => RenderOptions.RenderContext;

    public abstract void Draw(GfxRenderer renderer);
}