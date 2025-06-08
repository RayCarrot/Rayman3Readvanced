﻿using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class IndexedTiledTexture2D : Texture2D
{
    public IndexedTiledTexture2D(byte[] tileSet, int tileIndex, bool is8Bit, int colorOffset) :
        base(Engine.GraphicsDevice, Tile.Size, Tile.Size, false,
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

        if (is8Bit)
        {
            int tilePixelIndex = tileIndex * 0x40;
            DrawHelpers.DrawIndexedTile_8bpp(texColorIndexes, 0, 0, Width, tileSet, ref tilePixelIndex);
        }
        else
        {
            int tilePixelIndex = tileIndex * 0x20;
            DrawHelpers.DrawIndexedTile_4bpp(texColorIndexes, 0, 0, Width, tileSet, ref tilePixelIndex, colorOffset);
        }

        SetData(texColorIndexes);
    }

    public IndexedTiledTexture2D(int width, int height, byte[] tileSet, MapTile[] tileMap, bool is8Bit) :
        this(width, height, tileSet, tileMap, 0, is8Bit) { }

    public IndexedTiledTexture2D(int width, int height, byte[] tileSet, MapTile[] tileMap, int baseTileIndex, bool is8Bit) :
        this(width, height, 0, 0, width, height, tileSet, tileMap, baseTileIndex, is8Bit)
    { }

    public IndexedTiledTexture2D(int fullWidth, int fullHeight, int startX, int startY, int width, int height, byte[] tileSet, MapTile[] tileMap, int baseTileIndex, bool is8Bit) :
        base(Engine.GraphicsDevice, width * Tile.Size, height * Tile.Size, false,
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

        int endX = startX + width;
        int endY = startY + height;

        if (is8Bit)
        {
            int absTileY = 0;

            for (int tileY = startY; tileY < endY; tileY++)
            {
                int absTileX = 0;

                for (int tileX = startX; tileX < endX; tileX++)
                {
                    MapTile tile = tileMap[tileY * fullWidth + tileX];

                    int tilePixelIndex = (baseTileIndex + tile.TileIndex) * 0x40;

                    if (tile.FlipX && tile.FlipY)
                        DrawHelpers.DrawIndexedTile_8bpp_FlipXY(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex);
                    else if (tile.FlipX)
                        DrawHelpers.DrawIndexedTile_8bpp_FlipX(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex);
                    else if (tile.FlipY)
                        DrawHelpers.DrawIndexedTile_8bpp_FlipY(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex);
                    else
                        DrawHelpers.DrawIndexedTile_8bpp(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex);

                    absTileX += Tile.Size;
                }

                absTileY += Tile.Size;
            }
        }
        else
        {
            int absTileY = 0;

            for (int tileY = startY; tileY < endY; tileY++)
            {
                int absTileX = 0;

                for (int tileX = startX; tileX < endX; tileX++)
                {
                    MapTile tile = tileMap[tileY * fullWidth + tileX];

                    int tilePixelIndex = (baseTileIndex + tile.TileIndex) * 0x20;
                    int palOffset = tile.PaletteIndex * 16;

                    if (tile.FlipX && tile.FlipY)
                        DrawHelpers.DrawIndexedTile_4bpp_FlipXY(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex, palOffset);
                    else if (tile.FlipX)
                        DrawHelpers.DrawIndexedTile_4bpp_FlipX(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex, palOffset);
                    else if (tile.FlipY)
                        DrawHelpers.DrawIndexedTile_4bpp_FlipY(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex, palOffset);
                    else
                        DrawHelpers.DrawIndexedTile_4bpp(texColorIndexes, absTileX, absTileY, Width, tileSet, ref tilePixelIndex, palOffset);

                    absTileX += Tile.Size;
                }

                absTileY += Tile.Size;
            }
        }

        SetData(texColorIndexes);
    }
}