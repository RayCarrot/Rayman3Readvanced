using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class MissileRace1 : FrameSingleMode7
{
    public MissileRace1(MapId mapId) : base(mapId, [60, 55, 50]) { }

    public override void Init()
    {
        base.Init();

        AddWalls(new Point(1, 22), new Point(3, 3));

        ExtendMap(
        [
            new(2), new(3), new(4),
            new(6), new(1), new(7),
            new(8), new(5), new(9)
        ], 3, 3);
    }
}