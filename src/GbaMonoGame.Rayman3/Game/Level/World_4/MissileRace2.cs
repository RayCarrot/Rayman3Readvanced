namespace GbaMonoGame.Rayman3;

public class MissileRace2 : FrameSingleMode7
{
    public MissileRace2(MapId mapId) : base(mapId, [70, 65, 60]) { }

    public override void Init()
    {
        base.Init();

        ExtendMap(
        [
            new(1), new(2), new(3),
            new(4), new(5), new(6),
            new(7), new(8), new(9)
        ], 3, 3);
    }
}