namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackLevelMenuOption : TextMenuOption
{
    public TimeAttackLevelMenuOption(MapId mapId, float scale = 1f) : base(GetLevelName(mapId), scale) { }

    private static string GetLevelName(MapId map)
    {
        int textId = GameInfo.Levels[(int)map].NameTextId;
        string name = Localization.GetText(TextBankId.LevelNames, textId)[0];
        return name.ToUpperInvariant().Replace('’', '\''); // TODO: Improve (’ is not in the font)
    }
}