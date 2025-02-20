using System;
using System.Diagnostics;
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
        MultiplayerPlayersOffsetY = Rom.Platform switch
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

        if (Rom.Platform == Platform.GBA)
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

        if (Rom.Platform == Platform.GBA)
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

        if (Rom.Platform == Platform.NGage)
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

    public MenuAllAnimations Anims { get; set; }
    public Action CurrentStepAction { get; set; }
    public Action NextStepAction { get; set; }

    public float CursorBaseY { get; } = Rom.Platform switch
    {
        Platform.GBA => 67,
        Platform.NGage => 77,
        _ => throw new UnsupportedPlatformException()
    };

    public int PrevSelectedOption { get; set; }
    public int SelectedOption { get; set; }
    public int StemMode { get; set; } // TODO: Enum

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

    public void SetMenuText(int textId, bool blink)
    {
        ShouldTextBlink = blink;

        string[] text = Localization.GetText(11, textId);

        Debug.Assert(text.Length <= Anims.Texts.Length, "Too many lines for this text");

        int unusedLines = Anims.Texts.Length - text.Length;
        for (int i = 0; i < Anims.Texts.Length; i++)
        {
            if (i < unusedLines)
            {
                Anims.Texts[i].Text = "";
            }
            else
            {
                Anims.Texts[i].Text = text[i - unusedLines];
                Anims.Texts[i].ScreenPos = new Vector2(140 - Anims.Texts[i].GetStringWidth() / 2f, 32 + i * 16);
            }
        }
    }

    // N-Gage uses a more complex method for setting text, with wrapping and optional params
    public void NGageSetMenuText(int textId, bool blink, int? baseY, int maxLineWidth, params object[] paramsBuffer)
    {
        ShouldTextBlink = blink;

        string[] text = Localization.GetText(11, textId);

        baseY ??= text.Length * -16 + 96;

        int paramIndex = 0;
        int textIndex;
        for (textIndex = 0; textIndex < text.Length && textIndex < Anims.Texts.Length; textIndex++)
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
            if (maxLineWidth < 1 || textIndex != text.Length - 1 || text.Length >= Anims.Texts.Length)
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
        for (; textIndex < Anims.Texts.Length; textIndex++)
            Anims.Texts[textIndex].Text = "";

        // Helper method for drawing text
        void drawText(int index, string str)
        {
            int lineWidth = FontManager.GetStringWidth(FontSize.Font16, str);
            Anims.Texts[index].ScreenPos = new Vector2(108 - lineWidth / 2f, baseY.Value + 48 + index * 16);
            Anims.Texts[index].Text = str;
        }
    }

    public void DrawText(bool front)
    {
        if (!ShouldTextBlink || (GameTime.ElapsedFrames & 0x10) != 0)
        {
            foreach (SpriteTextObject text in Anims.Texts)
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
        GfxTileKitManager tileKitManager = Playfield.GfxTileKitManager;
        GfxScreen screen = Playfield.TileLayers[0].Screen;

        screen.RenderOptions.PaletteTexture = new PaletteTexture(
            Texture: Engine.TextureCache.GetOrCreateObject(
                pointer: tileKitManager.SelectedPalette.CachePointer,
                id: index + 1, // +1 since 0 is the default
                data: index,
                createObjFunc: static i => new PaletteTexture2D(GetBackgroundPalette(i))),
            PaletteIndex: 0);
    }

    public void ResetStem()
    {
        StemMode = 1;
        Anims.Stem.CurrentAnimation = 12;
    }

    public void ManageCursorAndStem()
    {
        if (StemMode == 0)
        {
            if (Anims.Cursor.CurrentAnimation == 16)
            {
                Debug.Assert(Anims.Stem.CurrentAnimation == 1, "The steam has the wrong animation");

                if (Anims.Cursor.EndOfAnimation)
                {
                    Anims.Cursor.CurrentAnimation = 0;

                    if (Anims.Cursor.ScreenPos.Y <= CursorBaseY)
                    {
                        Anims.Stem.CurrentAnimation = 15;
                    }
                }
            }
            else if (Anims.Cursor.ScreenPos.Y > CursorBaseY)
            {
                Anims.Cursor.ScreenPos -= new Vector2(0, 4);

                if (Anims.Cursor.ScreenPos.Y <= CursorBaseY)
                {
                    Anims.Cursor.ScreenPos = Anims.Cursor.ScreenPos with { Y = CursorBaseY };
                    Anims.Stem.CurrentAnimation = 15;
                }
            }
            else if (Anims.Stem.CurrentAnimation == 15 && Anims.Stem.EndOfAnimation)
            {
                Anims.Stem.CurrentAnimation = 14;
                StemMode = 3;
            }
        }
        else if (StemMode == 1)
        {
            if (Anims.Stem.CurrentAnimation == 12 && Anims.Stem.EndOfAnimation)
            {
                Anims.Stem.CurrentAnimation = 17;
            }
            else if (Anims.Stem.CurrentAnimation == 17 && Anims.Stem.EndOfAnimation)
            {
                Anims.Stem.CurrentAnimation = 1;
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

                    if (yPos < Anims.Cursor.ScreenPos.Y)
                    {
                        Anims.Cursor.ScreenPos -= new Vector2(0, 4);
                    }
                    else
                    {
                        Anims.Cursor.ScreenPos = Anims.Cursor.ScreenPos with { Y = yPos };
                        PrevSelectedOption = SelectedOption;
                    }
                }
                else
                {
                    float yPos = SelectedOption * lineHeight + CursorBaseY;

                    if (yPos > Anims.Cursor.ScreenPos.Y)
                    {
                        Anims.Cursor.ScreenPos += new Vector2(0, 4);
                    }
                    else
                    {
                        Anims.Cursor.ScreenPos = Anims.Cursor.ScreenPos with { Y = yPos };
                        PrevSelectedOption = SelectedOption;
                    }
                }
            }
        }

        AnimationPlayer.Play(Anims.Stem);

        // The cursor is usually included in the stem animation, except for animation 1
        if (Anims.Stem.CurrentAnimation == 1)
            AnimationPlayer.Play(Anims.Cursor);
    }

    public void TransitionOutCursorAndStem()
    {
        if (Rom.Platform == Platform.NGage || StemMode is 2 or 3)
        {
            PrevSelectedOption = SelectedOption;
            SelectedOption = 0;
        }

        StemMode = 0;

        Anims.Stem.CurrentAnimation = 1;

        if (Anims.Cursor.ScreenPos.Y <= CursorBaseY && Anims.Cursor.CurrentAnimation != 16)
            Anims.Stem.CurrentAnimation = 15;
    }

    public void SelectOption(int selectedOption, bool playSound)
    {
        if (Rom.Platform == Platform.NGage || StemMode is 2 or 3)
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
            if (SaveGameManager.SlotExists(i))
            {
                // Load the slot
                GameInfo.Load(i);

                // Get the info from the slot
                Slots[i] = new Slot(GameInfo.GetTotalDeadLums(), GameInfo.GetTotalDeadCages(), GameInfo.PersistentInfo.Lives);

                if (Rom.Platform == Platform.GBA)
                {
                    if (GameInfo.PersistentInfo.FinishedLyChallenge1)
                        FinishedLyChallenge1 = true;

                    if (GameInfo.PersistentInfo.FinishedLyChallenge2)
                        FinishedLyChallenge2 = true;

                    if (Slots[i]?.CagesCount == 50)
                        HasAllCages = true;
                }
            }
            else
            {
                Slots[i] = null;
            }
        }
    }

    public override void Init()
    {
        LoadGameInfo();

        AnimationPlayer = new AnimationPlayer(false, null);

        Anims = new MenuAllAnimations(Rom.OriginalGameRenderContext, MultiplayerPlayersOffsetY, SinglePakPlayersOffsetY);
        WheelRotation = 0;

        PlayfieldResource menuPlayField = Rom.LoadResource<PlayfieldResource>(GameResource.MenuPlayfield);
        Playfield = TgxPlayfield.Load<TgxPlayfield2D>(menuPlayField);
        Playfield.RenderContext.SetFixedResolution(Rom.OriginalResolution);

        Gfx.ClearColor = Color.Black;

        Playfield.Camera.GetMainCluster().Position = Vector2.Zero;
        Playfield.Camera.GetCluster(1).Position = new Vector2(0, 160);
        Playfield.Camera.GetCluster(2).Position = Vector2.Zero;

        Playfield.Step();

        switch (InitialPage)
        {
            case Page.Language:
                // NOTE: The game doesn't do this, but this allows the saved language to be pre-selected
                SelectedOption = Localization.LanguageId;
                Anims.LanguageList.CurrentAnimation = LanguagesBaseAnimation + SelectedOption;

                CurrentStepAction = Rom.Platform switch
                {
                    Platform.GBA => Step_Language,
                    Platform.NGage => Step_InitializeTransitionToLanguage,
                    _ => throw new UnsupportedPlatformException()
                };
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
                break;

            case Page.GameMode:
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Step_InitializeTransitionToGameMode;
                break;

            case Page.Options:
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Step_InitializeTransitionToOptions;
                break;

            case Page.Multiplayer:
                IsLoadingMultiplayerMap = true;
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Rom.Platform switch
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
            case Page.NGage_FirstPage when Rom.Platform == Platform.NGage:
                Playfield.TileLayers[3].Screen.IsEnabled = false;
                CurrentStepAction = Step_InitializeFirstPage;
                break;

            default:
                throw new Exception("Invalid start page for MenuAll");
        }

        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__raytheme) &&
            !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__sadslide))
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__raytheme);
            SoundEngineInterface.SetNbVoices(10);
        }

        RSMultiplayer.UnInit();
        RSMultiplayer.Init();

        if (Rom.Platform == Platform.GBA)
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

        if (!IsLoadingMultiplayerMap)
        {
            RSMultiplayer.UnInit();
            GameTime.Resume();
        }

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__raytheme);

        Playfield.UnInit();
    }

    public override void Step()
    {
        Playfield.Step();
        TransitionsFX.StepAll();
        AnimationPlayer.Execute();

        CurrentStepAction();

        if (Rom.Platform == Platform.NGage || CurrentStepAction != Step_Language)
            ManageCursorAndStem();

        if (Rom.Platform == Platform.NGage)
        {
            Anims.SelectSymbol.CurrentAnimation = Localization.LanguageUiIndex;
            Anims.BackSymbol.CurrentAnimation = 5 + Localization.LanguageUiIndex;

            Anims.BackSymbol.ScreenPos = Anims.BackSymbol.ScreenPos with
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

            AnimationPlayer.PlayFront(Anims.SelectSymbol);

            if (CurrentStepAction != Step_GameMode &&
                CurrentStepAction != Step_InitializeTransitionToGameMode &&
                CurrentStepAction != Step_TransitionToGameMode &&
                CurrentStepAction != Step_TransitionOutOfGameMode)
            {
                AnimationPlayer.PlayFront(Anims.BackSymbol);
            }
        }

        WheelRotation += 4;

        if (WheelRotation >= 2048)
            WheelRotation = 0;

        if (Rom.Platform == Platform.GBA)
        {
            Anims.Wheel1.AffineMatrix = new AffineMatrix(WheelRotation % 256, 1, 1);
            Anims.Wheel2.AffineMatrix = new AffineMatrix(255 - WheelRotation / 2f % 256, 1, 1);
            Anims.Wheel3.AffineMatrix = new AffineMatrix(WheelRotation / 4f % 256, 1, 1);
            Anims.Wheel4.AffineMatrix = new AffineMatrix(WheelRotation / 8f % 256, 1, 1);

            AnimationPlayer.Play(Anims.Wheel1);
            AnimationPlayer.Play(Anims.Wheel2);
            AnimationPlayer.Play(Anims.Wheel3);
            AnimationPlayer.Play(Anims.Wheel4);

            if (SteamTimer == 0)
            {
                if (!Anims.Steam.EndOfAnimation)
                {
                    AnimationPlayer.Play(Anims.Steam);
                }
                else
                {
                    SteamTimer = Random.GetNumber(180) + 60; // Value between 60 and 240
                    Anims.Steam.CurrentAnimation = Random.GetNumber(200) < 100 ? 0 : 1;
                }
            }
            else
            {
                SteamTimer--;
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Anims.Wheel2.AffineMatrix = new AffineMatrix(255 - WheelRotation / 2f % 256, 1, 1);
            Anims.Wheel4.AffineMatrix = new AffineMatrix(WheelRotation / 8f % 256, 1, 1);

            AnimationPlayer.Play(Anims.Wheel2);
            AnimationPlayer.Play(Anims.Wheel4);
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
        InitialPage = Page.Language;

        // TODO: If the game has failed to load the save file then it transitions to a page where it says the drive is full - re-implement?

        CurrentStepAction = Step_InitializeTransitionToGameMode;
    }

    #endregion

    #region Data Types

    public enum Page
    {
        Language,
        GameMode,
        Options,
        Multiplayer,
        MultiplayerLostConnection,
        NGage_FirstPage,
    }

    public record Slot(int LumsCount, int CagesCount, int LivesCount);

    #endregion
}