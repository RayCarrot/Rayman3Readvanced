using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TgxTileLayer : TgxGameLayer
{
    public TgxTileLayer(RenderContext renderContext, GameLayerResource gameLayerResource) : base(gameLayerResource)
    {
        Resource = gameLayerResource.TileLayer;

        TileMap = Resource.TileMap;
        LayerId = Resource.LayerId;
        Is8Bit = Resource.Is8Bit;
        IsDynamic = Resource.IsDynamic;

        Screen = new GfxScreen(LayerId)
        {
            IsEnabled = true,
            Offset = Vector2.Zero,
            Priority = 3 - LayerId,
            Wrap = true,
            Is8Bit = Resource.Is8Bit,
            IsAlphaBlendEnabled = Resource.HasAlphaBlending, // TODO: We also need to update TransitionsFX, see Beneath map 1
            Alpha = Resource.AlphaCoeff,
            RenderContext = renderContext,
        };

        Gfx.AddScreen(Screen);
    }

    public TileLayerResource Resource { get; }
    public GfxScreen Screen { get; }
    public MapTile[] TileMap { get; }
    public byte LayerId { get; }
    public bool Is8Bit { get; }
    public bool IsDynamic { get; }

    public override void SetOffset(Vector2 offset)
    {
        Screen.Offset = offset;
    }

    public void LoadRenderer(TileKit tileKit, GbaVram vram)
    {
        PaletteTexture paletteTexture = new(
            Texture: Engine.TextureCache.GetOrCreateObject(
                pointer: vram.SelectedPalette.CachePointer,
                id: 0,
                data: vram.SelectedPalette,
                createObjFunc: static p => new PaletteTexture2D(p)),
            PaletteIndex: 0);

        // The game has two ways of allocating tilesets. If it's dynamic then it reserves space in vram for dynamically loading
        // the tile graphics as they're being displayed on screen (as the camera scrolls). If it's static then it's all pre-loaded.
        if (IsDynamic)
        {
            byte[] tileSet = Is8Bit ? tileKit.Tiles8bpp : tileKit.Tiles4bpp;
            Screen.Renderer = new TileMapScreenRenderer(
                // Use the tilekit as the cache pointer since multiple layers can share the same tilekit and we're
                // caching per tile, but make the pointer differ depending on if it's 4-bit or 8-bit.
                cachePointer: tileKit.Offset + (Is8Bit ? 1 : 0), 
                width: Width, 
                height: Height, 
                tileMap: TileMap, 
                tileSet: tileSet, 
                paletteTexture: paletteTexture, 
                is8Bit: Is8Bit);
        }
        else
        {
            // NOTE: Using a single texture is more optimized, but won't work if tiles should be animated! Luckily static tile layers
            //       are never animated in Rayman 3, so we can do this.
            Texture2D layerTexture = Engine.TextureCache.GetOrCreateObject(
                pointer: Resource.Offset,
                id: 0,
                data: (Vram: vram, Layer: this),
                createObjFunc: static data => new IndexedTiledTexture2D(data.Layer.Width, data.Layer.Height, data.Vram.TileSet, data.Layer.TileMap, data.Layer.Is8Bit));

            Screen.Renderer = new TextureScreenRenderer(layerTexture)
            {
                PaletteTexture = paletteTexture,
            };
        }
    }
}