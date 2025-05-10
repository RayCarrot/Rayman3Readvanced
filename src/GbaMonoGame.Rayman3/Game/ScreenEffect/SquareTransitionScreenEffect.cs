using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class SquareTransitionScreenEffect : ScreenEffect
{
    public Box Square { get; set; }

    public override void Draw(GfxRenderer renderer)
    {
        renderer.BeginSpriteRender(RenderOptions);

        renderer.DrawFilledRectangle(Vector2.Zero, new Vector2(Square.Left, RenderContext.Resolution.Y), Color.Black); // Left
        renderer.DrawFilledRectangle(new Vector2(Square.Right, 0), new Vector2(RenderContext.Resolution.X - Square.Right, RenderContext.Resolution.Y), Color.Black); // Right
        renderer.DrawFilledRectangle(new Vector2(Square.Left, 0), new Vector2(Square.Size.X, Square.Top), Color.Black); // Top
        renderer.DrawFilledRectangle(new Vector2(Square.Left, Square.Bottom), new Vector2(Square.Size.X, RenderContext.Resolution.Y - Square.Bottom), Color.Black); // Bottom
    }
}