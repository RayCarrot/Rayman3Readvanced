namespace GbaMonoGame.Rayman3;

public class BossFinal : FrameSideScroller
{
    public BossFinal(MapId mapId) : base(mapId) { }

    public override void Step()
    {
        CurrentStepAction();

        if (EndOfFrame)
        {
            if (GameInfo.MapId == MapId.BossFinal_M2)
                Engine.FrameMngr.SetNextFrame(new Act6());
            else
                GameInfo.LoadLevel(GameInfo.GetNextLevelId());
        }
    }
}