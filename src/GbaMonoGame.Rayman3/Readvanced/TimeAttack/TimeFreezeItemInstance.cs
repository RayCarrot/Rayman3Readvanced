using System.Text.Json.Serialization;

namespace GbaMonoGame.Rayman3.Readvanced;

public readonly struct TimeFreezeItemInstance
{
    [JsonConstructor]
    public TimeFreezeItemInstance(int time, short x, short y)
    {
        Time = time;
        X = x;
        Y = y;
    }

    [JsonPropertyName("time")]
    public int Time { get; }

    [JsonPropertyName("x")]
    public short X { get; }

    [JsonPropertyName("y")]
    public short Y { get; }
}