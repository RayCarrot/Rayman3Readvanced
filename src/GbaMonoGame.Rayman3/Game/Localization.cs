using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public static class Localization
{
    private static TextBank[] _textBanks;

    public static int Language { get; private set; }
    public static int LanguageUiIndex { get; private set; }
    
    public static string[] GetText(int bankId, int textId)
    {
        Text text = _textBanks[bankId].Texts[textId];
        
        string[] strings = new string[text.LinesCount];
        for (int i = 0; i < strings.Length; i++)
            strings[i] = text.Lines.Value[i];

        return strings;
    }

    public static void SetLanguage(int language)
    {
        Language = language;
        LanguageUiIndex = language; // TODO: On N-Gage this is different!

        _textBanks = Rom.Loader.Rayman3_LocalizedTextBanks.TextBanks[language].Value.Select(x => x.Value).ToArray();
    }
}