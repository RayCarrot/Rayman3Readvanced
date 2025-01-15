namespace GbaMonoGame.Rayman3;

public class Language
{
    public Language(string englishName, string gameName, string levelSelectName, string locale, int uiIndex)
    {
        EnglishName = englishName;
        GameName = gameName;
        LevelSelectName = levelSelectName;
        Locale = locale;
        UiIndex = uiIndex;
    }

    public string EnglishName { get; }
    public string GameName { get; }
    public string LevelSelectName { get; }
    public string Locale { get; }
    public int UiIndex { get; }
}