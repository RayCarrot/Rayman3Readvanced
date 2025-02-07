using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public abstract class TgxGameLayer
{
    protected TgxGameLayer(GameLayerResource gameLayerResource)
    {
        Width = gameLayerResource.Width;
        Height = gameLayerResource.Height;
    }

    public int Width { get; }
    public int Height { get; }

    public int PixelWidth => Width * Tile.Size;
    public int PixelHeight => Height * Tile.Size;

    public abstract void SetOffset(Vector2 offset);
    public abstract void SetWorldViewProjMatrix(Matrix worldViewProj);
}