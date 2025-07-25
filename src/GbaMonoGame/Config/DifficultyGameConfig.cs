namespace GbaMonoGame;

public record DifficultyGameConfig : IniSectionObject
{
    public DifficultyGameConfig()
    {
        InfiniteLives = false;
        NoInstaKills = false;
        KeepLumsInRaces = false;
        NoCheckpoints = false;
        OneHitPoint = false;
    }

    public override string SectionKey => "Difficulty";

    public bool InfiniteLives { get; set; }
    public bool NoInstaKills { get; set; }
    public bool KeepLumsInRaces { get; set; }
    public bool NoCheckpoints { get; set; }
    public bool OneHitPoint { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        InfiniteLives = serializer.Serialize<bool>(InfiniteLives, "InfiniteLives");
        NoInstaKills = serializer.Serialize<bool>(NoInstaKills, "NoInstaKills");
        KeepLumsInRaces = serializer.Serialize<bool>(KeepLumsInRaces, "KeepLumsInRaces");
        NoCheckpoints = serializer.Serialize<bool>(NoCheckpoints, "NoCheckpoints");
        OneHitPoint = serializer.Serialize<bool>(OneHitPoint, "OneHitPoint");
    }
}