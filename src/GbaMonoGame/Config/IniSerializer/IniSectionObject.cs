namespace GbaMonoGame;

public abstract class IniSectionObject
{
    public abstract string SectionKey { get; }

    public abstract void Serialize(BaseIniSerializer serializer);
}