using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackTimeJsonConverter : JsonConverter<int>
{
    public override int Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        string str = reader.GetString()!;
        int separatorIndex1 = str.IndexOf(':');
        int separatorIndex2 = str.IndexOf('.');

        int minutes = Int32.Parse(str[..separatorIndex1]);
        int seconds = Int32.Parse(str[(separatorIndex1 + 1)..separatorIndex2]);
        int centiseconds = Int32.Parse(str[(separatorIndex2 + 1)..]);

        return (minutes * 60 * 60) + (seconds * 60) + (centiseconds * 60 / 100);
    }

    // No need to implement writing
    public override void Write(
        Utf8JsonWriter writer,
        int value,
        JsonSerializerOptions options)
    {
        throw new JsonException();
    }
}