namespace GbaMonoGame.Rayman3;

public class BossFinal : FrameSideScroller
{
    public BossFinal(MapId mapId) : base(mapId) { }

    public override void Step()
    {
        CurrentStepAction();

        if (EndOfFrame)
        {
            if (Rayman3.GameInfo.MapId == MapId.BossFinal_M2)
                Engine.FrameMngr.SetNextFrame(new Act6());
            else
                Rayman3.GameInfo.LoadLevel(Rayman3.GameInfo.GetNextLevelId());
        }
    }
}