using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TgxPlayfield2D : TgxPlayfield
{
    public TgxPlayfield2D(Playfield2DResource playfieldResource) : base(new TgxCamera2D(Engine.GameRenderContext), playfieldResource.TileKit)
    {
        List<TgxTileLayer> tileLayers = new();

        // Add clusters to the camera
        foreach (ClusterResource clusterResource in playfieldResource.Clusters)
            Camera.AddCluster(clusterResource);

        // Load vram
        Vram = GbaVram.AllocateStatic(playfieldResource.TileKit, playfieldResource.TileMappingTable, 0x180, false, playfieldResource.DefaultPalette);

        // Load the layers
        foreach (GameLayerResource gameLayerResource in playfieldResource.Layers)
        {
            if (gameLayerResource.Type == GameLayerType.TileLayer)
            {
                TgxTileLayer layer = new(gameLayerResource);
                tileLayers.Add(layer);

                layer.LoadRenderer(playfieldResource.TileKit, Vram);
                
                // The game does this in the layer constructor, but it's easier here since we have access to the camera
                Camera.AddLayer(gameLayerResource.TileLayer.ClusterIndex, layer);

                // Set if the layer is scaled. The game doesn't do this as it has no concept of scaling.
                TgxCluster cluster = Camera.GetCluster(gameLayerResource.TileLayer.ClusterIndex);
                layer.Screen.RenderContext = cluster.RenderContext;

                // Add the renderer to the animated tile kit manager
                if (layer.Screen.Renderer is TileMapScreenRenderer renderer)
                    AnimatedTilekitManager?.AddRenderer(renderer);
            }
            else if (gameLayerResource.Type == GameLayerType.PhysicalLayer)
            {
                PhysicalLayer = new TgxTilePhysicalLayer(gameLayerResource);
                PhysicalLayer.DebugScreen.RenderContext = RenderContext;

                // We want the debug collision map to scroll with the main cluster
                Camera.AddLayer(0, PhysicalLayer);
            }
        }

        TileLayers = tileLayers;
    }

    public new TgxCamera2D Camera => (TgxCamera2D)base.Camera;
    public Vector2 Size => Camera.GetMainCluster().Size;
    public IReadOnlyList<TgxTileLayer> TileLayers { get; }
    public GbaVram Vram { get; }
}