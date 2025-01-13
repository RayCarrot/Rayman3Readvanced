using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class BeneathTheSanctuary_M1 : FrameSideScroller
{
    public BeneathTheSanctuary_M1(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        // Why is this here and only on GBA?
        if (Rom.Platform == Platform.GBA)
        {
            Scene.GetGameObject(129).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(130).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(131).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(132).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(133).ProcessMessage(this, Message.Destroy);
        }
    }
}