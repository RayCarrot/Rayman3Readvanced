using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3;

public class Act2 : Act
{
    public override void Init()
    {
        Init(Rom.Loader.ReadStoryAct(2));
    }

    public override void Step()
    {
        base.Step();

        // NOTE: The N-Gage version immediately skips this cutscene
        if (IsFinished || Rom.Platform == Platform.NGage)
            Engine.FrameMngr.SetNextFrame(LevelFactory.Create(MapId.MarshAwakening1));
    }
}