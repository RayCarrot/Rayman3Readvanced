using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class NGageAscension : FrameSideScroller
{
    public NGageAscension(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage)
        {
            TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
            TextureScreenRenderer renderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
            cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(renderer.Texture, [32, 120, 227]);
        }
    }
}