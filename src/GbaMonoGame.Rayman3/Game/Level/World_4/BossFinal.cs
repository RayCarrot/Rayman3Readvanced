using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class BossFinal : FrameSideScroller
{
    public BossFinal(MapId mapId) : base(mapId) { }

    public override void Step()
    {
        CurrentStepAction();

        if (EndOfFrame)
        {
            if (!TimeAttackInfo.IsActive && GameInfo.MapId == MapId.BossFinal_M2)
                FrameManager.SetNextFrame(new Act6());
            else
                GameInfo.LoadLevel(GameInfo.GetNextLevelId());
        }
    }
}