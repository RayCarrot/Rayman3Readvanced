using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class BossScaleMan : FrameSideScroller
{
    public BossScaleMan(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        if (!TimeAttackInfo.IsActive)
        {
            Scene.Camera.LinkedObject = Scene.GetGameObject<MovableActor>(1);
            Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
        }
    }
}