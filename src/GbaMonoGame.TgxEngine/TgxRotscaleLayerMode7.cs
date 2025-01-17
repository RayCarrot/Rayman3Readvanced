using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class TgxRotscaleLayerMode7 : TgxGameLayer
{
    public TgxRotscaleLayerMode7(GameLayerResource gameLayerResource) : base(gameLayerResource)
    {
        Resource = gameLayerResource.RotscaleLayerMode7;

        TileMap = Resource.TileMap;
        BaseTileIndex = 512;
        Is8Bit = true;
        LayerId = Resource.LayerId;

        // TODO: Set render context
        Screen = new GfxScreen(LayerId)
        {
            IsEnabled = true,
            Offset = Vector2.Zero,
            Priority = 3 - LayerId,
            Wrap = true,
            Is8Bit = Is8Bit,
            IsAlphaBlendEnabled = Resource.HasAlphaBlending,
            GbaAlpha = Resource.AlphaCoeff,
        };

        Gfx.AddScreen(Screen);

        Vector2_0x20 = new Vector2(126, 126);
        Vector2_0x24 = new Vector2(120, 120);
    }

    public RotscaleLayerMode7Resource Resource { get; }
    public GfxScreen Screen { get; }
    public MapTile[] TileMap { get; }
    public int BaseTileIndex { get; }
    public bool Is8Bit { get; }
    public byte LayerId { get; }

    // TODO: Name
    public Vector2 Vector2_0x20 { get; set; }
    public Vector2 Vector2_0x24 { get; set; }

    public override void SetOffset(Vector2 offset)
    {
        Screen.Offset = offset;
    }

    public void LoadRenderer(TileKit tileKit, GbaVram vram)
    {
        // TODO: Use indexed texture with palette texture
        // TODO: This doesn't work with animated tiles! We probably need to use the TileMap renderer and convert the
        //       TileMap like Ray1Map does (since it's static and not dynamic).
        Texture2D tex = Engine.TextureCache.GetOrCreateObject(
            pointer: Resource.Offset,
            id: 0,
            data: (Vram: vram, Layer: this, BaseTileIndex: BaseTileIndex),
            createObjFunc: static data => new TiledTexture2D(data.Layer.Width, data.Layer.Height, data.Vram.TileSet, data.Layer.TileMap, data.BaseTileIndex, data.Vram.SelectedPalette, true));
        Screen.Renderer = new TextureScreenRenderer(tex);
    }
}