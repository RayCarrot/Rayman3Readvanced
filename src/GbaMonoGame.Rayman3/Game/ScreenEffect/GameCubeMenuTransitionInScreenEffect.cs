using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class GameCubeMenuTransitionInScreenEffect : ScreenEffect
{
    public float Value { get; set; } // 0-240

    public override void Draw(GfxRenderer renderer)
    {
        renderer.BeginSpriteRender(RenderOptions);

        // 3 rects with heights 54, 52 and 54
        renderer.DrawFilledRectangle(Vector2.Zero, new Vector2(RenderContext.Resolution.X - Value, 54), Color.Black);
        renderer.DrawFilledRectangle(new Vector2(Value, 54), new Vector2(RenderContext.Resolution.X - Value, 52), Color.Black);
        renderer.DrawFilledRectangle(new Vector2(0, 54 + 52), new Vector2(RenderContext.Resolution.X - Value, 54), Color.Black);
    }
}