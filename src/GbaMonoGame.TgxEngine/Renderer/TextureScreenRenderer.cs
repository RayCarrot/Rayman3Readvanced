﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

public class TextureScreenRenderer : IScreenRenderer
{
    public TextureScreenRenderer(Texture2D texture)
    {
        Texture = texture;
        TextureRectangle = texture.Bounds;
    }

    public Texture2D Texture { get; }
    public Rectangle TextureRectangle { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;

    public Vector2 GetSize(GfxScreen screen) => TextureRectangle.Size.ToVector2();

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginSpriteRender(screen.RenderOptions);

        renderer.Draw(Texture, position, TextureRectangle, 0, Vector2.Zero, Scale, SpriteEffects.None, color);
    }
}