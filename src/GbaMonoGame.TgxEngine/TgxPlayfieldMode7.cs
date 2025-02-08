using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.TgxEngine;

public class TgxPlayfieldMode7 : TgxPlayfield
{
    public TgxPlayfieldMode7(PlayfieldMode7Resource playfieldResource) 
        : base(new TgxCameraMode7(CreateRenderContext(playfieldResource)), playfieldResource.TileKit)
    {
        List<TgxRotscaleLayerMode7> rotScaleLayers = new();
        List<TgxTextLayerMode7> textLayers = new();

        // Load vram
        Vram = GbaVram.AllocateStatic(playfieldResource.TileKit, playfieldResource.TileMappingTable, 0x100, true, playfieldResource.DefaultPalette);

        // Load the layers
        foreach (GameLayerResource gameLayerResource in playfieldResource.Layers)
        {
            if (gameLayerResource.Type == GameLayerType.RotscaleLayerMode7)
            {
                TgxRotscaleLayerMode7 layer = new(RenderContext, gameLayerResource);
                rotScaleLayers.Add(layer);

                layer.LoadRenderer(playfieldResource.TileKit, Vram);

                Camera.AddRotScaleLayer(layer);

                // TODO: Fix - in Mode7 it always uses the TextureScreenRenderer, but tiles may still be animated!
                // Add the renderer to the animated tile kit manager
                if (layer.Screen.Renderer is TileMapScreenRenderer renderer)
                    AnimatedTilekitManager?.AddRenderer(renderer);
            }
            else if (gameLayerResource.Type == GameLayerType.TextLayerMode7)
            {
                TgxTextLayerMode7 layer = new(RenderContext, playfieldResource, gameLayerResource);
                textLayers.Add(layer);

                layer.LoadRenderer(playfieldResource.TileKit, Vram);

                // TODO: Add to camera
                // TODO: Animated tiles?
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

    private static RenderContext CreateRenderContext(PlayfieldMode7Resource resource)
    {
        // TODO: Implement
        return Engine.GameRenderContext;
    }

    public new TgxCameraMode7 Camera => (TgxCameraMode7)base.Camera;
    public IReadOnlyList<TgxRotscaleLayerMode7> RotScaleLayers { get; }
    public IReadOnlyList<TgxTextLayerMode7> TextLayers { get; }
    public GbaVram Vram { get; }
}