using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.TgxEngine;

public class TgxPlayfieldMode7 : TgxPlayfield
{
    public TgxPlayfieldMode7(PlayfieldMode7Resource playfieldResource) 
        : base(new TgxCameraMode7(CreateRenderContext(playfieldResource)), playfieldResource.TileKit)
    {
        List<TgxRotscaleLayerMode7> rotScaleLayers = new();

        // Load vram
        Vram = GbaVram.AllocateStatic(playfieldResource.TileKit, playfieldResource.TileMappingTable, 0x100, true, playfieldResource.DefaultPalette);

        // Load the layers
        foreach (GameLayerResource gameLayerResource in playfieldResource.Layers)
        {
            if (gameLayerResource.Type == GameLayerType.RotscaleLayerMode7)
            {
                TgxRotscaleLayerMode7 layer = new(gameLayerResource);
                rotScaleLayers.Add(layer);

                layer.LoadRenderer(playfieldResource.TileKit, Vram);

                layer.Screen.RenderContext = RenderContext;
                ((TextureScreenRenderer)layer.Screen.Renderer).Shader = Camera.BasicEffectShader;

                // TODO: Fix - in Mode7 it always uses the TextureScreenRenderer, but tiles may still be animated!
                // Add the renderer to the animated tile kit manager
                if (layer.Screen.Renderer is TileMapScreenRenderer renderer)
                    AnimatedTilekitManager?.AddRenderer(renderer);
            }
            else if (gameLayerResource.Type == GameLayerType.TextLayerMode7)
            {
                // TODO: Implement
            }
            else if (gameLayerResource.Type == GameLayerType.PhysicalLayer)
            {
                PhysicalLayer = new TgxTilePhysicalLayer(RenderContext, gameLayerResource);
                ((CollisionMapScreenRenderer)PhysicalLayer.DebugScreen.Renderer).Shader = Camera.BasicEffectShader;
            }
        }

        RotScaleLayers = rotScaleLayers;
    }

    private static RenderContext CreateRenderContext(PlayfieldMode7Resource resource)
    {
        // TODO: Implement
        return Engine.GameRenderContext;
    }

    public new TgxCameraMode7 Camera => (TgxCameraMode7)base.Camera;
    public IReadOnlyList<TgxRotscaleLayerMode7> RotScaleLayers { get; }
    public GbaVram Vram { get; }
}