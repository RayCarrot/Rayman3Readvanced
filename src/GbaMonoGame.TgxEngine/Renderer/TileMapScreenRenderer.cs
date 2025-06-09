using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TileMapScreenRenderer : IScreenRenderer
{
    public TileMapScreenRenderer(
        Pointer cachePointer, 
        int width, 
        int height, 
        MapTile[] tileMap, 
        byte[] tileSet, 
        bool is8Bit)
    {
        CachePointer = cachePointer;
        Width = width;
        Height = height;
        TileMap = tileMap;
        TileSet = tileSet;
        Is8Bit = is8Bit;

        _replacedTiles = new Dictionary<int, int>();

        // TODO: Cache texture
        int tileSize = is8Bit ? 0x40 : 0x20;
        int tilesCount = tileSet.Length / tileSize;
        const int tileSetWidth = 32;
        int tileSetHeight = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(tilesCount / (float)tileSetWidth));
        int[] colorOffsets = null;
        if (!is8Bit)
        {
            colorOffsets = new int[tilesCount];
            foreach (MapTile mapTile in tileMap)
            {
                if (mapTile.TileIndex != 0)
                    colorOffsets[mapTile.TileIndex - 1] = mapTile.PaletteIndex * 16;
            }
        }
        TileSetTexture = new IndexedTiledTexture2D(tileSetWidth, tileSetHeight, tileSet, is8Bit, colorOffsets);
        TileRectangles = new Rectangle[tilesCount];
        for (int i = 0; i < tilesCount; i++)
            TileRectangles[i] = new Rectangle((i % 32) * Tile.Size, (i / 32) * Tile.Size, Tile.Size, Tile.Size);
    }

    private readonly Dictionary<int, int> _replacedTiles;

    public Pointer CachePointer { get; }
    public int Width { get; }
    public int Height { get; }
    public MapTile[] TileMap { get; }
    public byte[] TileSet { get; }
    public bool Is8Bit { get; }
    public Rectangle? TilesClip { get; set; } // Optional
    public Texture2D TileSetTexture { get; }
    public Rectangle[] TileRectangles { get; }

    private Rectangle GetVisibleTilesArea(Vector2 position, GfxScreen screen)
    {
        Box renderBox = new(position, GetSize(screen));

        int xStart = (int)((Math.Max(0, renderBox.Left) - renderBox.Left) / Tile.Size);
        int yStart = (int)((Math.Max(0, renderBox.Top) - renderBox.Top) / Tile.Size);
        int xEnd = (int)Math.Ceiling((Math.Min(screen.RenderOptions.RenderContext.Resolution.X, renderBox.Right) - renderBox.Left) / Tile.Size);
        int yEnd = (int)Math.Ceiling((Math.Min(screen.RenderOptions.RenderContext.Resolution.Y, renderBox.Bottom) - renderBox.Top) / Tile.Size);

        // Make sure we don't go out of bounds. Only needed if the camera shows more than the actual map, which isn't usually the case.
        xEnd = Math.Min(xEnd, Width);
        yEnd = Math.Min(yEnd, Height);

        return new Rectangle(xStart, yStart, xEnd - xStart, yEnd - yStart);
    }

    public void ReplaceTile(int originalTileIndex, int newTileIndex)
    {
        _replacedTiles[originalTileIndex] = newTileIndex;
    }

    public Vector2 GetSize(GfxScreen screen) => new(Width * Tile.Size, Height * Tile.Size);

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginSpriteRender(screen.RenderOptions);

        Rectangle visibleTilesArea = GetVisibleTilesArea(position, screen);

        if (TilesClip != null)
            visibleTilesArea = Rectangle.Intersect(visibleTilesArea, TilesClip.Value);

        LocationCache<Texture2D> textureCache = Engine.TextureCache.GetOrCreateLocationCache(CachePointer);

        float absTileY = position.Y + visibleTilesArea.Y * Tile.Size;

        for (int tileY = visibleTilesArea.Top; tileY < visibleTilesArea.Bottom; tileY++)
        {
            float absTileX = position.X + visibleTilesArea.X * Tile.Size;

            for (int tileX = visibleTilesArea.Left; tileX < visibleTilesArea.Right; tileX++)
            {
                MapTile tile = TileMap[tileY * Width + tileX];

                int tileIndex = tile.TileIndex;

                if (tileIndex != 0)
                {
                    if (_replacedTiles.TryGetValue(tileIndex, out int newTileIndex))
                        tileIndex = newTileIndex;

                    SpriteEffects effects = SpriteEffects.None;

                    if (tile.FlipX)
                        effects |= SpriteEffects.FlipHorizontally;
                    if (tile.FlipY)
                        effects |= SpriteEffects.FlipVertically;

                    renderer.Draw(TileSetTexture, new Vector2(absTileX, absTileY), TileRectangles[tileIndex - 1], effects, color);
                }

                absTileX += Tile.Size;
            }

            absTileY += Tile.Size;
        }
    }

    private readonly struct TileDefine(byte[] tileSet, int tileIndex, int paletteIndex, bool is8Bit)
    {
        public byte[] TileSet { get; } = tileSet;
        public int TileIndex { get; } = tileIndex;
        public int PaletteIndex { get; } = paletteIndex;
        public bool Is8Bit { get; } = is8Bit;
    }
}