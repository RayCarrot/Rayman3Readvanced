using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Properties

    public uint GameLogoPrevMovedTime { get; set; }
    public int GameLogoYOffset { get; set; }
    public int GameLogoYSpeed { get; set; }
    public int GameLogoSinValue { get; set; }
    public int GameLogoMovementXOffset { get; set; }
    public int GameLogoMovementWidth { get; set; }
    public int GameLogoMovementXCountdown { get; set; }

    // Custom values for smooth movement
    public float GameLogoStartX { get; set; }
    public float GameLogoEndX { get; set; }

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

    // Custom for going back to the modern menu
    public bool IsLoadingModernMenu { get; set; }

    #endregion

    #region Private Methods

    private void MoveGameLogo()
    {
        // Move Y
        if (GameLogoYOffset < 56)
        {
            // Move down
            Anims.GameLogo.ScreenPos = Anims.GameLogo.ScreenPos with { Y = GameLogoYOffset * 2 - 54 };
            GameLogoYOffset += 4;
        }
        else if (GameLogoYSpeed != 12)
        {
            // Bounce up and down
            GameLogoSinValue = (GameLogoSinValue + 16) % 256;

            float y = 56 + MathHelpers.Sin256(GameLogoSinValue) * GameLogoYSpeed;
            Anims.GameLogo.ScreenPos = Anims.GameLogo.ScreenPos with { Y = y };

            if (GameLogoYSpeed == 20 && GameLogoSinValue == 96)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Pannel_BigFoot1_Mix02);

            if (GameLogoSinValue == 0)
                GameLogoYSpeed -= 4;
        }
        else if (Anims.GameLogo.ScreenPos.Y > 16)
        {
            // Move up
            Anims.GameLogo.ScreenPos -= new Vector2(0, 1);
        }

        // Move X (back and forth from a width of 10 to 0)
        uint elapsedTime = GameTime.ElapsedFrames - GameLogoPrevMovedTime;
        uint targetTime = GameLogoMovementWidth switch
        {
            10 => 4,
            9 => 6,
            8 => 8,
            7 => 10,
            6 => 12,
            5 => 14,
            4 => 16,
            3 => 18,
            2 => 20,
            1 => 22,
            _ => 0
        };

        // Custom code for smooth movement
        if (targetTime != 0)
            Anims.GameLogo.ScreenPos = Anims.GameLogo.ScreenPos with { X = MathHelper.Lerp(GameLogoStartX, GameLogoEndX, elapsedTime / (float)targetTime) };

        if (targetTime != 0 && elapsedTime > targetTime)
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
            GameLogoPrevMovedTime = GameTime.ElapsedFrames;

            // Custom code for smooth movement
            GameLogoStartX = Anims.GameLogo.ScreenPos.X;
            GameLogoEndX = GameLogoBaseX + x;
        }
    }

    #endregion

    #region Steps

    private void Step_InitializeTransitionToGameMode()
    {
        Anims.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * GameModeOptionsCount + SelectedOption;

        // Center sprites if English
        if (Localization.LanguageId == 0)
        {
            if (Rom.Platform == Platform.GBA)
            {
                Anims.GameModeList.ScreenPos = Anims.GameModeList.ScreenPos with { X = 86 };
                Anims.Cursor.ScreenPos = Anims.Cursor.ScreenPos with { X = 46 };
                Anims.Stem.ScreenPos = Anims.Stem.ScreenPos with { X = 60 };
            }
            else if (Rom.Platform == Platform.NGage)
            {
                Anims.GameModeList.ScreenPos = Anims.GameModeList.ScreenPos with { X = 58 };
                Anims.Cursor.ScreenPos = Anims.Cursor.ScreenPos with { X = 18 };
                Anims.Stem.ScreenPos = Anims.Stem.ScreenPos with { X = 32 };
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }

        // The game does a bit of a hack to skip the transition if we start at the game mode selection
        if (InitialPage == InitialMenuPage.GameMode)
        {
            CurrentStepAction = Step_GameMode;
            InitialPage = InitialMenuPage.Language;
        }
        else
        {
            CurrentStepAction = Step_TransitionToGameMode;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        }

        IsLoadingMultiplayerMap = false;
        GameLogoPrevMovedTime = 0;
        GameLogoMovementXOffset = 10;
        GameLogoMovementWidth = 10;
        GameLogoMovementXCountdown = 0;
        Anims.GameLogo.ScreenPos = Anims.GameLogo.ScreenPos with { X = GameLogoBaseX };
        GameLogoYSpeed = 20;
        GameLogoSinValue = 0;
        GameLogoYOffset = 0;
        GameLogoStartX = Anims.GameLogo.ScreenPos.X;
        GameLogoEndX = Anims.GameLogo.ScreenPos.X;

        ResetStem();
        SetBackgroundPalette(3);

        SelectedOption = 0;
    }

    private void Step_TransitionToGameMode()
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
            CurrentStepAction = Step_GameMode;
        }

        MoveGameLogo();

        // NOTE The game gives the render box a height of 255 instead of 240 here
        Anims.GameLogo.FrameChannelSprite();
        
        AnimationPlayer.Play(Anims.GameLogo);
        AnimationPlayer.Play(Anims.GameModeList);
    }

    private void Step_GameMode()
    {
        if (IsLoadingModernMenu)
        {
            if (!TransitionsFX.IsFadingOut)
                FrameManager.SetNextFrame(new ModernMenuAll(InitialMenuPage.GameMode));
        }
        else
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Up))
            {
                SelectOption(SelectedOption == 0 ? GameModeOptionsCount - 1 : SelectedOption - 1, true);

                Anims.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * GameModeOptionsCount + SelectedOption;
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
            {
                SelectOption(SelectedOption == GameModeOptionsCount - 1 ? 0 : SelectedOption + 1, true);

                Anims.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * GameModeOptionsCount + SelectedOption;
            }
            else if (Engine.LocalConfig.Controls.UseModernButtonMapping switch
                     {
                         true => JoyPad.IsButtonJustPressed(GbaInput.A) || JoyPad.IsButtonJustPressed(GbaInput.Start),
                         false when Rom.Platform is Platform.GBA => JoyPad.IsButtonJustPressed(GbaInput.A),
                         false when Rom.Platform is Platform.NGage => NGageJoyPadHelpers.IsConfirmButtonJustPressed(),
                         _ => throw new UnsupportedPlatformException()
                     })
            {
                Anims.Cursor.CurrentAnimation = 16;

                NextStepAction = SelectedOption switch
                {
                    0 => Step_InitializeTransitionToSinglePlayer,
                    1 when Rom.Platform == Platform.GBA => Step_InitializeTransitionToMultiplayerModeSelection,
                    1 when Rom.Platform == Platform.NGage => Step_InitializeTransitionToMultiplayerConnectionSelection,
                    2 => Step_InitializeTransitionToOptions,

                    3 when Rom.Platform == Platform.NGage => Step_InitializeTransitionToHelp,
                    4 when Rom.Platform == Platform.NGage => Step_InitializeTransitionToQuit,

                    _ => throw new Exception("Wrong game mode selection")
                };

                CurrentStepAction = Step_TransitionOutOfGameMode;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                SelectOption(0, false);
                TransitionValue = 0;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                TransitionOutCursorAndStem();
            }
            // Custom to return to the modern menu
            else if (Engine.LocalConfig.Controls.UseModernButtonMapping switch 
                     { 
                         true => JoyPad.IsButtonJustPressed(GbaInput.B) || JoyPad.IsButtonJustPressed(GbaInput.Select),
                         false when Rom.Platform is Platform.GBA => JoyPad.IsButtonJustPressed(GbaInput.B),
                         false when Rom.Platform is Platform.NGage => NGageJoyPadHelpers.IsBackButtonJustPressed(),
                         _ => throw new UnsupportedPlatformException()
                     })
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                IsLoadingModernMenu = true;
                TransitionsFX.FadeOutInit(4);
            }
        }

        AnimationPlayer.Play(Anims.GameModeList);

        MoveGameLogo();

        // NOTE The game gives the render box a height of 255 instead of 240 here
        Anims.GameLogo.FrameChannelSprite();
        AnimationPlayer.Play(Anims.GameLogo);
    }

    private void Step_TransitionOutOfGameMode()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            TgxCluster cluster = Playfield.Camera.GetCluster(1);
            cluster.Position -= new Vector2(0, 4);
            Anims.GameLogo.ScreenPos = Anims.GameLogo.ScreenPos with { Y = 16 - TransitionValue / 2f };
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        AnimationPlayer.Play(Anims.GameModeList);

        MoveGameLogo();

        // NOTE The game gives the render box a height of 255 instead of 240 here
        Anims.GameLogo.FrameChannelSprite();
        AnimationPlayer.Play(Anims.GameLogo);
    }

    #endregion
}