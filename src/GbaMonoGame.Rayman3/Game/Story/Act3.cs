namespace GbaMonoGame.Rayman3;

public class Act3 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.ReadStoryAct(3));
    }

    public override void Step()
    {
        base.Step();

        if (IsFinished)
            Rayman3.GameInfo.LoadLevel(Rayman3.GameInfo.GetNextLevelId());
    }
}