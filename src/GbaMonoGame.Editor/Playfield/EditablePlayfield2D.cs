using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Editor;

public class EditablePlayfield2D
{
    public EditablePlayfield2D(Playfield2DResource playfieldResource, EditorCamera camera)
    {
        // Load vram
        Vram = GbaVram.AllocateStatic(playfieldResource.TileKit, playfieldResource.TileMappingTable, 0x180, false, playfieldResource.DefaultPalette);

        List<TgxTileLayer> tileLayers = new();

        // Load the layers
        foreach (GameLayerResource gameLayerResource in playfieldResource.Layers)
        {
            // Only load main tile layers for now
            if (gameLayerResource.Type == GameLayerType.TileLayer && 
                gameLayerResource.TileLayer.LayerId is 2 or 3)
            {
                TgxTileLayer layer = new(camera.RenderContext, gameLayerResource);
                tileLayers.Add(layer);

                layer.LoadRenderer(Vram, playfieldResource.TileKit, null);
                layer.Screen.Wrap = false;

                camera.AddGameLayer(layer);
            }
        }

        TileLayers = tileLayers;
    }

    public IReadOnlyList<TgxTileLayer> TileLayers { get; }
    public GbaVram Vram { get; }
}