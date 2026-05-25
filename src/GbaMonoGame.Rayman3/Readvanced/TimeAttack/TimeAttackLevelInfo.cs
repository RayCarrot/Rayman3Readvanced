using System.Collections.Generic;
using System.Text.Json.Serialization;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackLevelInfo
{
    [JsonConstructor]
    public TimeAttackLevelInfo(
        MapId level, 
        int world, 
        TimeAttackTime[] targetTimes, 
        Dictionary<MapId, TimeFreezeItemInstance[]> actors, 
        Platform? exclusivePlatform = null)
    {
        Level = level;
        World = world;
        TargetTimes = targetTimes;
        Actors = actors;
        ExclusivePlatform = exclusivePlatform;
    }

    [JsonPropertyName("level")] 
    public MapId Level { get; }
    
    [JsonPropertyName("world")] 
    public int World { get; }
    
    [JsonPropertyName("target_times")]
    public TimeAttackTime[] TargetTimes { get; }
    
    [JsonPropertyName("actors")] 
    public Dictionary<MapId, TimeFreezeItemInstance[]> Actors { get; }

    [JsonPropertyName("exclusive_platform")]
    [JsonConverter(typeof(JsonStringEnumConverter<Platform>))]
    public Platform? ExclusivePlatform { get; }
}