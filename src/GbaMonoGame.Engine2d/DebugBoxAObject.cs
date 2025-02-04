using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Engine2d;

/// <summary>
/// Custom object for drawing a box in debug view
/// </summary>
public class DebugBoxAObject : AObject
{
    public Vector2 Size { get; set; }
    public Color Color { get; set; }
    public float Thickness { get; set; } = 1;
    public bool IsFilled { get; set; }

    private void DrawLine(Vector2 point1, Vector2 point2)
    {
        float distance = Vector2.Distance(point1, point2);
        float angle = MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X);

        Gfx.AddSprite(new Sprite
        {
            Texture = Gfx.Pixel,
            Position = (point1 + point2) * 0.5f,
            Priority = BgPriority,
            Center = true,
            AffineMatrix = new AffineMatrix(angle, new Vector2(distance, Thickness)),
            Color = Color,
            Shader = Shader,
            RenderContext = RenderContext,
        });
    }

    public override void Execute(Action<short> soundEventCallback)
    {
        if (IsFilled)
        {
            const float alpha = 0.5f;

            Gfx.AddSprite(new Sprite
            {
                Texture = Gfx.Pixel,
                Position = ScreenPos,
                Priority = BgPriority,
                Center = false,
                AffineMatrix = new AffineMatrix(0, Size),
                Color = new Color(Color, alpha),
                Shader = Shader,
                RenderContext = RenderContext,
            });
        }
        else
        {
            DrawLine(ScreenPos, ScreenPos + new Vector2(Size.X, 0)); // Top
            DrawLine(ScreenPos, ScreenPos + new Vector2(0, Size.Y)); // Left
            DrawLine(ScreenPos + new Vector2(0, Size.Y), ScreenPos + Size); // Bottom
            DrawLine(ScreenPos + new Vector2(Size.X, 0), ScreenPos + Size); // Right
        }
    }
}