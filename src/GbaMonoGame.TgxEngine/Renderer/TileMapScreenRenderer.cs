using System;
using System.Collections.Generic;
using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

public class TileMapScreenRenderer : IScreenRenderer
{
    public TileMapScreenRenderer(
        int width, 
        int height, 
        MapTile[] tileMap, 
        bool is8Bit, 
        Dictionary<int, Texture2D> tileTextures)
    {
        Width = width;
        Height = height;
        TileMap = tileMap;
        TileTextures = tileTextures;
        Is8Bit = is8Bit;

        _replacedTiles = new Dictionary<int, int>();
    }

    private readonly Dictionary<int, int> _replacedTiles;

    public int Width { get; }
    public int Height { get; }
    public MapTile[] TileMap { get; }
    public bool Is8Bit { get; }
    public Dictionary<int, Texture2D> TileTextures { get; }

    private Rectangle GetVisibleTilesArea(Vector2 position, GfxScreen screen)
    {
        Box renderBox = new(position, GetSize(screen));

        int xStart = (int)((Math.Max(0, renderBox.Left) - renderBox.Left) / Tile.Size);
        int yStart = (int)((Math.Max(0, renderBox.Top) - renderBox.Top) / Tile.Size);
        int xEnd = (int)Math.Ceiling((Math.Min(screen.RenderContext.Resolution.X, renderBox.Right) - renderBox.Left) / Tile.Size);
        int yEnd = (int)Math.Ceiling((Math.Min(screen.RenderContext.Resolution.Y, renderBox.Bottom) - renderBox.Top) / Tile.Size);

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

                    Texture2D tex = TileTextures[tileIndex];

                    SpriteEffects effects = SpriteEffects.None;

                    if (tile.FlipX)
                        effects |= SpriteEffects.FlipHorizontally;
                    if (tile.FlipY)
                        effects |= SpriteEffects.FlipVertically;

                    renderer.Draw(tex, new Vector2(absTileX, absTileY), effects, color);
                }

                absTileX += Tile.Size;
            }

            absTileY += Tile.Size;
        }
    }
}