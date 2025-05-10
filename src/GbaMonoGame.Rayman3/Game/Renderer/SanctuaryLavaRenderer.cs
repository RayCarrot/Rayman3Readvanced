using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class SanctuaryLavaRenderer : IScreenRenderer
{
    public SanctuaryLavaRenderer(Texture2D texture)
    {
        Texture = texture;
    }

    public Texture2D Texture { get; }

    public float SinValue { get; set; }

    public Vector2 GetSize(GfxScreen screen) => new(Texture.Width, Texture.Height);
    public Box GetRenderBox(GfxScreen screen) => new(-48, 0, Texture.Width + 48, Texture.Height);

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginSpriteRender(screen.RenderOptions);

        // Render top part normally
        renderer.Draw(Texture, position, new Rectangle(0, 0, Texture.Width, 160), color);

        // Draw lava line-by-line
        float sin = SinValue;
        float sinFactor = 0;
        for (int y = 160; y < Texture.Height; y++)
        {
            Vector2 offset = new(0, y);

            offset += new Vector2(sinFactor * MathHelpers.Sin256(sin), 0);
            sin++;
            sinFactor = (y - 160) / 2f;

            renderer.Draw(Texture, position + offset, new Rectangle(0, y, Texture.Width, 1), color);
        }
    }
}