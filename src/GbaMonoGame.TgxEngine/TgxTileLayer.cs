using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework;
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
            RenderOptions = { RenderContext = renderContext, BlendMode = Resource.HasAlphaBlending ? BlendMode.AlphaBlend : BlendMode.None },
        };

        if (Resource.HasAlphaBlending)
            TransitionsFX.SetBGAlphaBlending(Screen, Resource.AlphaCoeff * 16);

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

    public override void SetWorldViewProjMatrix(Matrix worldViewProj)
    {
        Screen.RenderOptions.WorldViewProj = worldViewProj;
    }

    public void LoadRenderer(GfxTileKitManager tileKitManager, AnimatedTilekitManager animatedTilekitManager)
    {
        // TRAILER
        if (LayerId == 2)
            TileMap[13 * Width + 153] = default;

        Screen.Renderer = tileKitManager.CreateTileMapRenderer(
            renderOptions: Screen.RenderOptions,
            animatedTilekitManager: animatedTilekitManager,
            layerCachePointer: Resource.Offset,
            width: Width,
            height: Height,
            tileMap: TileMap,
            baseTileIndex: 0,
            is8Bit: Is8Bit,
            isDynamic: IsDynamic);
    }
}