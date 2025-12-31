using System;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class CircleTransitionScreenEffect : ScreenEffect
{
    public int Radius { get; set; }
    public Vector2 CirclePosition { get; set; }

    public void Init(int radius, Vector2 pos)
    {
        Radius = radius;
        CirclePosition = pos;
    }
    
    public override void Draw(GfxRenderer renderer)
    {
        renderer.BeginSpriteRender(RenderOptions);

        Vector2 res = RenderContext.Resolution;

        // Scale the radius
        float radius = Radius;
        float scaleX = res.X / Rom.OriginalResolution.X;
        float scaleY = res.Y / Rom.OriginalResolution.Y;
        radius *= scaleX > scaleY ? scaleX : scaleY;

        // Get the origin point
        Vector2 pos = CirclePosition;
        pos -= new Vector2(radius);

        // Draw the circle
        if (radius != 0)
        {
            for (int y = 0; y < radius; y++)
            {
                float flipY = radius - y - 1;

                // Calculate the width. In the game the width is retrieved from a pre-calculated table
                // (located at 0x0820e9a4 in the EU version), but we can calculate it during runtime.
                float width = (float)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(y, 2));

                // The original game stores the width as an integer, losing precision
                if (!Engine.ActiveConfig.Tweaks.VisualImprovements)
                    width = (int)width;
                
                float lineWidth = radius - width;
             
                // Draw lines
                renderer.DrawLine(pos + new Vector2(0, flipY), lineWidth, 0, Color.Black); // Top-left
                renderer.DrawLine(pos + new Vector2(radius + width, flipY), lineWidth, 0, Color.Black); // Top-right
                renderer.DrawLine(pos + new Vector2(radius + width, radius + y), lineWidth, 0, Color.Black); // Bottom-right
                renderer.DrawLine(pos + new Vector2(0, radius + y), lineWidth, 0, Color.Black); // Bottom-left
            }
        }

        // Draw black around circle to fill screen
        renderer.DrawFilledRectangle(Vector2.Zero, new Vector2(res.X, pos.Y), Color.Black); // Top
        renderer.DrawFilledRectangle(new Vector2(0, pos.Y + radius * 2), new Vector2(res.X, res.Y - (pos.Y + radius * 2)), Color.Black); // Bottom
        renderer.DrawFilledRectangle(new Vector2(0, pos.Y), new Vector2(pos.X, radius * 2), Color.Black); // Left
        renderer.DrawFilledRectangle(new Vector2(pos.X + radius * 2, pos.Y), new Vector2(res.X - (pos.X + radius * 2), radius * 2), Color.Black); // Right
    }
}