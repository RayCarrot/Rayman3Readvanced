using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Properties

    public int GameLogoYOffset { get; set; }
    public int OtherGameLogoValue { get; set; }
    public int GameLogoSinValue { get; set; }
    public int GameLogoMovementXOffset { get; set; }
    public int GameLogoMovementWidth { get; set; }
    public int GameLogoMovementXCountdown { get; set; }

    public float GameLogoBaseX { get; } = Rom.Platform switch
    {
        Platform.GBA => 174,
        Platform.NGage => 110,
        _ => throw new UnsupportedPlatformException()
    };

    public int GameModeOptionsCount { get; } = Rom.Platform switch
    {
        Platform.GBA => 3,
        Platform.NGage => 5,
        _ => throw new UnsupportedPlatformException()
    };

    #endregion

    #region Private Methods

    private void MoveGameLogo()
    {
        // Move Y
        if (GameLogoYOffset < 56)
        {
            Data.GameLogo.ScreenPos = Data.GameLogo.ScreenPos with { Y = GameLogoYOffset * 2 - 54 };
            GameLogoYOffset += 4;
        }
        else if (OtherGameLogoValue != 12)
        {
            GameLogoSinValue = (GameLogoSinValue + 16) % 256;

            float y = 56 + MathHelpers.Sin256(GameLogoSinValue) * OtherGameLogoValue;
            Data.GameLogo.ScreenPos = Data.GameLogo.ScreenPos with { Y = y };

            if (OtherGameLogoValue == 20 && GameLogoSinValue == 96)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Pannel_BigFoot1_Mix02);

            if (GameLogoSinValue == 0)
                OtherGameLogoValue -= 4;
        }
        else if (Data.GameLogo.ScreenPos.Y > 16)
        {
            Data.GameLogo.ScreenPos -= new Vector2(0, 1);
        }

        // TODO: Rewrite with floats to move in 60fps
        // Move X (back and forth from a width of 10 to 0)
        uint time = GameTime.ElapsedFrames - PrevGameTime;
        if (time > 4 && GameLogoMovementWidth == 10 ||
            time > 6 && GameLogoMovementWidth == 9 ||
            time > 8 && GameLogoMovementWidth == 8 ||
            time > 10 && GameLogoMovementWidth == 7 ||
            time > 12 && GameLogoMovementWidth == 6 ||
            time > 14 && GameLogoMovementWidth == 5 ||
            time > 16 && GameLogoMovementWidth == 4 ||
            time > 18 && GameLogoMovementWidth == 3 ||
            time > 20 && GameLogoMovementWidth == 2 ||
            time > 22 && GameLogoMovementWidth == 1)
        {
            int x;

            if (GameLogoMovementXOffset < GameLogoMovementWidth * 2)
            {
                x = GameLogoMovementXOffset - GameLogoMovementWidth;
            }
            else if (GameLogoMovementXOffset < GameLogoMovementWidth * 4)
            {
                x = GameLogoMovementWidth * 3 - GameLogoMovementXOffset;
            }
            else
            {
                GameLogoMovementXOffset = 0;
                if (GameLogoMovementXCountdown == 2)
                {
                    GameLogoMovementWidth--;
                    GameLogoMovementXCountdown = 0;
                }
                else
                {
                    GameLogoMovementXCountdown++;
                }

                x = -GameLogoMovementWidth;
            }

            GameLogoMovementXOffset++;
            PrevGameTime = GameTime.ElapsedFrames;
            Data.GameLogo.ScreenPos = Data.GameLogo.ScreenPos with { X = GameLogoBaseX + x };
        }
    }

    #endregion

    #region Steps

    private void Step_InitializeTransitionToSelectGameMode()
    {
        Data.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * GameModeOptionsCount + SelectedOption;

        // Center sprites if English
        if (Localization.Language == 0)
        {
            if (Rom.Platform == Platform.GBA)
            {
                Data.GameModeList.ScreenPos = Data.GameModeList.ScreenPos with { X = 86 };
                Data.Cursor.ScreenPos = Data.Cursor.ScreenPos with { X = 46 };
                Data.Stem.ScreenPos = Data.Stem.ScreenPos with { X = 60 };
            }
            else if (Rom.Platform == Platform.NGage)
            {
                Data.GameModeList.ScreenPos = Data.GameModeList.ScreenPos with { X = 58 };
                Data.Cursor.ScreenPos = Data.Cursor.ScreenPos with { X = 18 };
                Data.Stem.ScreenPos = Data.Stem.ScreenPos with { X = 32 };
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }

        // The game does a bit of a hack to skip the transition if we start at the game mode selection
        if (InitialPage == Page.SelectGameMode)
        {
            CurrentStepAction = Step_SelectGameMode;
            InitialPage = Page.SelectLanguage;
        }
        else
        {
            CurrentStepAction = Step_TransitionToSelectGameMode;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        }

        IsLoadingMultiplayerMap = false;
        PrevGameTime = 0;
        GameLogoMovementXOffset = 10;
        GameLogoMovementWidth = 10;
        GameLogoMovementXCountdown = 0;
        Data.GameLogo.ScreenPos = Data.GameLogo.ScreenPos with { X = GameLogoBaseX };
        OtherGameLogoValue = 20;
        GameLogoSinValue = 0;
        GameLogoYOffset = 0;

        ResetStem();
        SetBackgroundPalette(3);

        SelectedOption = 0;
    }

    private void Step_TransitionToSelectGameMode()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position += new Vector2(0, 8);
        }

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_SelectGameMode;
        }

        MoveGameLogo();

        Data.GameLogo.FrameChannelSprite(); // NOTE The game gives the bounding box a width of 255 instead of 240 here
        
        AnimationPlayer.Play(Data.GameLogo);
        AnimationPlayer.Play(Data.GameModeList);
    }

    private void Step_SelectGameMode()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up))
        {
            SelectOption(SelectedOption == 0 ? GameModeOptionsCount - 1 : SelectedOption - 1, true);

            Data.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * GameModeOptionsCount + SelectedOption;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SelectOption(SelectedOption == GameModeOptionsCount - 1 ? 0 : SelectedOption + 1, true);

            Data.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * GameModeOptionsCount + SelectedOption;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            Data.Cursor.CurrentAnimation = 16;

            NextStepAction = SelectedOption switch
            {
                0 => Step_InitializeTransitionToSinglePlayer,
                1 when Rom.Platform == Platform.GBA => Step_InitializeTransitionToMultiplayerModeSelection,
                1 when Rom.Platform == Platform.NGage => Step_InitializeTransitionToMultiplayerConnectionSelection,
                2 => Step_InitializeTransitionToOptions,

                3 when Rom.Platform == Platform.NGage => Step_InitializeTransitionToHelp,
                4 when Rom.Platform == Platform.NGage => Step_InitializeTransitionToQuit,
                
                _ => throw new Exception("Invalid game mode")
            };

            CurrentStepAction = Step_TransitionOutOfSelectGameMode;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
            SelectOption(0, false);
            TransitionValue = 0;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
            TransitionOutCursorAndStem();
        }

        AnimationPlayer.Play(Data.GameModeList);

        MoveGameLogo();

        Data.GameLogo.FrameChannelSprite(); // NOTE The game gives the bounding box a width of 255 instead of 240 here
        AnimationPlayer.Play(Data.GameLogo);
    }

    private void Step_TransitionOutOfSelectGameMode()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position -= new Vector2(0, 4);
            Data.GameLogo.ScreenPos = Data.GameLogo.ScreenPos with { Y = 16 - TransitionValue / 2f };
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        AnimationPlayer.Play(Data.GameModeList);

        MoveGameLogo();

        Data.GameLogo.FrameChannelSprite(); // NOTE The game gives the bounding box a width of 255 instead of 240 here
        AnimationPlayer.Play(Data.GameLogo);
    }

    #endregion
}