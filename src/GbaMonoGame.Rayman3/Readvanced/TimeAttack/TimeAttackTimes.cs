using System.Collections.Frozen;
using System.Collections.Generic;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class TimeAttackTimes
{
    // TODO: Fill out times
    public static FrozenDictionary<MapId, TimeAttackTime[]> Gba { get; } = new Dictionary<MapId, TimeAttackTime[]>()
    {
        [MapId.WoodLight_M1] =
        [
            new TimeAttackTime(TimeAttackTimeType.Bronze, 60 * 60),
            new TimeAttackTime(TimeAttackTimeType.Silver, 50 * 60),
            new TimeAttackTime(TimeAttackTimeType.Gold, 45 * 60),
        ],
    }.ToFrozenDictionary();

    public static FrozenDictionary<MapId, TimeAttackTime[]> NGage { get; } = new Dictionary<MapId, TimeAttackTime[]>
    {
        [MapId.WoodLight_M1] =
        [
            new TimeAttackTime(TimeAttackTimeType.Bronze, 60 * 60),
            new TimeAttackTime(TimeAttackTimeType.Silver, 50 * 60),
            new TimeAttackTime(TimeAttackTimeType.Gold, 45 * 60),
        ],
    }.ToFrozenDictionary();
}