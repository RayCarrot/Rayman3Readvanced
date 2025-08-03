using System.Collections.Generic;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class TimeAttackTimes
{
    // TODO: Fill out times
    public static Dictionary<MapId, TimeAttackTime[]> Gba { get; } = new()
    {
        [MapId.WoodLight_M1] =
        [
            new TimeAttackTime(TimeAttackTimeType.Bronze, 2000),
            new TimeAttackTime(TimeAttackTimeType.Silver, 1500),
            new TimeAttackTime(TimeAttackTimeType.Gold, 1000),
        ],
    };

    public static Dictionary<MapId, TimeAttackTime[]> NGage { get; } = new()
    {
        [MapId.WoodLight_M1] =
        [
            new TimeAttackTime(TimeAttackTimeType.Bronze, 2000),
            new TimeAttackTime(TimeAttackTimeType.Silver, 1500),
            new TimeAttackTime(TimeAttackTimeType.Gold, 1000),
        ],
    };
}