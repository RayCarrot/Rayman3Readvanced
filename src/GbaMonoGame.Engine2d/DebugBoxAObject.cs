using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Engine2d;

/// <summary>
/// Custom object for drawing a box in debug view
/// </summary>
public class DebugBoxAObject : AObject
{
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Color Color { get; set; }
    public bool IsFilled { get; set; }

    private void DrawLine(Vector2 point1, Vector2 point2)
    {
        const float thickness = 1;

        float distance = Vector2.Distance(point1, point2);
        float angle = MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X);

        Gfx.AddSprite(new Sprite
        {
            Texture = Gfx.Pixel,
            Position = point1,
            Priority = BgPriority,
            Center = false,
            AffineMatrix = new AffineMatrix(angle, new Vector2(distance, thickness)),
            Color = Color,
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
                Position = Position,
                Priority = BgPriority,
                Center = false,
                AffineMatrix = new AffineMatrix(0, Size),
                Color = new Color(Color, alpha),
                RenderContext = RenderContext,
            });
        }
        else
        {
            DrawLine(Position, Position + new Vector2(Size.X, 0)); // Top
            DrawLine(Position, Position + new Vector2(0, Size.Y)); // Left
            DrawLine(Position + new Vector2(0, Size.Y), Position + Size); // Bottom
            DrawLine(Position + new Vector2(Size.X, 0), Position + Size); // Right
        }
    }
}