using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
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

        // The red pirate enemies are enabled by default even though they're linked to a captor. This
        // is not normally visible in the original game due to the small screen width, but here it
        // becomes very noticeable in high resolution.
        if (Engine.ActiveConfig.Tweaks.FixBugs)
        {
            Scene.GetGameObject(77).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(80).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(82).ProcessMessage(this, Message.Destroy);
        }
    }
}