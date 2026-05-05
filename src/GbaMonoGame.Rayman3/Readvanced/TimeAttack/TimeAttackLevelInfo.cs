using System.Collections.Frozen;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public struct TimeAttackLevelInfo
{
    public TimeAttackLevelInfo(
        MapId level, 
        int world, 
        TimeAttackTime[] targetTimes, 
        FrozenDictionary<MapId, TimeFreezeItemResource[]> actors, 
        Platform? exclusivePlatform = null)
    {
        Level = level;
        World = world;
        TargetTimes = targetTimes;
        Actors = actors;
        ExclusivePlatform = exclusivePlatform;
    }

    public MapId Level { get; }
    public int World { get; }
    public TimeAttackTime[] TargetTimes { get; }
    public FrozenDictionary<MapId, TimeFreezeItemResource[]> Actors { get; } 
    public Platform? ExclusivePlatform { get; }
}