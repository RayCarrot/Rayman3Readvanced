using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TgxPlayfield2D : TgxPlayfield
{
    public TgxPlayfield2D(Playfield2DResource playfieldResource) 
        : base(new TgxCamera2D(CreateRenderContext(playfieldResource)), playfieldResource.TileKit)
    {
        List<TgxTileLayer> tileLayers = new();

        // Add clusters to the camera
        foreach (ClusterResource clusterResource in playfieldResource.Clusters)
            Camera.AddCluster(clusterResource);

        // Load tiles
        GfxTileKitManager.LoadTileKit(playfieldResource.TileKit, playfieldResource.TileMappingTable, 0x180, false, playfieldResource.DefaultPalette);

        // Load the layers
        foreach (GameLayerResource gameLayerResource in playfieldResource.Layers)
        {
            if (gameLayerResource.Type == GameLayerType.TileLayer)
            {
                // Get the render context for this cluster. Different clusters can have different ones
                // depending on how they scale. The game doesn't do this as it has no concept of scaling.
                TgxCluster cluster = Camera.GetCluster(gameLayerResource.TileLayer.ClusterIndex);
                RenderContext renderContext = cluster.RenderContext;

                TgxTileLayer layer = new(renderContext, gameLayerResource);
                tileLayers.Add(layer);

                layer.LoadRenderer(GfxTileKitManager, AnimatedTilekitManager);
                
                // The game does this in the layer constructor, but it's easier here since we have access to the camera
                Camera.AddLayer(gameLayerResource.TileLayer.ClusterIndex, layer);
            }
            else if (gameLayerResource.Type == GameLayerType.PhysicalLayer)
            {
                PhysicalLayer = new TgxTilePhysicalLayer(RenderContext, gameLayerResource);

                // We want the debug collision map to scroll with the main cluster
                Camera.AddLayer(0, PhysicalLayer);
            }
        }

        TileLayers = tileLayers;
    }

    private static Playfield2DRenderContext CreateRenderContext(Playfield2DResource resource)
    {
        Cluster mainClusterResource = resource.Clusters[0];
        Vector2 size = new(mainClusterResource.SizeX * Tile.Size, mainClusterResource.SizeY * Tile.Size);
        return new Playfield2DRenderContext(null, size);
    }

    public new TgxCamera2D Camera => (TgxCamera2D)base.Camera;
    public new Playfield2DRenderContext RenderContext => (Playfield2DRenderContext)base.RenderContext;
    public Vector2 Size => Camera.GetMainCluster().Size;
    public IReadOnlyList<TgxTileLayer> TileLayers { get; }
}