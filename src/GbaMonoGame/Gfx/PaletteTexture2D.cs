using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BinarySerializer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class PaletteTexture2D : Texture2D
{
    public PaletteTexture2D(PaletteResource palette) : this(palette.Colors) { }

    public PaletteTexture2D(IReadOnlyList<BaseColor> palette) : base(Engine.GraphicsDevice, TextureWidth, GetHeight(palette.Count))
    {
        Color[] texColors = new Color[Width * Height];
        
        // Set the palette colors, skipping the first color since it's the transparent color
        for (int colorIndex = 1; colorIndex < palette.Count; colorIndex++)
            texColors[colorIndex] = palette[colorIndex].ToColor();

        SetData(texColors);
    }

    public PaletteTexture2D(Palette palette) : this(palette.Colors) { }

    public PaletteTexture2D(Color[] palette) : base(Engine.GraphicsDevice, TextureWidth, GetHeight(palette.Length))
    {
        Color[] texColors = new Color[Width * Height];
        
        // Set the palette colors, skipping the first color since it's the transparent color
        for (int colorIndex = 1; colorIndex < palette.Length; colorIndex++)
            texColors[colorIndex] = palette[colorIndex];

        SetData(texColors);
    }

    public PaletteTexture2D(PaletteResource[] palettes) : base(Engine.GraphicsDevice, TextureWidth, GetHeight(palettes.Sum(x => x.Colors.Length)))
    {
        Color[] texColors = new Color[Width * Height];
        
        // Enumerate each palette
        int texColorIndex = 0;
        foreach (PaletteResource pal in palettes)
        {
            // Skip the first color in each palette since it's the transparent color
            texColorIndex++;

            // Set the palette colors
            for (int colorIndex = 1; colorIndex < pal.Colors.Length; colorIndex++)
            {
                texColors[texColorIndex] = pal.Colors[colorIndex].ToColor();
                texColorIndex++;
            }

            if (texColorIndex % 16 != 0)
                throw new Exception("Invalid palette size");
        }

        SetData(texColors);
    }

    public PaletteTexture2D(Palette[] palettes) : base(Engine.GraphicsDevice, TextureWidth, GetHeight(palettes.Sum(x => x.Colors.Length)))
    {
        Color[] texColors = new Color[Width * Height];
        
        // Enumerate each palette
        int texColorIndex = 0;
        foreach (Palette pal in palettes)
        {
            // Skip the first color in each palette since it's the transparent color
            texColorIndex++;

            // Set the palette colors
            for (int colorIndex = 1; colorIndex < pal.Colors.Length; colorIndex++)
            {
                texColors[texColorIndex] = pal.Colors[colorIndex];
                texColorIndex++;
            }

            if (texColorIndex % 16 != 0)
                throw new Exception("Invalid palette size");
        }

        SetData(texColors);
    }

    private const int TextureWidth = 16;

    // The height is the number of 16-color palettes, rounded up to power of 2 to avoid issues in the shader
    private static int GetHeight(int colorsCount)
    {
        return (int)BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(colorsCount / (float)TextureWidth));
    }
}