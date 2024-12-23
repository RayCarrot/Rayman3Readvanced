using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class SquareTransitionScreenEffect : ScreenEffect
{
    public Box Square { get; set; }

    public override void Draw(GfxRenderer renderer)
    {
        renderer.BeginRender(new RenderOptions(false, null, RenderContext));

        renderer.DrawFilledRectangle(Vector2.Zero, new Vector2(Square.MinX, RenderContext.Resolution.Y), Color.Black); // Left
        renderer.DrawFilledRectangle(new Vector2(Square.MaxX, 0), new Vector2(RenderContext.Resolution.X - Square.MaxX, RenderContext.Resolution.Y), Color.Black); // Right
        renderer.DrawFilledRectangle(new Vector2(Square.MinX, 0), new Vector2(Square.Size.X, Square.MinY), Color.Black); // Top
        renderer.DrawFilledRectangle(new Vector2(Square.MinX, Square.MaxY), new Vector2(Square.Size.X, RenderContext.Resolution.Y - Square.MaxY), Color.Black); // Bottom
    }
}