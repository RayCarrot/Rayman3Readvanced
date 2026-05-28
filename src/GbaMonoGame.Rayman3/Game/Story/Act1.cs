namespace GbaMonoGame.Rayman3;

public class Act1 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.ReadStoryAct(1));
    }

    public override void Step()
    {
        base.Step();

        if (IsFinished)
            Rayman3.GameInfo.LoadLevel(MapId.WoodLight_M1);
    }
}