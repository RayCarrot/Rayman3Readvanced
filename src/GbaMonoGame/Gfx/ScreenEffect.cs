namespace GbaMonoGame;

public abstract class ScreenEffect
{
    public RenderContext RenderContext { get; set; } = Engine.GameRenderContext;

    public abstract void Draw(GfxRenderer renderer);
}