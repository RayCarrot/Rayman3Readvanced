namespace GbaMonoGame;

public record DebugGameSettings : IniSectionObject
{
    public DebugGameSettings()
    {
        DebugModeEnabled = false;
        WriteSerializerLog = false;
    }

    public override string SectionKey => "Debug";

    public bool DebugModeEnabled { get; set; }
    public bool WriteSerializerLog { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        DebugModeEnabled = serializer.Serialize<bool>(DebugModeEnabled, "DebugModeEnabled");
        WriteSerializerLog = serializer.Serialize<bool>(WriteSerializerLog, "WriteSerializerLog");
    }
}