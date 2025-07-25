namespace GbaMonoGame;

public abstract record IniSectionObject
{
    public abstract string SectionKey { get; }

    public abstract void Serialize(BaseIniSerializer serializer);
}