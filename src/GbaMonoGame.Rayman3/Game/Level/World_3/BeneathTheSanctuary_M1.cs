using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class BeneathTheSanctuary_M1 : FrameSideScroller
{
    public BeneathTheSanctuary_M1(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        // Deactivate the captors since they get triggered by the switch. The N-Gage version doesn't
        // do this since the trigger code in the Switch actor uses the wrong object IDs, but that
        // causes an oversight where the captor with ID 132 remains active, even though it should be
        // unused as it's not connected to a switch.
        if (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.FixBugs)
        {
            Scene.GetGameObject(129).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(130).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(131).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(132).ProcessMessage(this, Message.Destroy);
            Scene.GetGameObject(133).ProcessMessage(this, Message.Destroy);
        }
    }
}