﻿using System;
using System.Numerics;
using System.Text;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public static class FontManager
{
    private const int TextureWidth = 512;
    private const int Padding = 2;

    private static LoadedFont _font8;
    private static LoadedFont _font16;
    private static LoadedFont _font32;

    public static Encoding Encoding { get; } = Encoding.GetEncoding(1252);

    private static Point GetCharSizeInTexture(Font font)
    {
        int charWidth = font.CharacterHeight; // Use the height as the width in the texture
        int charHeight = font.CharacterHeight;

        // Apply padding so the characters don't bleed into each other when scaling
        charWidth += Padding;
        charHeight += Padding;

        return new Point(charWidth, charHeight);
    }

    private static Texture2D CreateFontTexture(Font font, Color foreground, Color background)
    {
        // Get the size for each character in the font texture
        Point charSize = GetCharSizeInTexture(font);

        // Use a single texture for the entire texture for better performance
        int charsPerRow = TextureWidth / charSize.X;
        int textureHeight = (int)BitOperations.RoundUpToPowerOf2((uint)(charSize.Y * (Font.CharactersCount / charsPerRow)));
        Texture2D tex = new(Engine.GraphicsDevice, TextureWidth, textureHeight);
        Color[] texColors = new Color[tex.Width * tex.Height];

        // Pad out end of array
        byte[] expandedImgData = new byte[font.ImgData.Length + 3];
        Array.Copy(font.ImgData, expandedImgData, font.ImgData.Length);

        if (background != Color.Transparent)
            Array.Fill(texColors, background);

        // Draw every character to the texture
        for (int charIndex = 0; charIndex < Font.CharactersCount; charIndex++)
        {
            int width = font.CharacterWidths[charIndex];

            if (width == 0)
                continue;

            // Get the origin point of the character on the texture
            int originX = (charIndex % charsPerRow) * charSize.X;
            int originY = (charIndex / charsPerRow) * charSize.Y;

            int bitIndex = 0;
            uint value = 0;
            
            int imgDataOffset = font.CharacterOffsets[charIndex];
            int pixelOffset = originY * tex.Width + originX;

            // Draw every row
            for (int x = 0; x < width; x++)
            {
                // Get next value
                if (bitIndex == 0 || (bitIndex & 0x30) != 0)
                {
                    value = BitConverter.ToUInt32(expandedImgData, imgDataOffset);
                    bitIndex = 0;
                    imgDataOffset += font.CharacterHeight == 0x20 ? 4 : 2;
                }

                // Draw the column
                int currentPixelOffset = pixelOffset;
                for (int y = 0; y < font.CharacterHeight; y++)
                {
                    if (((value >> bitIndex) & 1) != 0)
                        texColors[currentPixelOffset] = foreground;

                    bitIndex += 1;
                    currentPixelOffset += tex.Width;
                }

                pixelOffset += 1;
            }
        }

        tex.SetData(texColors);

        return tex;
    }

    private static Rectangle[] GetFontCharacterRectangles(Font font)
    {
        // Get the size for each character in the font texture
        Point charSize = GetCharSizeInTexture(font);

        int charsPerRow = TextureWidth / charSize.X;
        Rectangle[] rects = new Rectangle[Font.CharactersCount];

        for (int charIndex = 0; charIndex < rects.Length; charIndex++)
        {
            // Get the origin point of the character on the texture
            int originX = (charIndex % charsPerRow) * charSize.X;
            int originY = (charIndex / charsPerRow) * charSize.Y;

            rects[charIndex] = new Rectangle(originX, originY, font.CharacterWidths[charIndex], font.CharacterHeight);
        }

        return rects;
    }

    public static void Load(Font font8, Font font16, Font font32)
    {
        _font8 = new LoadedFont(font8, CreateFontTexture(font8, Color.White, Color.Transparent), GetFontCharacterRectangles(font8));
        _font16 = new LoadedFont(font16, CreateFontTexture(font16, Color.White, Color.Transparent), GetFontCharacterRectangles(font16));
        _font32 = new LoadedFont(font32, CreateFontTexture(font32, Color.White, Color.Transparent), GetFontCharacterRectangles(font32));
    }

    public static void Unload()
    {
        _font8?.Texture.Dispose();
        _font16?.Texture.Dispose();
        _font32?.Texture.Dispose();
        
        _font8 = null;
        _font16 = null;
        _font32 = null;
    }

    public static byte[] GetTextBytes(string text)
    {
        return Encoding.GetBytes(text);
    }

    public static byte[] GetTextBytes(string text, int index, int count)
    {
        return Encoding.GetBytes(text, index, count);
    }

    public static string GetTextString(byte[] bytes)
    {
        return Encoding.GetString(bytes);
    }

    public static Matrix CreateTextTransformation(Vector2 position, Vector2 scale, Vector2 origin)
    {
        Matrix transformation = Matrix.Identity;
        transformation.M11 = scale.X;
        transformation.M22 = scale.Y;
        transformation.M41 = -origin.X * transformation.M11 + position.X + origin.X;
        transformation.M42 = -origin.Y * transformation.M22 + position.Y + origin.Y;
        return transformation;
    }

    public static int GetStringWidth(FontSize fontSize, string text)
    {
        return GetStringWidth(fontSize, GetTextBytes(text));
    }

    public static int GetStringWidth(FontSize fontSize, byte[] textBytes)
    {
        LoadedFont loadedFont = fontSize switch
        {
            FontSize.Font8 => _font8,
            FontSize.Font16 => _font16,
            FontSize.Font32 => _font32,
            _ => throw new ArgumentOutOfRangeException(nameof(fontSize), fontSize, null)
        };

        int width = 0;

        foreach (byte c in textBytes)
            width += loadedFont.Font.CharacterWidths[c];

        return width;
    }

    public static int GetFontHeight(FontSize fontSize)
    {
        LoadedFont loadedFont = fontSize switch
        {
            FontSize.Font8 => _font8,
            FontSize.Font16 => _font16,
            FontSize.Font32 => _font32,
            _ => throw new ArgumentOutOfRangeException(nameof(fontSize), fontSize, null)
        };

        return loadedFont.Font.CharacterHeight;
    }

    public static string WrapText(FontSize fontSize, string text, float width)
    {
        LoadedFont loadedFont = fontSize switch
        {
            FontSize.Font8 => _font8,
            FontSize.Font16 => _font16,
            FontSize.Font32 => _font32,
            _ => throw new ArgumentOutOfRangeException(nameof(fontSize), fontSize, null)
        };

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

            foreach (byte b in GetTextBytes(text, charIndex, 1))
                xPos += loadedFont.Font.CharacterWidths[b];

            if (xPos >= width)
            {
                for (int i = charIndex; i >= startIndex; i--)
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

    public static Sprite GetCharacterSprite(
        byte c, 
        FontSize fontSize, 
        Matrix transformation,
        ref Vector2 position, 
        int priority, 
        AffineMatrix? affineMatrix, 
        float alpha, 
        Color color,
        RenderOptions renderOptions)
    {
        LoadedFont loadedFont = fontSize switch
        {
            FontSize.Font8 => _font8,
            FontSize.Font16 => _font16,
            FontSize.Font32 => _font32,
            _ => throw new ArgumentOutOfRangeException(nameof(fontSize), fontSize, null)
        };

        Vector2 spritePos = position;
        Vector2.Transform(ref spritePos, ref transformation, out spritePos);

        Sprite sprite = new()
        {
            Texture = loadedFont.Texture,
            TextureRectangle = loadedFont.CharacterRectangles[c],
            Position = spritePos,
            Priority = priority,
            Center = false, // Don't center here since the transformation matrix already does that!
            AffineMatrix = affineMatrix,
            Alpha = alpha,
            Color = color,
            RenderOptions = renderOptions,
        };

        position += new Vector2(loadedFont.Font.CharacterWidths[c], 0);

        return sprite;
    }

    private record LoadedFont(Font Font, Texture2D Texture, Rectangle[] CharacterRectangles);
}