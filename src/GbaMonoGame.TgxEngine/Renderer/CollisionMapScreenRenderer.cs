using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

public class CollisionMapScreenRenderer : IScreenRenderer
{
    public CollisionMapScreenRenderer(int width, int height, byte[] collisionMap)
    {
        TileSetTexture = Engine.FixContentManager.Load<Texture2D>(Assets.CollisionTileSetTexture);
        Width = width;
        Height = height;
        CollisionMap = collisionMap;
    }

    private const int CollisionTileSize = 16;
    private const int CollisionTileSetWidth = 16;

    public Texture2D TileSetTexture { get; }
    public int Width { get; }
    public int Height { get; }
    public byte[] CollisionMap { get; }

    private Rectangle GetVisibleTilesArea(Vector2 position, GfxScreen screen)
    {
        // If it's in 3D then we can't calculate the visible tiles area, so just return the whole map
        if (screen.RenderOptions.WorldViewProj != null)
            return new Rectangle(Point.Zero, (GetSize(screen) / Tile.Size).ToPoint());

        Rectangle rect = new(position.ToPoint(), GetSize(screen).ToPoint());

        int xStart = (Math.Max(0, rect.Left) - rect.X) / Tile.Size;
        int yStart = (Math.Max(0, rect.Top) - rect.Y) / Tile.Size;
        int xEnd = (int)Math.Ceiling((Math.Min(screen.RenderOptions.RenderContext.Resolution.X, rect.Right) - rect.X) / Tile.Size);
        int yEnd = (int)Math.Ceiling((Math.Min(screen.RenderOptions.RenderContext.Resolution.Y, rect.Bottom) - rect.Y) / Tile.Size);

        return new Rectangle(xStart, yStart, xEnd - xStart, yEnd - yStart);
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
                byte type = CollisionMap[tileY * Width + tileX];
                
                if (type != 0xFF)
                {
                    renderer.Draw(
                        texture: TileSetTexture, 
                        position: new Vector2(absTileX, absTileY), 
                        sourceRectangle: new Rectangle((type % CollisionTileSetWidth) * CollisionTileSize, (type / CollisionTileSetWidth) * CollisionTileSize, CollisionTileSize, CollisionTileSize), 
                        rotation: 0,
                        origin: Vector2.Zero, 
                        scale: new Vector2(Tile.Size / (float)CollisionTileSize),
                        effects: SpriteEffects.None,
                        color: color);
                }

                absTileX += Tile.Size;
            }

            absTileY += Tile.Size;
        }
    }
}