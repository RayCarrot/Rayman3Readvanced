namespace GbaMonoGame.Rayman3;

public class Act1 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.Rayman3_Act1);
    }

    public override void Step()
    {
        base.Step();

        if (IsFinished)
            GameInfo.LoadLevel(MapId.WoodLight_M1);
    }
}