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
        RenderOptions renderOptions = RenderOptions;

        if (alpha < 1)
            renderOptions = renderOptions with { BlendMode = BlendMode.AlphaBlend };

        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = Gfx.Pixel;
        sprite.Position = position;
        sprite.Priority = BgPriority;
        sprite.Center = false;
        sprite.AffineMatrix = new AffineMatrix(0, size);
        sprite.Color = Color;
        sprite.Alpha = alpha;
        sprite.RenderOptions = renderOptions;
        Gfx.AddSprite(sprite);
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