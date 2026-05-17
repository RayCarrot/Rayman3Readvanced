using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.AnimEngine;

public class SpriteTexture2D : Texture2D
{
    public SpriteTexture2D(AnimatedObjectResource resource, int spriteShape, int spriteSize, Palette palette, int tileIndex) :
        this(resource, Constants.GetSpriteShape(spriteShape, spriteSize), palette, tileIndex) { }

    public SpriteTexture2D(AnimatedObjectResource resource, Constants.Size shape, Palette palette, int tileIndex) :
        base(Engine.Assets.GraphicsDevice, shape.Width, shape.Height)
    {
        Color[] texColors = new Color[Width * Height];
        byte[] tileSet = resource.SpriteTable.Data;
        int tileSetIndex = tileIndex * 0x20;

        if (resource.Is8Bit)
        {
            int absTileY = 0;

            for (int tileY = 0; tileY < shape.TilesHeight; tileY++)
            {
                int absTileX = 0;

                for (int tileX = 0; tileX < shape.TilesWidth; tileX++)
                {
                    DrawHelpers.DrawTile_8bpp(texColors, absTileX, absTileY, Width, tileSet, ref tileSetIndex, palette);

                    absTileX += Tile.Size;
                }

                absTileY += Tile.Size;
            }
        }
        else
        {
            int absTileY = 0;

            for (int tileY = 0; tileY < shape.TilesHeight; tileY++)
            {
                int absTileX = 0;

                for (int tileX = 0; tileX < shape.TilesWidth; tileX++)
                {
                    DrawHelpers.DrawTile_4bpp(texColors, absTileX, absTileY, Width, tileSet, ref tileSetIndex, palette, 0);

                    absTileX += Tile.Size;
                }

                absTileY += Tile.Size;
            }
        }

        SetData(texColors);
    }
}