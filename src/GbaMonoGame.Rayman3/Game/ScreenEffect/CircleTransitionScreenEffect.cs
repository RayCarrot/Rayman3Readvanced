using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class CircleTransitionScreenEffect : ScreenEffect
{
    static CircleTransitionScreenEffect()
    {
        // In the game this table is pre-calculated (located at 0x0820e9a4 in the EU version), but we can calculate it during runtime
        _radiusWidthsTable = new byte[MaxRadius][];
        for (int i = 0; i < _radiusWidthsTable.Length; i++)
        {
            int radius = i + 1;

            byte[] widths = new byte[radius];
            _radiusWidthsTable[i] = widths;

            for (int y = 0; y < radius; y++)
                widths[radius - y - 1] = (byte)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(y, 2));
        }
    }

    private const int MaxRadius = 252;

    private static readonly Texture2D[] _cachedCircleTextures = new Texture2D[MaxRadius];
    private static readonly byte[][] _radiusWidthsTable;

    public int Radius { get; set; }
    public Vector2 CirclePosition { get; set; }

    private static Texture2D GetCircleTexture(int radius)
    {
        if (_cachedCircleTextures[radius - 1] == null)
        {
            Texture2D tex = new(Engine.GraphicsDevice, radius, radius);

            Color[] colors = new Color[radius * radius];

            for (int y = 0; y < radius; y++)
                Array.Fill(colors, Color.White, y * radius, radius - _radiusWidthsTable[radius - 1][y]);

            tex.SetData(colors);

            _cachedCircleTextures[radius - 1] = tex;
        }

        return _cachedCircleTextures[radius - 1];
    }

    private void DrawCirclePart(GfxRenderer renderer, Texture2D texture, Vector2 position, bool flipX, bool flipY)
    {
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (flipX)
            spriteEffects |= SpriteEffects.FlipHorizontally;
        if (flipY)
            spriteEffects |= SpriteEffects.FlipVertically;

        renderer.Draw(texture, position, spriteEffects, Color.Black);
    }

    public void Init(int radius, Vector2 pos)
    {
        Radius = radius;
        CirclePosition = pos;
    }
    
    public override void Draw(GfxRenderer renderer)
    {
        renderer.BeginSpriteRender(RenderOptions);

        Vector2 pos = CirclePosition;

        pos -= new Vector2(Radius);

        if (Radius != 0)
        {
            // Draw circle
            Texture2D tex = GetCircleTexture(Radius);
            DrawCirclePart(renderer, tex, pos + new Vector2(0, 0), false, false); // Top-left
            DrawCirclePart(renderer, tex, pos + new Vector2(Radius, 0), true, false); // Top-right
            DrawCirclePart(renderer, tex, pos + new Vector2(0, Radius), false, true); // Bottom-left
            DrawCirclePart(renderer, tex, pos + new Vector2(Radius, Radius), true, true); // Bottom-right
        }

        Vector2 res = RenderContext.Resolution;

        // Draw black around circle to fill screen
        renderer.DrawFilledRectangle(Vector2.Zero, new Vector2(res.X, pos.Y), Color.Black); // Top
        renderer.DrawFilledRectangle(new Vector2(0, pos.Y + Radius * 2), new Vector2(res.X, res.Y - (pos.Y + Radius * 2)), Color.Black); // Bottom
        renderer.DrawFilledRectangle(new Vector2(0, pos.Y), new Vector2(pos.X, Radius * 2), Color.Black); // Left
        renderer.DrawFilledRectangle(new Vector2(pos.X + Radius * 2, pos.Y), new Vector2(res.X - (pos.X + Radius * 2), Radius * 2), Color.Black); // Right
    }
}