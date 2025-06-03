using BinarySerializer;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class Palette
{
    public Palette(PaletteResource palette) : this(palette.Colors, palette.Offset) { }

    public Palette(RGB555Color[] colors, Pointer cachePointer)
    {
        CachePointer = cachePointer;
        Colors = new Color[colors.Length];

        for (int i = 0; i < Colors.Length; i++)
            Colors[i] = colors[i].ToColor();
    }

    public Palette(Color[] colors, Pointer cachePointer)
    {
        CachePointer = cachePointer;
        Colors = colors;
    }

    public Color[] Colors { get; }
    public Pointer CachePointer { get; }
}