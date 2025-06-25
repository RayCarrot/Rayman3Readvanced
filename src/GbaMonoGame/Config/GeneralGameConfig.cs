namespace GbaMonoGame;

public class GeneralGameConfig : IniSectionObject
{
    public GeneralGameConfig()
    {
        LastPlayedGbaSaveSlot = null;
        LastPlayedNGageSaveSlot = null;
    }

    public override string SectionKey => "General";

    public int? LastPlayedGbaSaveSlot { get; set; }
    public int? LastPlayedNGageSaveSlot { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        LastPlayedGbaSaveSlot = serializer.Serialize<int?>(LastPlayedGbaSaveSlot, "LastPlayedGbaSaveSlot");
        LastPlayedNGageSaveSlot = serializer.Serialize<int?>(LastPlayedNGageSaveSlot, "LastPlayedNGageSaveSlot");
    }
}