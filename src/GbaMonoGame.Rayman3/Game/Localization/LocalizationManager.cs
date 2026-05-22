using System;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public class LocalizationManager
{
    public LocalizationManager(LocalizedTextBanks localizedTextBanks)
    {
        _localizedTextBanks = localizedTextBanks;
    }

    // NOTE: The original level select names have a typo for Deutsch and in N-Gage it swaps Italian and Spanish - these are fixed here
    private static readonly Language[] _gbaLanguages = 
    [
        new(englishName: "English",   gameName: "English",    levelSelectName: "English",     locale: "en",    uiIndex: 0),
        new(englishName: "French",    gameName: "Français",   levelSelectName: "French",      locale: "fr-FR", uiIndex: 1),
        new(englishName: "Spanish",   gameName: "Español",    levelSelectName: "Spanish",     locale: "es-ES", uiIndex: 2),
        new(englishName: "German",    gameName: "Deutsch",    levelSelectName: "Deutsch",     locale: "de-DE", uiIndex: 3),
        new(englishName: "Italian",   gameName: "Italiano",   levelSelectName: "Italian",     locale: "it-IT", uiIndex: 4),
        new(englishName: "Dutch",     gameName: "Nederlands", levelSelectName: "Netherlands", locale: "nl-NL", uiIndex: 5),
        new(englishName: "Swedish",   gameName: "Swedish",    levelSelectName: "Swedish",     locale: "sv-SE", uiIndex: 6),
        new(englishName: "Finnish",   gameName: "Finnish",    levelSelectName: "Finnish",     locale: "fi-FI", uiIndex: 7),
        new(englishName: "Norwegian", gameName: "Norsk",      levelSelectName: "Norwegian",   locale: "nb-NO", uiIndex: 8),
        new(englishName: "Danish",    gameName: "Danish",     levelSelectName: "Danish",      locale: "da-DK", uiIndex: 9),
    ];
    private static readonly Language[] _nGageLanguages = 
    [
        new(englishName: "English",   gameName: "English",    levelSelectName: "English",     locale: "en",    uiIndex: 0),
        new(englishName: "English",   gameName: "English US", levelSelectName: "EnglishUS",   locale: "en-US", uiIndex: 0),
        new(englishName: "French",    gameName: "Français",   levelSelectName: "French",      locale: "fr-FR", uiIndex: 1),
        new(englishName: "Spanish",   gameName: "Español",    levelSelectName: "Spanish",     locale: "es-ES", uiIndex: 2),
        new(englishName: "Italian",   gameName: "Italiano",   levelSelectName: "Italian",     locale: "it-IT", uiIndex: 4),
        new(englishName: "German",    gameName: "Deutsch",    levelSelectName: "Deutsch",     locale: "de-DE", uiIndex: 3),
    ];

    private readonly LocalizedTextBanks _localizedTextBanks;
    private TextBank[] _textBanks;

    public Language Language { get; private set; }
    public int LanguageId { get; private set; }
    public int LanguageUiIndex { get; private set; }

    public Language[] GetLanguages()
    {
        return Rom.Platform switch
        {
            Platform.GBA => _gbaLanguages,
            Platform.NGage => _nGageLanguages,
            _ => throw new UnsupportedPlatformException()
        };
    }

    public string[] GetText(TextBankId bankId, int textId)
    {
        Text text = _textBanks[(int)bankId].Texts[textId];
        
        string[] strings = new string[text.Lines.Count];
        for (int i = 0; i < strings.Length; i++)
            strings[i] = text.Lines[i];

        return strings;
    }

    public int GetLanguageId(string locale)
    {
        Language[] languages = GetLanguages();
        return Array.FindIndex(languages, x => x.Locale == locale);
    }

    public Language GetLanguage(int languageId)
    {
        Language[] languages = GetLanguages();
        return languages[languageId];
    }

    public void SetLanguage(Language language)
    {
        SetLanguage(language.Locale);
    }

    public void SetLanguage(string locale)
    {
        int id = GetLanguageId(locale);

        if (id == -1)
            id = 0;

        SetLanguage(id);
    }

    public void SetLanguage(int languageId)
    {
        Language = GetLanguage(languageId);
        LanguageId = languageId;
        LanguageUiIndex = Language.UiIndex;

        // Get the text banks for the specified language
        List<TextBank> textBanks = new();

        // Get the text banks from the ROM
        foreach (TextBankResource textBankResource in _localizedTextBanks.TextBanks[languageId].Value!)
        {
            List<Text> texts = new();

            foreach (TextResource text in textBankResource.Texts)
            {
                List<string> lines = new();
                foreach (string line in text.Lines.Value)
                    lines.Add(line);
                texts.Add(new Text(lines));
            }

            textBanks.Add(new TextBank(texts));
        }

        // TODO: Don't hard-code this here! Use constants/enums to index.
        // Get the Readvanced text banks
        textBanks.Add(new TextBank(
        [
            new Text(["2:Hit the butterflies to decrease", "the timer."]),
            new Text(["2:Use L and R to move to the side", "without turning."]),
            new Text(["2:Use your body to hit his head", "when he's vulnerable."]),
        ]));

        _textBanks = textBanks.ToArray();
    }
}