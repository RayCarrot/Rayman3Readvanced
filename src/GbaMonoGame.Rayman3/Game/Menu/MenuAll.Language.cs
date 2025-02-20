using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Properties

    public int LanguagesCount { get; } = Rom.Platform switch
    {
        Platform.GBA => 10, // TODO: 3 for US version
        Platform.NGage => 6,
        _ => throw new UnsupportedPlatformException()
    };

    public int LanguagesBaseAnimation { get; } = Rom.Platform switch
    {
        Platform.GBA => 0, // TODO: 10 for US version
        Platform.NGage => 10 + 3,
        _ => throw new UnsupportedPlatformException()
    };

    #endregion

    #region Steps

    // N-Gage exclusive
    private void Step_InitializeTransitionToLanguage()
    {
        CurrentStepAction = Step_TransitionToLanguage;
        SetBackgroundPalette(1);
        SelectOption(Localization.LanguageId, false);
        Anims.LanguageList.CurrentAnimation = LanguagesBaseAnimation + SelectedOption;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        ResetStem();
    }

    // N-Gage exclusive
    private void Step_TransitionToLanguage()
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
            CurrentStepAction = Step_Language;
        }

        AnimationPlayer.Play(Anims.LanguageList);
    }

    private void Step_Language()
    {
        if (Rom.Platform != Platform.NGage || TransitionsFX.IsFadeInFinished)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Up))
            {
                int selectedOption;
                if (SelectedOption == 0)
                    selectedOption = LanguagesCount - 1;
                else
                    selectedOption = SelectedOption - 1;

                if (Rom.Platform == Platform.NGage)
                    SelectOption(selectedOption, true);
                else
                    SelectedOption = selectedOption;

                Anims.LanguageList.CurrentAnimation = LanguagesBaseAnimation + SelectedOption;

                // TODO: Game passes in 0 as obj here, but that's probably a mistake
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
            {
                int selectedOption;
                if (SelectedOption == LanguagesCount - 1)
                    selectedOption = 0;
                else
                    selectedOption = SelectedOption + 1;

                if (Rom.Platform == Platform.NGage)
                    SelectOption(selectedOption, true);
                else
                    SelectedOption = selectedOption;

                Anims.LanguageList.CurrentAnimation = LanguagesBaseAnimation + SelectedOption;

                // TODO: Game passes in 0 as obj here, but that's probably a mistake
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                CurrentStepAction = Step_TransitionOutOfLanguage;

                if (Rom.Platform == Platform.GBA)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
                }

                Localization.SetLanguage(SelectedOption);
                Engine.Config.Language = Localization.Language.Locale;
                Engine.SaveConfig();

                TransitionValue = 0;
                SelectedOption = 0;
                PrevSelectedOption = 0;

                if (Rom.Platform == Platform.GBA)
                {
                    GameLogoYOffset = 56;
                    OtherGameLogoValue = 12;
                    
                    Anims.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * 3 + SelectedOption;
                }

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

                if (Rom.Platform == Platform.GBA)
                {
                    ResetStem();
                }
                else if (Rom.Platform == Platform.NGage)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                    TransitionOutCursorAndStem();
                }
                else
                {
                    throw new UnsupportedPlatformException();
                }
            }
            else if (Rom.Platform == Platform.NGage && JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                CurrentStepAction = Step_TransitionOutOfLanguage;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                TransitionValue = 0;
                SelectedOption = 0;
                PrevSelectedOption = 0;
                TransitionOutCursorAndStem();
            }
        }

        AnimationPlayer.Play(Anims.LanguageList);
    }

    private void Step_TransitionOutOfLanguage()
    {
        if (Rom.Platform == Platform.GBA)
        {
            TgxCluster mainCluster = Playfield.Camera.GetMainCluster();
            mainCluster.Position += new Vector2(0, 3);

            Anims.LanguageList.ScreenPos = Anims.LanguageList.ScreenPos with { Y = TransitionValue + 28 };
            Anims.LanguageList.FrameChannelSprite();
            AnimationPlayer.Play(Anims.LanguageList);

            MoveGameLogo();

            Anims.GameLogo.FrameChannelSprite(); // NOTE The game gives the bounding box a width of 255 instead of 240 here
            AnimationPlayer.Play(Anims.GameLogo);

            AnimationPlayer.Play(Anims.GameModeList);

            if (TransitionValue < -207)
            {
                TransitionValue = 0;
                CurrentStepAction = Step_GameMode;
            }
            else
            {
                TransitionValue -= 3;
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            TransitionValue += 4;

            if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
            {
                TgxCluster cluster = Playfield.Camera.GetCluster(1);
                cluster.Position -= new Vector2(0, 4);
            }
            else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
            {
                TransitionValue = 0;
                NextStepAction = Step_InitializeTransitionToOptions;
                CurrentStepAction = Step_InitializeTransitionToOptions;
            }

            AnimationPlayer.Play(Anims.LanguageList);
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    #endregion
}