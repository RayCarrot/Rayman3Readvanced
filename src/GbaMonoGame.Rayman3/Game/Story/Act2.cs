using BinarySerializer.Ubisoft.GbaEngine;

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

        // TODO: The cutscene still exists on N-Gage! Allow it to be played?
        if (IsFinished || Rom.Platform == Platform.NGage)
            FrameManager.SetNextFrame(LevelFactory.Create(MapId.MarshAwakening1));
    }
}