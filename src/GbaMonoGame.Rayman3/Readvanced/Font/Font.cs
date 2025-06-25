using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class Font
{
    public Font(Texture2D texture, Dictionary<char, Glyph> glyphs, float lineHeight)
    {
        Texture = texture;
        Glyphs = glyphs;
        LineHeight = lineHeight;
    }

    public Texture2D Texture { get; }
    public Dictionary<char, Glyph> Glyphs { get; }
    public float LineHeight { get; }

    public float GetWidth(string text, int charIndex)
    {
        float width = 0;
        char c = text[charIndex];

        if (!Glyphs.TryGetValue(c, out Glyph glyph))
            throw new Exception($"The character '{c}' is not defined for this font");

        Rectangle bounds = glyph.Bounds;

        if (charIndex > 0 && glyph.GlyphSpecificLayoutOffsets?.TryGetValue(text[charIndex - 1], out float off) == true)
            width += off;

        width += bounds.Width + glyph.LayoutOffset;

        return width;
    }

    public float GetWidth(string text)
    {
        float width = 0;

        for (int charIndex = 0; charIndex < text.Length; charIndex++)
            width += GetWidth(text, charIndex);

        return width;
    }

    public string WrapText(string text, float width)
    {
        StringBuilder wrappedText = new();

        float xPos = 0;
        int startIndex = 0;

        for (int charIndex = 0; charIndex < text.Length; charIndex++)
        {
            if (text[charIndex] == '\r' || text[charIndex] == '\n')
            {
                wrappedText.AppendLine(text[startIndex..charIndex]);

                if (charIndex + 1 < text.Length && text[charIndex] == '\r' && text[charIndex + 1] == '\n')
                    charIndex += 2;
                else
                    charIndex += 1;

                startIndex = charIndex;
                xPos = 0;
                continue;
            }

            xPos += GetWidth(text, charIndex);

            if (xPos >= width)
            {
                for (int i = charIndex; i >= 0; i--)
                {
                    if (text[i] == ' ')
                    {
                        wrappedText.AppendLine(text[startIndex..i]);
                        charIndex = i + 1;
                        startIndex = charIndex;
                        xPos = 0;
                        break;
                    }
                }

                if (xPos != 0)
                {
                    wrappedText.AppendLine(text[startIndex..(charIndex - 1)]);
                    charIndex--;
                    startIndex = charIndex;
                    xPos = 0;
                }
            }
        }

        wrappedText.Append(text[startIndex..]);

        return wrappedText.ToString();
    }

    public Sprite GetCharacterSprite(
        string text,
        int charIndex,
        Matrix transformation,
        ref Vector2 position,
        int priority,
        AffineMatrix? affineMatrix,
        float alpha,
        Color color,
        RenderOptions renderOptions)
    {
        char c = text[charIndex];

        if (!Glyphs.TryGetValue(c, out Glyph glyph))
            throw new Exception($"The character '{c}' is not defined for this font");

        Rectangle bounds = glyph.Bounds;

        if (charIndex > 0 && glyph.GlyphSpecificLayoutOffsets?.TryGetValue(text[charIndex - 1], out float off) == true)
            position += new Vector2(off, 0);

        Sprite sprite;
        if (bounds != Rectangle.Empty)
        {
            Vector2 spritePos = position;

            // Pivot is bottom-left
            spritePos -= new Vector2(0, bounds.Height);

            spritePos += glyph.RenderOffset;

            Vector2.Transform(ref spritePos, ref transformation, out spritePos);

            sprite = new Sprite()
            {
                Texture = Texture,
                TextureRectangle = bounds,
                Position = spritePos,
                Priority = priority,
                Center = false,
                AffineMatrix = affineMatrix,
                Alpha = alpha,
                Color = color,
                RenderOptions = renderOptions,
            };
        }
        else
        {
            sprite = null;
        }

        position += new Vector2(bounds.Width, 0) + new Vector2(glyph.LayoutOffset, 0);

        return sprite;
    }

    public class Glyph
    {
        public Glyph(Rectangle bounds)
        {
            Bounds = bounds;
            RenderOffset = Vector2.Zero;
            LayoutOffset = 0;
        }

        public Glyph(Rectangle bounds, Vector2 renderOffset)
        {
            Bounds = bounds;
            RenderOffset = renderOffset;
            LayoutOffset = 0;
        }

        public Glyph(Rectangle bounds, float layoutOffset)
        {
            Bounds = bounds;
            RenderOffset = Vector2.Zero;
            LayoutOffset = layoutOffset;
        }

        public Glyph(Rectangle bounds, Vector2 renderOffset, float layoutOffset)
        {
            Bounds = bounds;
            RenderOffset = renderOffset;
            LayoutOffset = layoutOffset;
        }

        public Rectangle Bounds { get; }
        public Vector2 RenderOffset { get; }
        public float LayoutOffset { get; }
        public Dictionary<char, float> GlyphSpecificLayoutOffsets { get; init; }
    }
}