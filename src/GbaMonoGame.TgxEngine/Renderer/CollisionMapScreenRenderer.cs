using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

public class CollisionMapScreenRenderer : IScreenRenderer
{
    public CollisionMapScreenRenderer(Texture2D collisionTileSet, int tileSize, int width, int height, byte[] collisionMap)
    {
        TileSetTexture = collisionTileSet;
        TileSize = tileSize;
        Width = width;
        Height = height;
        CollisionMap = collisionMap;
    }

    private const int CollisionTileSize = 16;
    private const int CollisionTileSetWidth = 16;

    public Texture2D TileSetTexture { get; }
    public int Width { get; }
    public int Height { get; }
    public int TileSize { get; }
    public byte[] CollisionMap { get; }

    private Rectangle GetVisibleTilesArea(Vector2 position, GfxScreen screen)
    {
        // If it's in 3D then we can't calculate the visible tiles area, so just return the whole map
        if (screen.RenderOptions.WorldViewProj != null)
            return new Rectangle(Point.Zero, (GetSize(screen) / TileSize).ToPoint());

        Rectangle rect = new(position.ToPoint(), GetSize(screen).ToPoint());

        int xStart = (Math.Max(0, rect.Left) - rect.X) / TileSize;
        int yStart = (Math.Max(0, rect.Top) - rect.Y) / TileSize;
        int xEnd = (int)Math.Ceiling((Math.Min(screen.RenderContext.Resolution.X, rect.Right) - rect.X) / TileSize);
        int yEnd = (int)Math.Ceiling((Math.Min(screen.RenderContext.Resolution.Y, rect.Bottom) - rect.Y) / TileSize);

        return new Rectangle(xStart, yStart, xEnd - xStart, yEnd - yStart);
    }

    public Vector2 GetSize(GfxScreen screen) => new(Width * TileSize, Height * TileSize);

    public void Draw(GfxRenderer renderer, GfxScreen screen, Vector2 position, Color color)
    {
        renderer.BeginSpriteRender(screen.RenderOptions);

        Rectangle visibleTilesArea = GetVisibleTilesArea(position, screen);

        float absTileY = position.Y + visibleTilesArea.Y * TileSize;

        for (int tileY = visibleTilesArea.Top; tileY < visibleTilesArea.Bottom; tileY++)
        {
            float absTileX = position.X + visibleTilesArea.X * TileSize;

            for (int tileX = visibleTilesArea.Left; tileX < visibleTilesArea.Right; tileX++)
            {
                byte type = CollisionMap[tileY * Width + tileX];
                
                if (type != 0xFF)
                {
                    renderer.Draw(
                        texture: TileSetTexture, 
                        position: new Vector2(absTileX, absTileY), 
                        sourceRectangle: new Rectangle((type % CollisionTileSetWidth) * CollisionTileSize, (type / CollisionTileSetWidth) * CollisionTileSize, CollisionTileSize, CollisionTileSize), 
                        rotation: 0,
                        origin: Vector2.Zero, 
                        scale: new Vector2(TileSize / (float)CollisionTileSize),
                        effects: SpriteEffects.None,
                        color: color);
                }

                absTileX += TileSize;
            }

            absTileY += TileSize;
        }
    }
}