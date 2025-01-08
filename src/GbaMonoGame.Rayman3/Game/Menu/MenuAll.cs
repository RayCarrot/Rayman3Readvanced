using System;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll : Frame, IHasPlayfield
{
    #region Constructor

    public MenuAll(Page initialPage)
    {
        WheelRotation = 0;
        SelectedOption = 0;
        PrevSelectedOption = 0;
        StartEraseCursorTargetIndex = 0;
        StartEraseCursorCurrentIndex = 0;
        CurrentStepAction = null;
        NextStepAction = null;
        TransitionValue = 0;
        MultiplayerPlayersOffsetY = Engine.Settings.Platform switch
        {
            Platform.GBA => 70,
            Platform.NGage => 100,
            _ => throw new UnsupportedPlatformException()
        };
        SinglePakPlayersOffsetY = 70;
        GameLogoMovementXOffset = 3;
        GameLogoMovementWidth = 6;
        PrevGameTime = 0;
        GameLogoMovementXCountdown = 0;
        GameLogoYOffset = 0;
        StemMode = 0;

        if (Engine.Settings.Platform == Platform.GBA)
        {
            IsMultiplayerConnected = null;
            MultiplayerConnectionTimer = 0;
            MultiplayerLostConnectionTimer = 0;
        }

        MultiplayerType = 0;
        MultiplayerMapId = 0;
        HasProcessedPackets = false;
        IsLoadingMultiplayerMap = false;
        ShouldTextBlink = false;

        if (Engine.Settings.Platform == Platform.GBA)
        {
            FinishedLyChallenge1 = false;
            FinishedLyChallenge2 = false;
            HasAllCages = false;
            ReturningFromMultiplayerGame = false;
        }

        Slots = new Slot[3];
        HasLoadedGameInfo = false;
        IsStartingGame = false;
        InitialPage = initialPage;
        PreviousTextId = 0;

        if (Engine.Settings.Platform == Platform.NGage)
        {
            CaptureTheFlagTargetTime = 360;
            CaptureTheFlagTargetFlagsCount = 3;
            CaptureTheFlagMode = CaptureTheFlagMode.Solo;
            CaptureTheFlagSoloMode = 0;
        }
    }

    #endregion

    #region Properties

    TgxPlayfield IHasPlayfield.Playfield => Playfield;

    public AnimationPlayer AnimationPlayer { get; set; }
    public TgxPlayfield2D Playfield { get; set; }
    public TransitionsFX TransitionsFX { get; set; }

    public MenuData Data { get; set; }
    public Action CurrentStepAction { get; set; }
    public Action NextStepAction { get; set; }

    public float CursorBaseY { get; } = Engine.Settings.Platform switch
    {
        Platform.GBA => 67,
        Platform.NGage => 77,
        _ => throw new UnsupportedPlatformException()
    };

    public int PrevSelectedOption { get; set; }
    public int SelectedOption { get; set; }
    public int StemMode { get; set; }

    public bool ShouldTextBlink { get; set; }
    public int PreviousTextId { get; set; }
    public int NextTextId { get; set; }

    public int TransitionValue { get; set; }
    public uint PrevGameTime { get; set; }
    public int WheelRotation { get; set; }
    public int SteamTimer { get; set; }

    public Page InitialPage { get; set; }

    public bool IsLoadingMultiplayerMap { get; set; }

    public bool HasLoadedGameInfo { get; set; }
    public Slot[] Slots { get; }
    public bool IsStartingGame { get; set; }

    public bool FinishedLyChallenge1 { get; set; }
    public bool FinishedLyChallenge2 { get; set; }
    public bool HasAllCages { get; set; }

    #endregion

    #region Methods

    public void SetText(int textId, bool blink)
    {
        ShouldTextBlink = blink;

        string[] text = Localization.GetText(11, textId);

        int unusedLines = Data.Texts.Length - text.Length;
        for (int i = 0; i < Data.Texts.Length; i++)
        {
            if (i < unusedLines)
            {
                Data.Texts[i].Text = "";
            }
            else
            {
                Data.Texts[i].Text = text[i - unusedLines];
                Data.Texts[i].ScreenPos = new Vector2(140 - Data.Texts[i].GetStringWidth() / 2f, 32 + i * 16);
            }
        }
    }

    // N-Gage uses a more complex method for setting text, with wrapping and optional params
    public void NGageSetText(int textId, bool blink, int? baseY, int maxLineWidth, params object[] paramsBuffer)
    {
        ShouldTextBlink = blink;

        string[] text = Localization.GetText(11, textId);

        baseY ??= text.Length * -16 + 96;

        int paramIndex = 0;
        int textIndex;
        for (textIndex = 0; textIndex < text.Length && textIndex < Data.Texts.Length; textIndex++)
        {
            // Get the string
            string str = text[textIndex];

            // Get the amount of params
            int paramsCount = str.Count(x => x == '%');

            // Get the params
            object[] strParams = new object[paramsCount];
            Array.Copy(paramsBuffer, paramIndex, strParams, 0, paramsCount);

            // Increment the param index
            paramIndex += paramsCount;

            // Replace the params in the string
            if (paramsCount != 0)
                str = str.sprintf(strParams);

            // Single line
            if (maxLineWidth < 1 || textIndex != text.Length - 1 || text.Length >= Data.Texts.Length)
            {
                drawText(textIndex, str);
            }
            // Wrap the string
            else
            {
                // Try to wrap the string at a spaces
                int currentLineWidth = 0;
                int wrapIndex = 0;
                int i = 0;
                while (currentLineWidth < maxLineWidth)
                {
                    bool reachedTheEnd = false;

                    i = str.IndexOf(' ', i);
                    if (i == -1)
                    {
                        i = str.Length;
                        reachedTheEnd = true;
                    }

                    currentLineWidth = FontManager.GetStringWidth(FontSize.Font16, str[..i]);

                    if (currentLineWidth < maxLineWidth)
                    {
                        wrapIndex = i;
                        i++;
                    }

                    if (reachedTheEnd)
                        break;
                }

                // Wrapping at a space found
                if (wrapIndex != 0)
                {
                    // Draw first line
                    drawText(textIndex, str[..wrapIndex]);

                    // Set index for second line if we end with a space (and skip the space)
                    if (wrapIndex < str.Length)
                    {
                        // Draw second line
                        textIndex++;
                        drawText(textIndex, str[(wrapIndex + 1)..]);
                    }
                }
                // No wrapping at a space found
                else
                {
                    // Find any wrapping index
                    currentLineWidth = 0;
                    wrapIndex = 0;
                    i = 0;
                    while (currentLineWidth < maxLineWidth)
                    {
                        bool reachedTheEnd = false;

                        i++;

                        if (i == str.Length)
                            reachedTheEnd = true;

                        currentLineWidth = FontManager.GetStringWidth(FontSize.Font16, str[..i]);

                        if (currentLineWidth < maxLineWidth)
                        {
                            wrapIndex = i;
                            i++;
                        }

                        if (reachedTheEnd)
                            break;
                    }

                    // Draw first line
                    drawText(textIndex, str[..wrapIndex]);

                    // Draw second line
                    textIndex++;
                    drawText(textIndex, str[wrapIndex..]);
                }
            }
        }

        // Set unused lines to be empty
        for (; textIndex < Data.Texts.Length; textIndex++)
            Data.Texts[textIndex].Text = "";

        // Helper method for drawing text
        void drawText(int index, string str)
        {
            int lineWidth = FontManager.GetStringWidth(FontSize.Font16, str);
            Data.Texts[index].ScreenPos = new Vector2(108 - lineWidth / 2f, baseY.Value + 48 + index * 16);
            Data.Texts[index].Text = str;
        }
    }

    public void DrawText(bool front)
    {
        if (!ShouldTextBlink || (GameTime.ElapsedFrames & 0x10) != 0)
        {
            foreach (SpriteTextObject text in Data.Texts)
            {
                if (front)
                    AnimationPlayer.PlayFront(text);
                else
                    AnimationPlayer.Play(text);
            }
        }
    }

    public static RGB555Color[] GetBackgroundPalette(int index)
    {
        RGB555Color[] colors = index switch
        {
            0 => new RGB555Color[]
            {
                new(0x25ee), new(0x8ba), new(0x1dae), new(0x1dae), new(0x2632), new(0x2211), new(0x21cf),
                new(0x196b), new(0x154a), new(0x3695), new(0x2e54), new(0x198c), new(0x10e7), new(0x1509),
                new(0x21f0), new(0x196c), new(0x1d8d), new(0x3f21),
            },
            1 => new RGB555Color[]
            {
                new(0x2653), new(0x249d), new(0x1db2), new(0x1d91), new(0x2216), new(0x21f5), new(0x1dd3),
                new(0x196f), new(0x154d), new(0x369a), new(0x2e58), new(0x1970), new(0x10e9), new(0x150b),
                new(0x21f4), new(0x196f), new(0x1990), new(0x23a2),
            },
            2 => new RGB555Color[]
            {
                new(0x3f28), new(0x6568), new(0x3d4d), new(0x394c), new(0x4990), new(0x498f), new(0x416e),
                new(0x310b), new(0x2d0a), new(0x5a34), new(0x55d2), new(0x352b), new(0x20a7), new(0x28e8),
                new(0x456f), new(0x352b), new(0x392c), new(0x1d97),
            },
            3 => new RGB555Color[]
            {
                new(0x29b2), new(0x645f), new(0x2111), new(0x2110), new(0x2955), new(0x2534), new(0x2532),
                new(0x1cee), new(0x18cc), new(0x3df8), new(0x3197), new(0x1cef), new(0x14a9), new(0x18cb),
                new(0x2533), new(0x1cee), new(0x1cef), new(0x7ca),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };

        RGB555Color[] allColors = new RGB555Color[49];
        Array.Fill(allColors, new RGB555Color(), 0, 31);
        Array.Copy(colors, 0, allColors, 31, colors.Length);
        return allColors;
    }

    public void SetBackgroundPalette(int index)
    {
        GbaVram vram = Playfield.Vram;
        TextureScreenRenderer renderer = (TextureScreenRenderer)Playfield.TileLayers[0].Screen.Renderer;
        
        renderer.PaletteTexture = new PaletteTexture(
            Texture: Engine.TextureCache.GetOrCreateObject(
                pointer: vram.SelectedPalette.CachePointer,
                id: index + 1, // +1 since 0 is the default
                data: index,
                createObjFunc: static i => new PaletteTexture2D(GetBackgroundPalette(i))),
            PaletteIndex: 0);
    }

    public void ResetStem()
    {
        StemMode = 1;
        Data.Stem.CurrentAnimation = 12;
    }

    public void ManageCursorAndStem()
    {
        if (StemMode == 0)
        {
            if (Data.Cursor.CurrentAnimation == 16)
            {
                if (Data.Cursor.EndOfAnimation)
                {
                    Data.Cursor.CurrentAnimation = 0;

                    if (Data.Cursor.ScreenPos.Y <= CursorBaseY)
                    {
                        Data.Stem.CurrentAnimation = 15;
                    }
                }
            }
            else if (Data.Cursor.ScreenPos.Y > CursorBaseY)
            {
                Data.Cursor.ScreenPos -= new Vector2(0, 4);

                if (Data.Cursor.ScreenPos.Y <= CursorBaseY)
                {
                    Data.Cursor.ScreenPos = Data.Cursor.ScreenPos with { Y = CursorBaseY };
                    Data.Stem.CurrentAnimation = 15;
                }
            }
            else if (Data.Stem.CurrentAnimation == 15 && Data.Stem.EndOfAnimation)
            {
                Data.Stem.CurrentAnimation = 14;
                StemMode = 3;
            }
        }
        else if (StemMode == 1)
        {
            if (Data.Stem.CurrentAnimation == 12 && Data.Stem.EndOfAnimation)
            {
                Data.Stem.CurrentAnimation = 17;
            }
            else if (Data.Stem.CurrentAnimation == 17 && Data.Stem.EndOfAnimation)
            {
                Data.Stem.CurrentAnimation = 1;
                StemMode = 2;
            }
        }
        else if (StemMode == 2)
        {
            int lineHeight;
            if (CurrentStepAction == Step_SinglePlayer)
                lineHeight = 18;
            else if (CurrentStepAction == Step_MultiplayerMapSelection)
                lineHeight = 20;
            else
                lineHeight = 16;

            if (SelectedOption != PrevSelectedOption)
            {
                if (SelectedOption < PrevSelectedOption)
                {
                    float yPos = SelectedOption * lineHeight + CursorBaseY;

                    if (yPos < Data.Cursor.ScreenPos.Y)
                    {
                        Data.Cursor.ScreenPos -= new Vector2(0, 4);
                    }
                    else
                    {
                        Data.Cursor.ScreenPos = Data.Cursor.ScreenPos with { Y = yPos };
                        PrevSelectedOption = SelectedOption;
                    }
                }
                else
                {
                    float yPos = SelectedOption * lineHeight + CursorBaseY;

                    if (yPos > Data.Cursor.ScreenPos.Y)
                    {
                        Data.Cursor.ScreenPos += new Vector2(0, 4);
                    }
                    else
                    {
                        Data.Cursor.ScreenPos = Data.Cursor.ScreenPos with { Y = yPos };
                        PrevSelectedOption = SelectedOption;
                    }
                }
            }
        }

        AnimationPlayer.Play(Data.Stem);

        // The cursor is usually included in the stem animation, except for animation 1
        if (Data.Stem.CurrentAnimation == 1)
            AnimationPlayer.Play(Data.Cursor);
    }

    public void TransitionOutCursorAndStem()
    {
        if (Engine.Settings.Platform == Platform.NGage || StemMode is 2 or 3)
        {
            PrevSelectedOption = SelectedOption;
            SelectedOption = 0;
        }

        StemMode = 0;

        Data.Stem.CurrentAnimation = 1;

        if (Data.Cursor.ScreenPos.Y <= CursorBaseY && Data.Cursor.CurrentAnimation != 16)
            Data.Stem.CurrentAnimation = 15;
    }

    public void SelectOption(int selectedOption, bool playSound)
    {
        if (Engine.Settings.Platform == Platform.NGage || StemMode is 2 or 3)
        {
            PrevSelectedOption = SelectedOption;
            SelectedOption = selectedOption;

            if (playSound)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }
    }

    public void LoadGameInfo()
    {
        if (HasLoadedGameInfo)
            return;

        GameInfo.Init();
        HasLoadedGameInfo = true;

        for (int i = 0; i < 3; i++)
        {
            if (Engine.Settings.Platform == Platform.NGage && !Engine.SaveGame.ValidSlots[i])
                continue;

            GameInfo.Load(i);

            if (GameInfo.PersistentInfo.Lives != 0)
                Slots[i] = new Slot(GameInfo.GetTotalCollectedYellowLums(), GameInfo.GetTotalCollectedCages(), GameInfo.PersistentInfo.Lives);
            else
                Slots[i] = null;

            if (Engine.Settings.Platform == Platform.GBA)
            {
                if (GameInfo.PersistentInfo.FinishedLyChallenge1)
                    FinishedLyChallenge1 = true;

                if (GameInfo.PersistentInfo.FinishedLyChallenge2)
                    FinishedLyChallenge2 = true;

                if (Slots[i]?.CagesCount == 50)
                    HasAllCages = true;
            }
        }
    }

    public override void Init()
    {
        LoadGameInfo();

        AnimationPlayer = new AnimationPlayer(false, null);

        Data = new MenuData(MultiplayerPlayersOffsetY, SinglePakPlayersOffsetY);
        WheelRotation = 0;

        PlayfieldResource menuPlayField = Storage.LoadResource<PlayfieldResource>(GameResource.MenuPlayfield);
        Playfield = TgxPlayfield.Load<TgxPlayfield2D>(menuPlayField);
        Engine.GameViewPort.SetResolutionBoundsToOriginalResolution();

        Gfx.ClearColor = Color.Black;

        Playfield.Camera.GetMainCluster().Position = Vector2.Zero;
        Playfield.Camera.GetCluster(1).Position = new Vector2(0, 160);
        Playfield.Camera.GetCluster(2).Position = Vector2.Zero;

        Playfield.Step();

        switch (InitialPage)
        {
            case Page.SelectLanguage:
                CurrentStepAction = Engine.Settings.Platform switch
                {
                    Platform.GBA => Step_SelectLanguage,
                    Platform.NGage => Step_InitializeTransitionToSelectLanguage,
                    _ => throw new UnsupportedPlatformException()
                };
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
                break;

            case Page.SelectGameMode:
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Step_InitializeTransitionToSelectGameMode;
                break;

            case Page.Options:
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Step_InitializeTransitionToOptions;
                break;

            case Page.Multiplayer:
                IsLoadingMultiplayerMap = true;
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Engine.Settings.Platform switch
                {
                    Platform.GBA => Step_InitializeTransitionToMultiplayerPlayerSelection,
                    Platform.NGage => Step_InitializeTransitionToMultiplayerTypeSelection,
                    _ => throw new UnsupportedPlatformException()
                };
                break;

            case Page.MultiplayerLostConnection:
                IsLoadingMultiplayerMap = true;
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Step_InitializeTransitionToMultiplayerLostConnection;
                break;

            // N-Gage exclusive
            case Page.NGage_FirstPage:
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Step_InitializeFirstPage;
                break;
        }

        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__raytheme) &&
            !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__sadslide))
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__raytheme);
            SoundEngineInterface.SetNbVoices(10);
        }

        RSMultiplayer.UnInit();
        RSMultiplayer.Init();

        if (Engine.Settings.Platform == Platform.GBA)
            MultiplayerInititialGameTime = GameTime.ElapsedFrames;
        
        MultiplayerInfo.Init();
        MultiplayerManager.Init();

        GameTime.Resume();

        TransitionsFX = new TransitionsFX(false);
        TransitionsFX.FadeInInit(1 / 16f);

        SteamTimer = 0;
    }

    public override void UnInit()
    {
        SoundEngineInterface.SetNbVoices(7);
        Playfield.UnInit();

        if (!IsLoadingMultiplayerMap)
        {
            RSMultiplayer.UnInit();
            GameTime.Resume();
        }

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__raytheme);
    }

    public override void Step()
    {
        Playfield.Step();
        TransitionsFX.StepAll();
        AnimationPlayer.Execute();

        CurrentStepAction();

        if (Engine.Settings.Platform == Platform.NGage || CurrentStepAction != Step_SelectLanguage)
            ManageCursorAndStem();

        if (Engine.Settings.Platform == Platform.NGage)
        {
            Data.SelectSymbol.CurrentAnimation = Localization.LanguageUiIndex;
            Data.BackSymbol.CurrentAnimation = 5 + Localization.LanguageUiIndex;

            Data.BackSymbol.ScreenPos = Data.BackSymbol.ScreenPos with
            {
                X = Localization.LanguageUiIndex switch
                {
                    1 => 121,
                    2 => 126,
                    3 => 123,
                    4 => 114,
                    _ => 135,
                }
            };

            AnimationPlayer.PlayFront(Data.SelectSymbol);

            if (CurrentStepAction != Step_SelectGameMode &&
                CurrentStepAction != Step_InitializeTransitionToSelectGameMode &&
                CurrentStepAction != Step_TransitionToSelectGameMode &&
                CurrentStepAction != Step_TransitionOutOfSelectGameMode)
            {
                AnimationPlayer.PlayFront(Data.BackSymbol);
            }
        }

        WheelRotation += 4;

        if (WheelRotation >= 2048)
            WheelRotation = 0;

        if (Engine.Settings.Platform == Platform.GBA)
        {
            Data.Wheel1.AffineMatrix = new AffineMatrix(WheelRotation % 256, 1, 1);
            Data.Wheel2.AffineMatrix = new AffineMatrix(255 - WheelRotation / 2f % 256, 1, 1);
            Data.Wheel3.AffineMatrix = new AffineMatrix(WheelRotation / 4f % 256, 1, 1);
            Data.Wheel4.AffineMatrix = new AffineMatrix(WheelRotation / 8f % 256, 1, 1);

            AnimationPlayer.Play(Data.Wheel1);
            AnimationPlayer.Play(Data.Wheel2);
            AnimationPlayer.Play(Data.Wheel3);
            AnimationPlayer.Play(Data.Wheel4);

            if (SteamTimer == 0)
            {
                if (!Data.Steam.EndOfAnimation)
                {
                    AnimationPlayer.Play(Data.Steam);
                }
                else
                {
                    SteamTimer = Random.GetNumber(180) + 60; // Value between 60 and 240
                    Data.Steam.CurrentAnimation = Random.GetNumber(200) < 100 ? 0 : 1;
                }
            }
            else
            {
                SteamTimer--;
            }
        }
        else if (Engine.Settings.Platform == Platform.NGage)
        {
            Data.Wheel2.AffineMatrix = new AffineMatrix(255 - WheelRotation / 2f % 256, 1, 1);
            Data.Wheel4.AffineMatrix = new AffineMatrix(WheelRotation / 8f % 256, 1, 1);

            AnimationPlayer.Play(Data.Wheel2);
            AnimationPlayer.Play(Data.Wheel4);
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    #endregion

    #region Steps

    // N-Gage exclusive
    private void Step_InitializeFirstPage()
    {
        InitialPage = Page.SelectLanguage;

        // TODO: If the game has failed to load the save file then it transitions to a page where it says the drive is full - re-implement?

        CurrentStepAction = Step_InitializeTransitionToSelectGameMode;
    }

    #endregion

    #region Data Types

    public enum Page
    {
        SelectLanguage,
        SelectGameMode,
        Options,
        Multiplayer,
        MultiplayerLostConnection,
        NGage_FirstPage,
    }

    public record Slot(int LumsCount, int CagesCount, int LivesCount);

    #endregion
}