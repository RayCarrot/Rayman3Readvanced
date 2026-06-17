using System.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class BitmapTexture2D : Texture2D
{
    public BitmapTexture2D(int width, int height, byte[] bitmap, Palette palette) : base(Engine.Assets.GraphicsDevice, width, height)
    {
        Color[] texColors = ArrayPool<Color>.Shared.Rent(width * height);

        int colorIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texColors[colorIndex] = palette.Colors[bitmap[colorIndex]];
                colorIndex++;
            }
        }

        SetData(texColors, 0, width * height);

        ArrayPool<Color>.Shared.Return(texColors);
    }
}