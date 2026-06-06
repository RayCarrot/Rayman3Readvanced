using System;
using System.Collections.Generic;
using BinarySerializer;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.J2ME;

// Replaces javax.microedition.lcdui.Graphics
public class Graphics
{
    public Graphics()
    {
        RenderContext = new Playfield2DRenderContext(null, null);
        RenderOptions = new RenderOptions() { RenderContext = RenderContext };
        Sprites = [];
    }

    public Point Resolution => RenderContext.Resolution.ToFloorPoint();
    public bool IsResolutionModified => RenderContext.Resolution != GameMidlet.OriginalResolution;
    public Playfield2DRenderContext RenderContext { get; }
    public RenderOptions RenderOptions { get; }
    public List<Sprite> Sprites { get; }
    public Rectangle Clip { get; set; }
    public Color Color { get; set; }
    public Font Font { get; set; }

    private static int AnchorX(int x, int width, ANCHOR anchor)
    {
        int anchoredX = x;
        if ((anchor & ANCHOR.HCENTER) != 0)
            anchoredX = x - width / 2;

        if ((anchor & ANCHOR.RIGHT) != 0)
            anchoredX = x - width;

        if ((anchor & ANCHOR.LEFT) != 0)
            anchoredX = x;

        return anchoredX;
    }

    private static int AnchorY(int y, int height, ANCHOR anchor)
    {
        int anchoredY = y;
        if ((anchor & ANCHOR.VCENTER) != 0)
            anchoredY = y - height / 2;

        if ((anchor & ANCHOR.TOP) != 0)
            anchoredY = y;

        if ((anchor & ANCHOR.BOTTOM) != 0)
            anchoredY = y - height;

        if ((anchor & ANCHOR.BASELINE) != 0)
            anchoredY = y + height;

        return anchoredY;
    }

    public void setClip(int x, int y, int width, int height)
    {
        Clip = new Rectangle(x, y, width, height);
    }

    public void setColor(int RGB)
    {
        byte b = (byte)BitHelpers.ExtractBits(RGB, 8, 0);
        byte g = (byte)BitHelpers.ExtractBits(RGB, 8, 8);
        byte r = (byte)BitHelpers.ExtractBits(RGB, 8, 16);
        Color = new Color(r, g, b);
    }

    public void setFont(Font font)
    {
        Font = font;
    }

    public void fillRect(int x, int y, int width, int height)
    {
        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = Gfx.Pixel;
        sprite.Color = Color;
        sprite.Position = new Vector2(x, y);
        sprite.AffineMatrix = new AffineMatrix(0, new Vector2(width, height));
        sprite.RenderOptions = RenderOptions;
        Sprites.Add(sprite);
    }

    public void fillRoundRect(int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        // TODO: Implement arc
        fillRect(x, y, width, height);
    }

    public void drawImage(Texture2D img, int x, int y, ANCHOR anchor)
    {
        x = AnchorX(x, img.Width, anchor);
        y = AnchorY(y, img.Height, anchor);

        Rectangle clip = Clip;
        clip.X = Math.Clamp(clip.X - x, 0, img.Width);
        clip.Y = Math.Clamp(clip.Y - y, 0, img.Height);
        clip.Width = Math.Min(clip.Width, clip.X + img.Width);
        clip.Height = Math.Min(clip.Height, clip.Y + img.Height);

        x += clip.X;
        y += clip.Y;

        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = img;
        sprite.Position = new Vector2(x, y);
        sprite.RenderOptions = RenderOptions;
        sprite.TextureRectangle = clip;
        Sprites.Add(sprite);
    }

    public void drawRegion(Texture2D src, int x_src, int y_src, int width, int height, TRANS transform, int x_dest, int y_dest, ANCHOR anchor)
    {
        if (transform != TRANS.MIRROR && transform != TRANS.NONE)
            throw new InvalidOperationException("Only mirror and none transforms are supported");

        x_dest = AnchorX(x_dest, src.Width, anchor);
        y_dest = AnchorY(y_dest, src.Height, anchor);

        Rectangle clip = new(x_src, y_src, width, height);

        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = src;
        sprite.Position = new Vector2(x_dest, y_dest);
        sprite.RenderOptions = RenderOptions;
        sprite.TextureRectangle = clip;
        sprite.FlipX = transform == TRANS.MIRROR;
        Sprites.Add(sprite);
    }

    public void drawString(string str, int x, int y, ANCHOR anchor)
    {
        // Get the text size
        int width = Font.stringWidth(str);
        int height = Font.getHeight();

        // Get the original position
        Vector2 originalPos = new(
            x: AnchorX(x, width, anchor), 
            y: AnchorY(y, height, anchor));

        // Create the transformation of the position
        Vector2 origin = new(0, 0);
        Matrix transformation = FontManager.CreateTextTransformation(originalPos, Vector2.One, origin);

        // TODO: Avoid allocating every frame - same with reading the string as well as getting the width
        // Draw each character
        Vector2 pos = Vector2.Zero;
        foreach (byte c in Engine.Font.GetTextBytes(str))
        {
            Sprites.Add(Engine.Font.GetCharacterSprite(
                c: c,
                fontSize: Font.Size,
                transformation: transformation,
                position: ref pos,
                priority: 0,
                affineMatrix: null,
                alpha: AlphaCoefficient.Max,
                color: Color,
                renderOptions: RenderOptions));
        }
    }

    // Custom
    public void ForceOriginalResolution()
    {
        RenderContext.SetFixedResolution(GameMidlet.OriginalResolution);
    }

    public void SetMaxResolution(float width, float height)
    {
        RenderContext.MinResolution = null;
        RenderContext.MaxResolution = new Vector2(width, height);
    }

    public void ClearScreen(int color)
    {
        setClip(0, 0, Resolution.X, Resolution.Y);
        setColor(color);
        fillRect(0, 0, Resolution.X, Resolution.Y);
    }

    public void DrawTexture(Texture2D texture, int x, int y)
    {
        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = texture;
        sprite.Position = new Vector2(x, y);
        sprite.RenderOptions = RenderOptions;
        Sprites.Add(sprite);
    }

    public void DrawGfx()
    {
        // Reverse order
        for (int i = Sprites.Count - 1; i >= 0; i--)
            Gfx.AddSprite(Sprites[i], SpriteType.Default);    

        Sprites.Clear();
    }
}