namespace GbaMonoGame;

public record DebugGameConfig : IniSectionObject
{
    public DebugGameConfig()
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