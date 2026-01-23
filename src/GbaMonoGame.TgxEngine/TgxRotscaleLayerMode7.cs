using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TgxRotscaleLayerMode7 : TgxGameLayer
{
    public TgxRotscaleLayerMode7(RenderContext renderContext, GameLayerResource gameLayerResource) : base(gameLayerResource)
    {
        Resource = gameLayerResource.RotscaleLayerMode7;

        TileMap = Resource.TileMap;
        Is8Bit = true;
        LayerId = Resource.LayerId;

        // Hack - we set the screen layer id to 4, which is normally out of bounds (as the GBA only has 0-3). The reason we
        // have to do this is that on the GBA the RotScale layer is actually shared with a Text layer, so they end up
        // having the same layer id.
        Screen = new GfxScreen(4)
        {
            IsEnabled = true,
            Offset = Vector2.Zero,
            Priority = 3 - LayerId,
            Wrap = true,
            Is8Bit = Is8Bit,
            RenderContext = renderContext,
            BlendMode = Resource.HasAlphaBlending ? BlendMode.AlphaBlend : BlendMode.None,
        };

        if (Resource.HasAlphaBlending)
            TransitionsFX.SetBGAlphaBlending(Screen, Resource.AlphaCoeff * 16);

        Gfx.AddScreen(Screen);
    }

    public RotscaleLayerMode7Resource Resource { get; }
    public GfxScreen Screen { get; }
    public MapTile[] TileMap { get; }
    public bool Is8Bit { get; }
    public byte LayerId { get; }

    public override void SetOffset(Vector2 offset)
    {
        Screen.Offset = offset;
    }

    public override void SetWorldViewProjMatrix(Matrix worldViewProj)
    {
        Screen.RenderOptions = Screen.RenderOptions with { WorldViewProj = worldViewProj };
    }

    public void LoadRenderer(GfxTileKitManager tileKitManager, AnimatedTilekitManager animatedTilekitManager)
    {
        Screen.RenderOptions = Screen.RenderOptions with { PaletteTexture = tileKitManager.CreateTileMapPalette() };
        Screen.Renderer = tileKitManager.CreateTileMapRenderer(
            animatedTilekitManager: animatedTilekitManager,
            layerCachePointer: Resource.Offset,
            width: Width,
            height: Height,
            tileMap: TileMap,
            baseTileIndex: 512,
            is8Bit: Is8Bit,
            isDynamic: false);
    }
}