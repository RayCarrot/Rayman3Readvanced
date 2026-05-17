namespace GbaMonoGame.Rayman3;

public class Act4 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.Rayman3_Act4);
    }

    public override void Step()
    {
        base.Step();

        if (IsFinished)
            Engine.FrameMngr.SetNextFrame(LevelFactory.Create(MapId.World4));
    }
}