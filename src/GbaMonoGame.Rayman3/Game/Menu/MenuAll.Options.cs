using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Properties

    public bool IsLoadingCredits { get; set; }

    public int OptionsOptionsCount { get; } = Rom.Platform switch
    {
        Platform.GBA => 3,
        Platform.NGage => 4,
        _ => throw new UnsupportedPlatformException()
    };

    #endregion

    #region Private Methods

    private void UpdateMusicVolumeAnimations()
    {
        if (Rom.Platform == Platform.GBA)
        {
            if (IsMusicOn())
            {
                switch (Localization.LanguageId)
                {
                    case 0:
                    case 1:
                    case 4:
                    case 7:
                    case 9:
                        Anims.MusicVolume.CurrentAnimation = 5;
                        break;

                    case 2:
                        Anims.MusicVolume.CurrentAnimation = 25;
                        break;

                    case 3:
                        Anims.MusicVolume.CurrentAnimation = 19;
                        break;

                    case 5:
                        Anims.MusicVolume.CurrentAnimation = 27;
                        break;

                    case 6:
                        Anims.MusicVolume.CurrentAnimation = 23;
                        break;

                    case 8:
                        Anims.MusicVolume.CurrentAnimation = 21;
                        break;
                }
            }
            else
            {
                switch (Localization.LanguageId)
                {
                    case 0:
                    case 1:
                    case 4:
                    case 7:
                    case 9:
                        Anims.MusicVolume.CurrentAnimation = 6;
                        break;

                    case 2:
                        Anims.MusicVolume.CurrentAnimation = 24;
                        break;

                    case 3:
                        Anims.MusicVolume.CurrentAnimation = 18;
                        break;

                    case 5:
                        Anims.MusicVolume.CurrentAnimation = 26;
                        break;

                    case 6:
                        Anims.MusicVolume.CurrentAnimation = 22;
                        break;

                    case 8:
                        Anims.MusicVolume.CurrentAnimation = 20;
                        break;
                }
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            switch (((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume)
            {
                case 0:
                    Anims.MusicVolume.CurrentAnimation = 28;
                    break;

                case 0x20:
                    Anims.MusicVolume.CurrentAnimation = 29;
                    break;

                case 0x40:
                    Anims.MusicVolume.CurrentAnimation = 30;
                    break;

                case 0x80:
                    Anims.MusicVolume.CurrentAnimation = 31;
                    break;
            }
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private void UpdateSfxVolumeAnimations()
    {
        if (Rom.Platform == Platform.GBA)
        {
            if (IsSfxOn())
            {
                switch (Localization.LanguageId)
                {
                    case 0:
                    case 1:
                    case 4:
                    case 7:
                    case 9:
                        Anims.SfxVolume.CurrentAnimation = 5;
                        break;

                    case 2:
                        Anims.SfxVolume.CurrentAnimation = 25;
                        break;

                    case 3:
                        Anims.SfxVolume.CurrentAnimation = 19;
                        break;

                    case 5:
                        Anims.SfxVolume.CurrentAnimation = 27;
                        break;

                    case 6:
                        Anims.SfxVolume.CurrentAnimation = 23;
                        break;

                    case 8:
                        Anims.SfxVolume.CurrentAnimation = 21;
                        break;
                }
            }
            else
            {
                switch (Localization.LanguageId)
                {
                    case 0:
                    case 1:
                    case 4:
                    case 7:
                    case 9:
                        Anims.SfxVolume.CurrentAnimation = 6;
                        break;

                    case 2:
                        Anims.SfxVolume.CurrentAnimation = 24;
                        break;

                    case 3:
                        Anims.SfxVolume.CurrentAnimation = 18;
                        break;

                    case 5:
                        Anims.SfxVolume.CurrentAnimation = 26;
                        break;

                    case 6:
                        Anims.SfxVolume.CurrentAnimation = 22;
                        break;

                    case 8:
                        Anims.SfxVolume.CurrentAnimation = 20;
                        break;
                }
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            switch (((NGageSoundEventsManager)SoundEventsManager.Current).SoundEffectsVolume)
            {
                case 0:
                    Anims.SfxVolume.CurrentAnimation = 28;
                    break;

                case 0x20:
                    Anims.SfxVolume.CurrentAnimation = 29;
                    break;

                case 0x40:
                    Anims.SfxVolume.CurrentAnimation = 30;
                    break;

                case 0x80:
                    Anims.SfxVolume.CurrentAnimation = 31;
                    break;
            }
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    // GBA
    private void ToggleMusicOnOff()
    {
        if (SoundEventsManager.GetVolumeForType(SoundType.Music) == 0)
            SoundEventsManager.SetVolumeForType(SoundType.Music, SoundEngineInterface.MaxVolume);
        else
            SoundEventsManager.SetVolumeForType(SoundType.Music, 0);
    }

    // GBA
    private void ToggleSfxOnOff()
    {
        if (SoundEventsManager.GetVolumeForType(SoundType.Sfx) == 0)
            SoundEventsManager.SetVolumeForType(SoundType.Sfx, SoundEngineInterface.MaxVolume);
        else
            SoundEventsManager.SetVolumeForType(SoundType.Sfx, 0);
    }

    // GBA
    private bool IsMusicOn()
    {
        return SoundEventsManager.GetVolumeForType(SoundType.Music) == SoundEngineInterface.MaxVolume;
    }

    // GBA
    private bool IsSfxOn()
    {
        return SoundEventsManager.GetVolumeForType(SoundType.Sfx) == SoundEngineInterface.MaxVolume;
    }

    // N-Gage
    private void ModifyMusicVolume(int volDelta)
    {
        float currentVolume = ((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume;
        float newVolume = 0;

        if (0 < volDelta)
        {
            newVolume = (int)currentVolume << volDelta;

            if (newVolume == 0)
                newVolume = 0x20;

            if (newVolume >= SoundEngineInterface.MaxVolume)
                newVolume = SoundEngineInterface.MaxVolume;
        }
        else if (volDelta < 0)
        {
            newVolume = (int)currentVolume >> -volDelta;

            if (newVolume < 0x20)
                newVolume = 0;
        }

        ((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume = newVolume;
    }

    // N-Gage
    private void ModifySfxVolume(int volDelta)
    {
        float currentVolume = ((NGageSoundEventsManager)SoundEventsManager.Current).SoundEffectsVolume;
        float newVolume = 0;

        if (0 < volDelta)
        {
            newVolume = (int)currentVolume << volDelta;

            if (newVolume == 0)
                newVolume = 0x20;

            if (newVolume >= SoundEngineInterface.MaxVolume)
                newVolume = SoundEngineInterface.MaxVolume;
        }
        else if (volDelta < 0)
        {
            newVolume = (int)currentVolume >> -volDelta;

            if (newVolume < 0x20)
                newVolume = 0;
        }

        ((NGageSoundEventsManager)SoundEventsManager.Current).SoundEffectsVolume = newVolume;
    }

    #endregion

    #region Steps

    private void Step_InitializeTransitionToOptions()
    {
        Anims.OptionsSelection.CurrentAnimation = Localization.LanguageUiIndex * OptionsOptionsCount + SelectedOption;
        UpdateMusicVolumeAnimations();
        UpdateSfxVolumeAnimations();

        // Center sprites if English
        if (Localization.LanguageId == 0)
        {
            if (Rom.Platform == Platform.GBA)
            {
                Anims.OptionsSelection.ScreenPos = Anims.OptionsSelection.ScreenPos with { X = 86 };
            }
            else if (Rom.Platform == Platform.NGage)
            {
                Anims.OptionsSelection.ScreenPos = Anims.OptionsSelection.ScreenPos with { X = 58 };
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }

        if (InitialPage == InitialMenuPage.Options)
        {
            CurrentStepAction = Step_Options;
            InitialPage = InitialMenuPage.Language;

            // Center sprites if English
            int x;
            if (Localization.LanguageId == 0)
            {
                x = Rom.Platform switch
                {
                    Platform.GBA => 180,
                    Platform.NGage => 142,
                    _ => throw new UnsupportedPlatformException()
                };
            }
            else
            {
                x = Rom.Platform switch
                {
                    Platform.GBA => 210,
                    Platform.NGage => 152,
                    _ => throw new UnsupportedPlatformException()
                };
            }

            Anims.SoundsBase.ScreenPos = Anims.SoundsBase.ScreenPos with { X = x };
            Anims.MusicVolume.ScreenPos = Anims.MusicVolume.ScreenPos with { X = x };
            Anims.SfxVolume.ScreenPos = Anims.SfxVolume.ScreenPos with { X = x };
        }
        else
        {
            CurrentStepAction = Step_TransitionToOptions;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        }

        ResetStem();
        SetBackgroundPalette(1);
        IsLoadingCredits = false;
    }

    private void Step_TransitionToOptions()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        // Center sprites if English
        int x;
        if (Localization.LanguageId == 0)
        {
            x = Rom.Platform switch
            {
                Platform.GBA => 260,
                Platform.NGage => 222,
                _ => throw new UnsupportedPlatformException()
            };
        }
        else
        {
            x = Rom.Platform switch
            {
                Platform.GBA => 290,
                Platform.NGage => 232,
                _ => throw new UnsupportedPlatformException()
            };
        }

        Anims.SoundsBase.ScreenPos = Anims.SoundsBase.ScreenPos with { X = x - TransitionValue / 2f };
        Anims.MusicVolume.ScreenPos = Anims.MusicVolume.ScreenPos with { X = x - TransitionValue / 2f };
        Anims.SfxVolume.ScreenPos = Anims.SfxVolume.ScreenPos with { X = x - TransitionValue / 2f };

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_Options;
        }

        if (Rom.Platform == Platform.NGage)
        {
            UpdateMusicVolumeAnimations();
            UpdateSfxVolumeAnimations();
        }

        AnimationPlayer.Play(Anims.OptionsSelection);
        AnimationPlayer.Play(Anims.SoundsBase);
        AnimationPlayer.PlayFront(Anims.MusicVolume);
        AnimationPlayer.PlayFront(Anims.SfxVolume);
    }

    private void Step_Options()
    {
        if (IsLoadingCredits)
        {
            if (!TransitionsFX.IsFadingOut)
                FrameManager.SetNextFrame(new Credits(true));
        }
        else
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Up) && Anims.Cursor.CurrentAnimation == 0)
            {
                if (SelectedOption == 0)
                    SelectOption(OptionsOptionsCount - 1, true);
                else
                    SelectOption(SelectedOption - 1, true);

                Anims.OptionsSelection.CurrentAnimation = Localization.LanguageUiIndex * OptionsOptionsCount + SelectedOption;
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Down) && Anims.Cursor.CurrentAnimation == 0)
            {
                if (SelectedOption == OptionsOptionsCount - 1)
                    SelectOption(0, true);
                else
                    SelectOption(SelectedOption + 1, true);

                Anims.OptionsSelection.CurrentAnimation = Localization.LanguageUiIndex * OptionsOptionsCount + SelectedOption;
            }
            else if (Rom.Platform switch
                     {
                         Platform.GBA => JoyPad.IsButtonJustPressed(GbaInput.B),
                         Platform.NGage => NGageJoyPadHelpers.IsBackButtonJustPressed(),
                         _ => throw new UnsupportedPlatformException()
                     } && Anims.Cursor.CurrentAnimation == 0)
            {
                // NOTE: N-Gage auto-saves the option here

                NextStepAction = Step_InitializeTransitionToGameMode;
                CurrentStepAction = Step_TransitionOutOfOptions;
                TransitionOutCursorAndStem();
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
            }
            else if (Rom.Platform switch
                     {
                         Platform.GBA => JoyPad.IsButtonJustPressed(GbaInput.A),
                         Platform.NGage => NGageJoyPadHelpers.IsConfirmButtonJustPressed(),
                         _ => throw new UnsupportedPlatformException()
                     } && Anims.Cursor.CurrentAnimation == 0)
            {
                Anims.Cursor.CurrentAnimation = 16;

                // Music volume
                if (SelectedOption == 0)
                {
                    if (Rom.Platform == Platform.GBA)
                    {
                        ToggleMusicOnOff();
                    }
                    else if (Rom.Platform == Platform.NGage)
                    {
                        if (((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume < SoundEngineInterface.MaxVolume)
                            ModifyMusicVolume(1);
                        else
                            ModifyMusicVolume(-3);
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    UpdateMusicVolumeAnimations();
                }
                // Sfx volume
                else if (SelectedOption == 1)
                {
                    if (Rom.Platform == Platform.GBA)
                    {
                        ToggleSfxOnOff();
                    }
                    else if (Rom.Platform == Platform.NGage)
                    {
                        if (((NGageSoundEventsManager)SoundEventsManager.Current).SoundEffectsVolume < SoundEngineInterface.MaxVolume)
                            ModifySfxVolume(1);
                        else
                            ModifySfxVolume(-3);
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    UpdateSfxVolumeAnimations();
                }

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
            }
            else if (Rom.Platform == Platform.NGage && JoyPad.IsButtonJustPressed(GbaInput.Left) && Anims.Cursor.CurrentAnimation == 0)
            {
                if (SelectedOption == 0)
                {
                    Anims.Cursor.CurrentAnimation = 16;
                    ModifyMusicVolume(-1);
                    UpdateMusicVolumeAnimations();
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                }
                else if (SelectedOption == 1)
                {
                    Anims.Cursor.CurrentAnimation = 16;
                    ModifySfxVolume(-1);
                    UpdateSfxVolumeAnimations();
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                }
            }
            else if (Rom.Platform == Platform.NGage && JoyPad.IsButtonJustPressed(GbaInput.Right) && Anims.Cursor.CurrentAnimation == 0)
            {
                if (SelectedOption == 0)
                {
                    Anims.Cursor.CurrentAnimation = 16;
                    ModifyMusicVolume(1);
                    UpdateMusicVolumeAnimations();
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                }
                else if (SelectedOption == 1)
                {
                    Anims.Cursor.CurrentAnimation = 16;
                    ModifySfxVolume(1);
                    UpdateSfxVolumeAnimations();
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                }
            }

            if (Anims.Cursor.CurrentAnimation == 16 && Anims.Cursor.EndOfAnimation)
            {
                Anims.Cursor.CurrentAnimation = 0;

                if (SelectedOption == 2)
                {
                    TransitionsFX.FadeOutInit(4);
                    IsLoadingCredits = true;
                }
                else if (Rom.Platform == Platform.NGage && SelectedOption == 3)
                {
                    NextStepAction = Step_InitializeTransitionToLanguage;
                    CurrentStepAction = Step_TransitionOutOfOptions;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                    TransitionOutCursorAndStem();
                }
            }
        }

        AnimationPlayer.Play(Anims.OptionsSelection);
        AnimationPlayer.Play(Anims.SoundsBase);
        AnimationPlayer.PlayFront(Anims.MusicVolume);
        AnimationPlayer.PlayFront(Anims.SfxVolume);
    }

    private void Step_TransitionOutOfOptions()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);

            // Center sprites if English
            int x;
            if (Localization.LanguageId == 0)
            {
                x = Rom.Platform switch
                {
                    Platform.GBA => 180,
                    Platform.NGage => 142,
                    _ => throw new UnsupportedPlatformException()
                };
            }
            else
            {
                x = Rom.Platform switch
                {
                    Platform.GBA => 210,
                    Platform.NGage => 152,
                    _ => throw new UnsupportedPlatformException()
                };
            }

            Anims.SoundsBase.ScreenPos = Anims.SoundsBase.ScreenPos with { X = x + TransitionValue / 2f };
            Anims.MusicVolume.ScreenPos = Anims.MusicVolume.ScreenPos with { X = x + TransitionValue / 2f };
            Anims.SfxVolume.ScreenPos = Anims.SfxVolume.ScreenPos with { X = x + TransitionValue / 2f };
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        AnimationPlayer.Play(Anims.OptionsSelection);
        AnimationPlayer.Play(Anims.SoundsBase);
        AnimationPlayer.PlayFront(Anims.MusicVolume);
        AnimationPlayer.PlayFront(Anims.SfxVolume);
    }

    #endregion
}