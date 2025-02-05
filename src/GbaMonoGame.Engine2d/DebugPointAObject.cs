using System;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Engine2d;

/// <summary>
/// Custom object for drawing a point in debug view
/// </summary>
public class DebugPointAObject : AObject
{
    public float Size { get; set; } = 5;
    public Color Color { get; set; }

    private void DrawBox(Vector2 position, Vector2 size, float alpha = 1)
    {
        Gfx.AddSprite(new Sprite
        {
            Texture = Gfx.Pixel,
            Position = position,
            Priority = BgPriority,
            Center = false,
            AffineMatrix = new AffineMatrix(0, size),
            Color = new Color(Color, alpha),
            Shader = Shader,
            RenderContext = RenderContext,
        });
    }

    public override void Execute(Action<short> soundEventCallback)
    {
        DrawBox(ScreenPos - new Vector2(0, Size), new Vector2(1, Size)); // Top
        DrawBox(ScreenPos - new Vector2(Size, 0), new Vector2(Size, 1)); // Left
        DrawBox(ScreenPos + new Vector2(0, 1), new Vector2(1, Size)); // Bottom
        DrawBox(ScreenPos + new Vector2(1, 0), new Vector2(Size, 1)); // Right
    }
}