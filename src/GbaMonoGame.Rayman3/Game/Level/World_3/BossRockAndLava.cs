using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class BossRockAndLava : FrameSideScroller
{
    public BossRockAndLava(MapId mapId) : base(mapId) { }

    public override void Init()
    {
        base.Init();

        Rayman3Achievements.BossRockAndLava_HasUsedBlueLum = false;
    }
}