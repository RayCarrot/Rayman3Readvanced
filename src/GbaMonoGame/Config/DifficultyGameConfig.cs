namespace GbaMonoGame;

public class DifficultyGameConfig : IniSectionObject
{
    public DifficultyGameConfig()
    {
        InfiniteLives = false;
    }

    public override string SectionKey => "Difficulty";

    public bool InfiniteLives { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        InfiniteLives = serializer.Serialize<bool>(InfiniteLives, "InfiniteLives");
    }
}