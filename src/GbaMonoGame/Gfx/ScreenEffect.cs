namespace GbaMonoGame;

public abstract class ScreenEffect
{
    public RenderContext RenderContext { get; set; }

    public abstract void Draw(GfxRenderer renderer);
}