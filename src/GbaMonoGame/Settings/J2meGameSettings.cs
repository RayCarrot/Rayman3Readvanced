namespace GbaMonoGame;

public record J2meGameSettings : IniSectionObject
{
    public J2meGameSettings()
    {
        InternalGameResolution = Resolution.J2meModern;
        FixBugs = true;
    }

    public override string SectionKey => "J2ME";

    public Vector2 InternalGameResolution { get; set; }
    public bool FixBugs { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        InternalGameResolution = serializer.Serialize<Vector2>(InternalGameResolution, "InternalGameResolution");
        FixBugs = serializer.Serialize<bool>(FixBugs, "FixBugs");
    }
}