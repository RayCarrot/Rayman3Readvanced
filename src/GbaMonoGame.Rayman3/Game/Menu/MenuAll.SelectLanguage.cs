using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Properties

    public int LanguagesCount { get; } = Engine.Settings.Platform switch
    {
        Platform.GBA => 10, // TODO: 3 for US version
        Platform.NGage => 6,
        _ => throw new UnsupportedPlatformException()
    };

    public int LanguagesBaseAnimation { get; } = Engine.Settings.Platform switch
    {
        Platform.GBA => 0, // TODO: 10 for US version
        Platform.NGage => 10 + 3,
        _ => throw new UnsupportedPlatformException()
    };

    #endregion

    #region Steps

    // N-Gage exclusive
    private void Step_InitializeTransitionToSelectLanguage()
    {
        CurrentStepAction = Step_TransitionToSelectLanguage;
        SetBackgroundPalette(1);
        SelectOption(Localization.Language, false);
        Data.LanguageList.CurrentAnimation = LanguagesBaseAnimation + SelectedOption;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        ResetStem();
    }

    // N-Gage exclusive
    private void Step_TransitionToSelectLanguage()
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
            CurrentStepAction = Step_SelectLanguage;
        }

        AnimationPlayer.Play(Data.LanguageList);
    }

    private void Step_SelectLanguage()
    {
        if (Engine.Settings.Platform != Platform.NGage || TransitionsFX.IsFadeInFinished)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Up))
            {
                int selectedOption;
                if (SelectedOption == 0)
                    selectedOption = LanguagesCount - 1;
                else
                    selectedOption = SelectedOption - 1;

                if (Engine.Settings.Platform == Platform.NGage)
                    SelectOption(selectedOption, true);
                else
                    SelectedOption = selectedOption;

                Data.LanguageList.CurrentAnimation = LanguagesBaseAnimation + SelectedOption;

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

                if (Engine.Settings.Platform == Platform.NGage)
                    SelectOption(selectedOption, true);
                else
                    SelectedOption = selectedOption;

                Data.LanguageList.CurrentAnimation = LanguagesBaseAnimation + SelectedOption;

                // TODO: Game passes in 0 as obj here, but that's probably a mistake
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                CurrentStepAction = Step_TransitionOutOfLanguage;

                if (Engine.Settings.Platform == Platform.GBA)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
                }

                Localization.SetLanguage(SelectedOption);

                // TODO: The N-Gage version saves the selected language to the save data here

                TransitionValue = 0;
                SelectedOption = 0;
                PrevSelectedOption = 0;

                if (Engine.Settings.Platform == Platform.GBA)
                {
                    GameLogoYOffset = 56;
                    OtherGameLogoValue = 12;
                    
                    Data.GameModeList.CurrentAnimation = Localization.LanguageUiIndex * 3 + SelectedOption;
                }

                // Center sprites if English
                if (Localization.Language == 0)
                {
                    if (Engine.Settings.Platform == Platform.GBA)
                    {
                        Data.GameModeList.ScreenPos = Data.GameModeList.ScreenPos with { X = 86 };
                        Data.Cursor.ScreenPos = Data.Cursor.ScreenPos with { X = 46 };
                        Data.Stem.ScreenPos = Data.Stem.ScreenPos with { X = 60 };
                    }
                    else if (Engine.Settings.Platform == Platform.NGage)
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

                if (Engine.Settings.Platform == Platform.GBA)
                {
                    ResetStem();
                }
                else if (Engine.Settings.Platform == Platform.NGage)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                    TransitionOutCursorAndStem();
                }
                else
                {
                    throw new UnsupportedPlatformException();
                }
            }
            else if (Engine.Settings.Platform == Platform.NGage && JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                CurrentStepAction = Step_TransitionOutOfLanguage;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                TransitionValue = 0;
                SelectedOption = 0;
                PrevSelectedOption = 0;
                TransitionOutCursorAndStem();
            }
        }

        AnimationPlayer.Play(Data.LanguageList);
    }

    private void Step_TransitionOutOfLanguage()
    {
        if (Engine.Settings.Platform == Platform.GBA)
        {
            TgxCluster mainCluster = Playfield.Camera.GetMainCluster();
            mainCluster.Position += new Vector2(0, 3);

            Data.LanguageList.ScreenPos = Data.LanguageList.ScreenPos with { Y = TransitionValue + 28 };
            Data.LanguageList.FrameChannelSprite();
            AnimationPlayer.Play(Data.LanguageList);

            MoveGameLogo();

            Data.GameLogo.FrameChannelSprite(); // NOTE The game gives the bounding box a width of 255 instead of 240 here
            AnimationPlayer.Play(Data.GameLogo);

            AnimationPlayer.Play(Data.GameModeList);

            if (TransitionValue < -207)
            {
                TransitionValue = 0;
                CurrentStepAction = Step_SelectGameMode;
            }
            else
            {
                TransitionValue -= 3;
            }
        }
        else if (Engine.Settings.Platform == Platform.NGage)
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

            AnimationPlayer.Play(Data.LanguageList);
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    #endregion
}