using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.TgxEngine;

public class TgxPlayfieldMode7 : TgxPlayfield
{
    public TgxPlayfieldMode7(PlayfieldMode7Resource playfieldResource) 
        : base(new TgxCameraMode7(CreateRenderContext()), playfieldResource.TileKit)
    {
        List<TgxRotscaleLayerMode7> rotScaleLayers = new();
        List<TgxTextLayerMode7> textLayers = new();

        // Load tiles
        GfxTileKitManager.LoadTileKit(playfieldResource.TileKit, playfieldResource.TileMappingTable, 0x100, true, playfieldResource.DefaultPalette);

        // Load the layers
        foreach (GameLayerResource gameLayerResource in playfieldResource.Layers)
        {
            if (gameLayerResource.Type == GameLayerType.RotscaleLayerMode7)
            {
                TgxRotscaleLayerMode7 layer = new(RenderContext, gameLayerResource);
                rotScaleLayers.Add(layer);

                layer.LoadRenderer(GfxTileKitManager, playfieldResource.TileKit, AnimatedTilekitManager);

                Camera.AddRotScaleLayer(layer);
            }
            else if (gameLayerResource.Type == GameLayerType.TextLayerMode7)
            {
                TgxTextLayerMode7 layer = new(Camera.TextLayerRenderContext, playfieldResource, gameLayerResource);
                textLayers.Add(layer);

                layer.LoadRenderer(GfxTileKitManager, playfieldResource.TileKit, AnimatedTilekitManager);

                Camera.AddTextLayer(layer);
            }
            else if (gameLayerResource.Type == GameLayerType.PhysicalLayer)
            {
                PhysicalLayer = new TgxTilePhysicalLayer(RenderContext, gameLayerResource);

                // We want the debug collision map to scroll with the camera, so add it as a layer
                Camera.AddRotScaleLayer(PhysicalLayer);
            }
        }

        RotScaleLayers = rotScaleLayers;
        TextLayers = textLayers;
    }

    private static RenderContext CreateRenderContext()
    {
        // Force it to use the original resolution (240x160) as a base. Otherwise we run into issues
        // if it's too big as just increasing it slightly will cause you to see waaaay past the end
        // of the level map.
        return Rom.OriginalScaledGameRenderContext;
    }

    public new TgxCameraMode7 Camera => (TgxCameraMode7)base.Camera;
    public IReadOnlyList<TgxRotscaleLayerMode7> RotScaleLayers { get; }
    public IReadOnlyList<TgxTextLayerMode7> TextLayers { get; }
}