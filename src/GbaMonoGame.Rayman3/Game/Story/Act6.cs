namespace GbaMonoGame.Rayman3;

public class Act6 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.Rayman3_Act6);
    }

    public override void Step()
    {
        base.Step();

        if (IsFinished)
            FrameManager.SetNextFrame(new Credits());
    }
}