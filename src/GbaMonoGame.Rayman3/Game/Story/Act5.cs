namespace GbaMonoGame.Rayman3;

public class Act5 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.Rayman3_Act5);
    }

    public override void Step()
    {
        base.Step();

        if (IsFinished)
            FrameManager.SetNextFrame(LevelFactory.Create(MapId.PirateShip_M1));
    }
}