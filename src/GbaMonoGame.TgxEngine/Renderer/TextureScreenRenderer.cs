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
    public PaletteTexture PaletteTexture { get; set; }
    public Rectangle TextureRectangle { get; set; }

    public Vector2 GetSize(GfxScreen screen) => TextureRectangle.Size.ToVector2();

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginRender(new RenderOptions(screen.IsAlphaBlendEnabled, PaletteTexture, screen.RenderContext));

        renderer.Draw(Texture, position, TextureRectangle, color);
    }
}