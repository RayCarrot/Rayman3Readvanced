using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.AnimEngine;

public class IndexedSpriteTexture2D : Texture2D
{
    public IndexedSpriteTexture2D(AnimatedObjectResource resource, int spriteShape, int spriteSize, int tileIndex) :
        this(resource, Constants.GetSpriteShape(spriteShape, spriteSize), tileIndex)
    { }

    public IndexedSpriteTexture2D(AnimatedObjectResource resource, Constants.Size shape, int tileIndex) :
        base(Engine.GraphicsDevice, shape.Width, shape.Height, false,
#if DESKTOPGL // Alpha8 binds to GL_LUMINANCE on OpenGL which is deprecated
            SurfaceFormat.Color
#else
            SurfaceFormat.Alpha8
#endif
            )
    {
#if DESKTOPGL
        int[] texColorIndexes = new int[Width * Height];
#else
        byte[] texColorIndexes = new byte[Width * Height];
#endif

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
                    DrawHelpers.DrawIndexedTile_8bpp(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tileSetIndex);

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
                    DrawHelpers.DrawIndexedTile_4bpp(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tileSetIndex, 0);

                    absTileX += Tile.Size;
                }

                absTileY += Tile.Size;
            }
        }

        SetData(texColorIndexes);
    }
}