using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Engine2d;

/// <summary>
/// Custom object for drawing a point in debug view
/// </summary>
public class DebugPointAObject : AObject
{
    public float Size { get; set; } = 10;
    public float Thickness { get; set; } = 1;
    public Color Color { get; set; }

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
        DrawLine(ScreenPos - new Vector2(Size / 2, 0), ScreenPos + new Vector2(Size / 2, 0));
        DrawLine(ScreenPos - new Vector2(0, Size / 2), ScreenPos + new Vector2(0, Size / 2));
    }
}