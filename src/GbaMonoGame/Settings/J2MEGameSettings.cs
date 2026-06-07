namespace GbaMonoGame;

public record J2MEGameSettings : IniSectionObject
{
    public J2MEGameSettings()
    {
        InternalGameResolution = Resolution.J2MEModern;
    }

    public override string SectionKey => "J2ME";

    public Vector2 InternalGameResolution { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        InternalGameResolution = serializer.Serialize<Vector2>(InternalGameResolution, "InternalGameResolution");
    }
}