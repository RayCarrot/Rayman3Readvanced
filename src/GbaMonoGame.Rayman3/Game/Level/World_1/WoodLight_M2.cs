using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class WoodLight_M2 : FrameSideScroller
{
    public WoodLight_M2(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        Scene.AddDialog(new TextBoxDialog(Scene), false, false);

        // TODO: Add config option for scrolling on N-Gage
        if (Rom.Platform == Platform.GBA)
        {
            TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
            TextureScreenRenderer renderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
            cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(renderer.Texture, [15, 71, 227]);
        }
    }
}