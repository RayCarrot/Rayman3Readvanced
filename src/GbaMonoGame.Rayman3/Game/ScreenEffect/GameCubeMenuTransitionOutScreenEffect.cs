using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class GameCubeMenuTransitionOutScreenEffect : ScreenEffect
{
    public float Value { get; set; } // 0-80

    public override void Draw(GfxRenderer renderer)
    {
        renderer.BeginRender(new RenderOptions(false, null, RenderContext));

        Vector2 size = new(Value * 1.5f, Value);

        renderer.DrawFilledRectangle(Vector2.Zero, size, Color.Black); // Top-left
        renderer.DrawFilledRectangle(new Vector2(RenderContext.Resolution.X - size.X, 0), size, Color.Black); // Top-right
        renderer.DrawFilledRectangle(new Vector2(0, RenderContext.Resolution.Y - size.Y), size, Color.Black); // Bottom-left
        renderer.DrawFilledRectangle(RenderContext.Resolution - size, size, Color.Black); // Bottom-right
        renderer.DrawFilledRectangle(RenderContext.Resolution / 2 - size, size * 2, Color.Black); // Middle
    }
}