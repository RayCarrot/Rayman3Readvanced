using System;
using System.Buffers;
using System.Collections.Generic;
using BinarySerializer;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.J2me;

// Replaces javax.microedition.lcdui.Graphics
public class Graphics
{
    public Graphics()
    {
        RenderContext = new Playfield2DRenderContext(null, null);
        RenderOptions = new RenderOptions() { RenderContext = RenderContext };
        Sprites = [];
    }

    public Vector2 Resolution => RenderContext.Resolution;
    public bool IsResolutionModified => RenderContext.Resolution != J2meRom.OriginalResolution;
    public Playfield2DRenderContext RenderContext { get; }
    public RenderOptions RenderOptions { get; }
    public List<Sprite> Sprites { get; }
    public GbaMonoGame.Box Clip { get; set; }
    public Color Color { get; set; }
    public Font Font { get; set; }
    public int Priority { get; set; } = 1;

    private static float AnchorX(float x, float width, ANCHOR anchor)
    {
        float anchoredX = x;
        if ((anchor & ANCHOR.HCENTER) != 0)
            anchoredX = x - width / 2;

        if ((anchor & ANCHOR.RIGHT) != 0)
            anchoredX = x - width;

        if ((anchor & ANCHOR.LEFT) != 0)
            anchoredX = x;

        return anchoredX;
    }

    private static float AnchorY(float y, float height, ANCHOR anchor)
    {
        float anchoredY = y;
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

    public void setClip(float x, float y, float width, float height)
    {
        Clip = new GbaMonoGame.Box(new Vector2(x, y), new Vector2(width, height));
    }

    public void setColor(Color color)
    {
        Color = color;
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

    public void fillRect(float x, float y, float width, float height)
    {
        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = Gfx.Pixel;
        sprite.Color = Color;
        sprite.Position = new Vector2(x, y);
        sprite.AffineMatrix = new AffineMatrix(0, new Vector2(width, height));
        sprite.RenderOptions = RenderOptions;
        sprite.Priority = Priority;
        Sprites.Add(sprite);
    }

    public void fillRoundRect(float x, float y, float width, float height, float arcWidth, float arcHeight)
    {
        if (arcWidth == 0 && arcHeight == 0)
        {
            fillRect(x, y, arcWidth, arcHeight);
            return;
        }

        // Draw the rectangle parts
        fillRect(x + arcWidth / 2 + 1, y, width - arcWidth - 2, height); // Middle
        fillRect(x, y + arcHeight / 2 + 1, arcWidth / 2 + 1, height - arcHeight - 2); // Left
        fillRect(x + (width - arcWidth / 2) - 1, y + arcHeight / 2 + 1, arcWidth / 2 + 1, height - arcHeight - 2); // Right

        // Fill rounded corners
        fillArc(x, y, arcWidth, arcHeight, 90, 90); // Top left
        fillArc(x + width - arcWidth - 1, y, arcWidth, arcHeight, 0, 90); // Top right
        fillArc(x, y + height - arcHeight - 1, arcWidth, arcHeight, 180, 90); // Bottom left
        fillArc(x + width - arcWidth - 1, y + height - arcHeight - 1, arcWidth, arcHeight, 270, 90); // Bottom right
    }

    public void fillArc(float x, float y, float width, float height, int startAngle, int arcAngle)
    {
        arcAngle = -arcAngle;
        startAngle = -startAngle;

        float centerX = x + width / 2;
        float centerY = y + height / 2;
        float radiusX = width / 2;
        float radiusY = height / 2;
        float startAngleRad = MathHelper.ToRadians(startAngle);
        float endAngleRad = MathHelper.ToRadians(startAngle + arcAngle) - MathHelper.ToRadians(startAngle);
        float maxRadius = Math.Max(radiusX, radiusY);

        int steps = (int)Math.Abs(arcAngle * ((width + height) / 2) / 50);

        int widthInt = (int)MathF.Ceiling(width);
        int heightInt = (int)MathF.Ceiling(height);

        // Draw-map to avoid duplicated draws
        bool[] drawMap = ArrayPool<bool>.Shared.Rent(widthInt * heightInt);
        Array.Clear(drawMap);
        for (int i = 0; i < steps; i++)
        {
            float angle = startAngleRad + i * endAngleRad / steps;

            for (float r = 0; r < maxRadius; r++)
            {
                int innerX = (int)Math.Round(radiusX * Math.Cos(angle) * (r / maxRadius));
                int innerY = (int)Math.Round(radiusY * Math.Sin(angle) * (r / maxRadius));

                int drawMapIndex = (innerY + heightInt / 2) * widthInt + (innerX + widthInt / 2);
                if (!drawMap[drawMapIndex])
                {
                    drawMap[drawMapIndex] = true;
                    fillRect(centerX + innerX, centerY + innerY, 1, 1);
                }
            }
        }
        ArrayPool<bool>.Shared.Return(drawMap);
    }

    public void drawImage(Texture2D img, float x, float y, ANCHOR anchor)
    {
        x = AnchorX(x, img.Width, anchor);
        y = AnchorY(y, img.Height, anchor);

        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = img;
        sprite.Position = new Vector2(x, y);
        sprite.RenderOptions = RenderOptions;
        sprite.Priority = Priority;
        sprite.ScissorBox = Clip;
        Sprites.Add(sprite);
    }

    public void drawRegion(Texture2D src, float x_src, float y_src, float width, float height, TRANS transform, float x_dest, float y_dest, ANCHOR anchor)
    {
        if (transform != TRANS.MIRROR && transform != TRANS.NONE)
            throw new InvalidOperationException("Only mirror and none transforms are supported");

        x_dest = AnchorX(x_dest, src.Width, anchor);
        y_dest = AnchorY(y_dest, src.Height, anchor);

        Rectangle clip = new(
            x: (int)MathF.Ceiling(x_src), 
            y: (int)MathF.Ceiling(y_src), 
            width: (int)MathF.Floor(width), 
            height: (int)MathF.Floor(height));

        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = src;
        sprite.Position = new Vector2(x_dest, y_dest);
        sprite.RenderOptions = RenderOptions;
        sprite.Priority = Priority;
        sprite.ScissorBox = Clip;
        sprite.TextureRectangle = clip;
        sprite.FlipX = transform == TRANS.MIRROR;
        Sprites.Add(sprite);
    }

    public void drawString(string str, float x, float y, ANCHOR anchor)
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
            Sprite sprite = Engine.Font.GetCharacterSprite(
                c: c,
                fontSize: Font.Size,
                transformation: transformation,
                position: ref pos,
                priority: Priority,
                affineMatrix: null,
                alpha: AlphaCoefficient.Max,
                color: Color,
                renderOptions: RenderOptions);

            sprite.ScissorBox = Clip;

            Sprites.Add(sprite);
        }
    }

    // Custom
    public void ForceOriginalResolution()
    {
        RenderContext.SetFixedResolution(J2meRom.OriginalResolution);
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

    public void DrawGfx()
    {
        // Reverse order
        for (int i = Sprites.Count - 1; i >= 0; i--)
            Gfx.AddSprite(Sprites[i], SpriteType.Default);    

        Sprites.Clear();
    }
}