using System.Text.Json.Serialization;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackConfig
{
    [JsonPropertyName("levels")]
    public TimeAttackLevelInfo[] Levels { get; init; }
}