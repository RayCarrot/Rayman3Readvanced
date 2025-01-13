using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class WoodLight_M1 : FrameSideScroller
{
    public WoodLight_M1(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        Scene.AddDialog(new TextBoxDialog(Scene), false, false);

        // TODO: Add config option for scrolling on N-Gage
        TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
        TextureScreenRenderer renderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
        if (Rom.Platform == Platform.GBA)
        {
            cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(renderer.Texture, [32, 120, 227])
            {
                PaletteTexture = renderer.PaletteTexture
            };
        }
        else
        {
            // Need to limit the background to 256 since the rest is just transparent
            renderer.TextureRectangle = renderer.TextureRectangle with { Width = 256 };
        }
    }
}