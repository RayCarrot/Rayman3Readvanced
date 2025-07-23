using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class BossBadDreams : FrameSideScroller
{
    public BossBadDreams(MapId mapId) : base(mapId) { }

    public override void Step()
    {
        base.Step();

        if (EndOfFrame)
        {
            // Show boss ending cutscene first time
            if (!TimeAttackInfo.IsActive && GameInfo.PersistentInfo.LastCompletedLevel == (int)MapId.BossBadDreams)
                FrameManager.SetNextFrame(new Act3());
            else
                GameInfo.LoadLevel(GameInfo.GetNextLevelId());
        }
    }
}