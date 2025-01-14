using Microsoft.Xna.Framework;

namespace GbaMonoGame.Editor;

public class SolidColorScreenRenderer : IScreenRenderer
{
    public SolidColorScreenRenderer(Vector2 size, Color color)
    {
        Size = size;
        Color = color;
    }

    public Vector2 Size { get; }
    public Color Color { get; }

    public Vector2 GetSize(GfxScreen screen) => Size;

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginRender(new RenderOptions(screen.RenderContext)
        {
            Alpha = screen.IsAlphaBlendEnabled,
        });

        renderer.DrawFilledRectangle(position, Size, Color);
    }
}