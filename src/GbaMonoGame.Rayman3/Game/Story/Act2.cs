namespace GbaMonoGame.Rayman3;

public class Act2 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.Rayman3_Act2);
    }

    public override void Step()
    {
        base.Step();

        // TODO: This cutscene doesn't play on N-Gage. What they did was to remove the condition here and have it directly move on to the level.
        if (IsFinished)
            FrameManager.SetNextFrame(LevelFactory.Create(MapId.MarshAwakening1));
    }
}