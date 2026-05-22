namespace GbaMonoGame.Rayman3;

public class Act5 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.ReadStoryAct(5));
    }

    public override void Step()
    {
        base.Step();

        if (IsFinished)
            Engine.FrameMngr.SetNextFrame(LevelFactory.Create(MapId.PirateShip_M1));
    }
}