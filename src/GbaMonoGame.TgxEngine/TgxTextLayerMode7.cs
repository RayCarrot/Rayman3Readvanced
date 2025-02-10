using System;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TgxTextLayerMode7 : TgxGameLayer
{
    public TgxTextLayerMode7(RenderContext renderContext, PlayfieldMode7Resource playfieldResource, GameLayerResource gameLayerResource) : base(gameLayerResource)
    {
        Resource = gameLayerResource.TextLayerMode7;
        TileMap = CreateTileMap(playfieldResource, gameLayerResource);
        LayerId = Resource.LayerId;
        Is8Bit = Resource.Is8Bit;

        if (Resource.MapDimensions.X == 64)
            RotationFactor = Resource.RotationFactor * 2;
        else
            RotationFactor = Resource.RotationFactor;

        Screen = new GfxScreen(LayerId)
        {
            IsEnabled = true,
            Offset = Vector2.Zero,
            Priority = Resource.Priority,
            Wrap = true,
            Is8Bit = Resource.Is8Bit,
            Alpha = Resource.AlphaCoeff,
            RenderOptions = { RenderContext = renderContext, Alpha = Resource.HasAlphaBlending }, // TODO: We also need to update TransitionsFX, see Beneath map 1
        };

        Gfx.AddScreen(Screen);
    }

    public TextLayerMode7Resource Resource { get; }
    public GfxScreen Screen { get; }
    public MapTile[] TileMap { get; }
    public byte LayerId { get; }
    public bool Is8Bit { get; }

    public Vector2 ScrolledPosition { get; set; }
    public float RotationFactor { get; }
    public bool IsStatic { get; set; }

    private static MapTile[] CreateTileMap(PlayfieldMode7Resource playfieldResource, GameLayerResource gameLayerResource)
    {
        // Get the resource
        TextLayerMode7Resource resource = gameLayerResource.TextLayerMode7;
        
        // Get the map dimensions
        int width = resource.MapDimensions.X;
        int height = resource.MapDimensions.Y;

        // Create a tilemap for this map. In the original game one background screen can share two maps stacked vertically.
        MapTile[] tileMap = new MapTile[width * height];

        // The size of a screen block in tiles (size in bytes is 2048)
        const int screenBlockSize = 1024;

        // Get the screen base block (a value between 0-31) to determine where we start reading tiles from
        int baseScreenBlock = resource.MapBlock;

        // Get the screen width in tiles
        int screenWidth = resource.BackgroundSize switch
        {
            0 => 16,
            1 => 32,
            2 => 64,
            3 => 128,
            _ => throw new ArgumentOutOfRangeException(nameof(resource.BackgroundSize), resource.BackgroundSize, null)
        };

        // Get the base tile index based on where the map is on the screen
        int baseTileIndex = resource.MapPosition.Y / Tile.Size * screenWidth;

        // Set each tile
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int screenBlock = baseScreenBlock + x / screenWidth;

                int screenX = x % screenWidth;
                int screenY = y;

                int tileIndex = baseTileIndex + screenBlock * screenBlockSize + screenY * screenWidth + screenX;

                tileMap[y * width + x] = playfieldResource.TextLayerTileMap[tileIndex];
            }
        }

        return tileMap;
    }

    public override void SetOffset(Vector2 offset)
    {
        Screen.Offset = offset;
    }

    public override void SetWorldViewProjMatrix(Matrix worldViewProj)
    {
        Screen.RenderOptions.WorldViewProj = worldViewProj;
    }

    public void LoadRenderer(TileKit tileKit, GbaVram vram)
    {
        // Create and set the palette texture
        Screen.RenderOptions.PaletteTexture = new PaletteTexture(
            Texture: Engine.TextureCache.GetOrCreateObject(
                pointer: vram.SelectedPalette.CachePointer,
                id: 0,
                data: vram.SelectedPalette,
                createObjFunc: static p => new PaletteTexture2D(p)),
            PaletteIndex: 0);

        Texture2D layerTexture = Engine.TextureCache.GetOrCreateObject(
            pointer: Resource.Offset,
            id: 0,
            data: (Vram: vram, Layer: this),
            createObjFunc: static data => new IndexedTiledTexture2D(data.Layer.Width, data.Layer.Height, data.Vram.TileSet, data.Layer.TileMap, data.Layer.Is8Bit));

        Screen.Renderer = new TextureScreenRenderer(layerTexture);
    }
}