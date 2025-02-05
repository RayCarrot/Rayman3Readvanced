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
    public bool IsFilled { get; set; }

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
        if (Size == Vector2.Zero)
            return;

        if (IsFilled)
        {
            DrawBox(ScreenPos, Size, 0.5f);
        }
        else
        {
            DrawBox(ScreenPos, new Vector2(Size.X, 1)); // Top
            DrawBox(ScreenPos, new Vector2(1, Size.Y)); // Left
            DrawBox(ScreenPos + Size, -new Vector2(1, Size.Y)); // Bottom
            DrawBox(ScreenPos + Size, -new Vector2(Size.X, 1)); // Right
        }
    }
}