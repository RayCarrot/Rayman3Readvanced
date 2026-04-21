using System;
using System.Collections.Generic;
using BinarySerializer.Nintendo.GBA;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TgxTileLayer : TgxGameLayer
{
    public TgxTileLayer(RenderContext renderContext, GameLayerResource gameLayerResource, int sceneId) : base(gameLayerResource)
    {
        Resource = gameLayerResource.TileLayer;

        LayerId = Resource.LayerId;
        Is8Bit = Resource.Is8Bit;
        IsDynamic = Resource.IsDynamic;

        // Optionally replace tiles
        if (Engine.ActiveConfig.Tweaks.FixTilingErrors && 
            _tileFixes.TryGetValue(sceneId, out var sceneValue) && 
            sceneValue.TryGetValue(LayerId, out var layerValue))
        {
            TileMap = new MapTile[Resource.TileMap.Length];
            Array.Copy(Resource.TileMap, TileMap, Resource.TileMap.Length);
            foreach ((int tileX, int tileY, MapTile newTile) in layerValue)
                TileMap[tileY * Width + tileX] = newTile;
        }
        else
        {
            TileMap = Resource.TileMap;
        }

        Screen = new GfxScreen(LayerId)
        {
            IsEnabled = true,
            Offset = Vector2.Zero,
            Priority = 3 - LayerId,
            Wrap = true,
            Is8Bit = Resource.Is8Bit,
            RenderContext = renderContext,
            BlendMode = Resource.HasAlphaBlending ? BlendMode.AlphaBlend : BlendMode.None,
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
            baseTileIndex: 0,
            is8Bit: Is8Bit,
            isDynamic: IsDynamic);
    }

    // For tile replacement
    private static readonly Dictionary<int, Dictionary<int, List<(int TileX, int TileY, MapTile NewTile)>>> _tileFixes = new();
    public static void ClearTileFixes() => _tileFixes.Clear();
    public static void DefineTileFix(int sceneId, int layerId, int tileX, int tileY, MapTile newTile)
    {
        if (!_tileFixes.TryGetValue(sceneId, out var sceneValue))
        {
            sceneValue = new();
            _tileFixes[sceneId] = sceneValue;
        }

        if (!sceneValue.TryGetValue(layerId, out var layerValue))
        {
            layerValue = [];
            sceneValue[layerId] = layerValue;
        }

        layerValue.Add((tileX, tileY, newTile));
    }
}