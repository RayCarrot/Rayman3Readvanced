using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public partial class PauseDialog
{
    public bool Fsm_CheckSelection(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                int maxOption = Rom.Platform switch
                {
                    Platform.GBA => 2,
                    Platform.NGage => 3,
                    _ => throw new UnsupportedPlatformException()
                };

                bool resume = false;
                bool sleepMode = false;
                bool quitGame = false;

                if (JoyPad.IsButtonJustPressed(GbaInput.Up))
                {
                    PrevSelectedOption = SelectedOption;
                    if (SelectedOption == 0)
                        SelectedOption = maxOption;
                    else
                        SelectedOption--;

                    if (Rom.Platform == Platform.GBA)
                        PauseSelection.CurrentAnimation = SelectedOption switch
                        {
                            0 => 0 + Localization.LanguageUiIndex,
                            1 => 10 + Localization.LanguageUiIndex,
                            2 => 20 + Localization.LanguageUiIndex,
                            _ => throw new IndexOutOfRangeException(),
                        };
                    else if (Rom.Platform == Platform.NGage)
                        PauseSelection.CurrentAnimation = SelectedOption switch
                        {
                            0 => 0 + Localization.LanguageUiIndex,
                            1 => 5 + Localization.LanguageUiIndex,
                            2 => 25 + Localization.LanguageUiIndex,
                            3 => 10 + Localization.LanguageUiIndex,
                            _ => throw new IndexOutOfRangeException(),
                        };
                    else
                        throw new UnsupportedPlatformException();

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
                {
                    PrevSelectedOption = SelectedOption;
                    if (SelectedOption == maxOption)
                        SelectedOption = 0;
                    else
                        SelectedOption++;

                    if (Rom.Platform == Platform.GBA)
                        PauseSelection.CurrentAnimation = SelectedOption switch
                        {
                            0 => 0 + Localization.LanguageUiIndex,
                            1 => 10 + Localization.LanguageUiIndex,
                            2 => 20 + Localization.LanguageUiIndex,
                            _ => throw new IndexOutOfRangeException(),
                        };
                    else if (Rom.Platform == Platform.NGage)
                        PauseSelection.CurrentAnimation = SelectedOption switch
                        {
                            0 => 0 + Localization.LanguageUiIndex,
                            1 => 5 + Localization.LanguageUiIndex,
                            2 => 25 + Localization.LanguageUiIndex,
                            3 => 10 + Localization.LanguageUiIndex,
                            _ => throw new IndexOutOfRangeException(),
                        };
                    else
                        throw new UnsupportedPlatformException();

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (Rom.Platform == Platform.NGage && JoyPad.IsButtonJustPressed(GbaInput.Left))
                {
                    if (SelectedOption == 1)
                    {
                        ModifyMusicVolume(-1);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                    else if (SelectedOption == 2)
                    {
                        ModifySfxVolume(-1);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                }
                else if (Rom.Platform == Platform.NGage && JoyPad.IsButtonJustPressed(GbaInput.Right))
                {
                    if (SelectedOption == 1)
                    {
                        ModifyMusicVolume(1);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                    else if (SelectedOption == 2)
                    {
                        ModifySfxVolume(1);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    }
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.B) || JoyPad.IsButtonJustPressed(GbaInput.Start))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    DrawStep = PauseDialogDrawStep.MoveOut;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

                    if (Rom.Platform == Platform.NGage)
                        ((NGageSoundEventsManager)SoundEventsManager.Current).ResumeLoopingSoundEffects();

                    resume = true;
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);

                    switch (SelectedOption)
                    {
                        // Continue
                        case 0:
                            DrawStep = PauseDialogDrawStep.MoveOut;
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

                            if (Rom.Platform == Platform.NGage)
                                ((NGageSoundEventsManager)SoundEventsManager.Current).ResumeLoopingSoundEffects();

                            resume = true;
                            break;

                        // Sleep mode
                        case 1 when Rom.Platform == Platform.GBA:
                            sleepMode = true;
                            break;

                        // Music volume
                        case 1 when Rom.Platform == Platform.NGage:
                            if (((NGageSoundEventsManager)SoundEventsManager.Current).MusicVolume < SoundEngineInterface.MaxVolume)
                                ModifyMusicVolume(1);
                            else
                                ModifyMusicVolume(-3);
                            break;

                        // Sfx volume
                        case 2 when Rom.Platform == Platform.NGage:
                            if (((NGageSoundEventsManager)SoundEventsManager.Current).SfxVolume < SoundEngineInterface.MaxVolume)
                                ModifySfxVolume(1);
                            else
                                ModifySfxVolume(-3);
                            break;

                        // Quit game
                        case 2 when Rom.Platform == Platform.GBA:
                        case 3 when Rom.Platform == Platform.NGage:
                            // In the original game it sets the x positions to 200 so that they're off-screen. But that
                            // won't work due to custom resolutions being possible, so we instead hide all sprite channels.
                            if (Rom.Platform == Platform.NGage)
                            {
                                MusicVolume.VisibleSpriteChannels = 0;
                                SfxVolume.VisibleSpriteChannels = 0;
                            }
                            
                            quitGame = true;
                            break;
                    }
                }

                if (Rom.Platform == Platform.NGage)
                {
                    SetMusicVolumeAnimation();
                    SetSfxVolumeAnimation();
                }

                if (resume)
                {
                    State.MoveTo(null);
                    return false;
                }

                if (Rom.Platform == Platform.GBA && sleepMode)
                {
                    State.MoveTo(Fsm_SleepMode);
                    return false;
                }

                if (quitGame)
                {
                    State.MoveTo(Fsm_QuitGame);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SleepMode(FsmAction action)
    {
        // This is implemented differently in the original game. It has loops where it calls
        // vsync here and does bios calls to trigger the sleep mode. We just try and simulate
        // the effect of sleep mode, with it instantly ending.
        switch (action)
        {
            case FsmAction.Init:
                IsInSleepMode = true;
                SleepModeTimer = 0;
                
                SavedFadeControl = Gfx.FadeControl;
                SavedFade = Gfx.Fade;

                Gfx.FadeControl = new FadeControl(FadeMode.BrightnessIncrease, FadeFlags.Screen0);
                Gfx.Fade = 1;
                break;

            case FsmAction.Step:
                SleepModeTimer++;

                // Text displays for 3 seconds
                if (SleepModeTimer > 180)
                {
                    State.MoveTo(Fsm_CheckSelection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                IsInSleepMode = false;

                Gfx.FadeControl = SavedFadeControl;
                Gfx.Fade = SavedFade;

                DrawStep = PauseDialogDrawStep.Hide;
                break;
        }

        return true;
    }

    public bool Fsm_QuitGame(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PrevSelectedOption = SelectedOption;

                if (Rom.Platform == Platform.GBA)
                {
                    SelectedOption = 1;
                    PauseSelection.CurrentAnimation = 40 + Localization.LanguageUiIndex;
                }
                else if (Rom.Platform == Platform.NGage)
                {
                    SelectedOption = 0;
                    PauseSelection.CurrentAnimation = 15 + Localization.LanguageUiIndex;
                }
                else
                {
                    throw new UnsupportedPlatformException();
                }
                break;

            case FsmAction.Step:
                bool goBack = false;

                int resumeOptionIndex = Rom.Platform switch
                {
                    Platform.GBA => 1,
                    Platform.NGage => 0,
                    _ => throw new UnsupportedPlatformException(),
                };
                int quitOptionIndex = Rom.Platform switch
                {
                    Platform.GBA => 0,
                    Platform.NGage => 1,
                    _ => throw new UnsupportedPlatformException(),
                };

                if (JoyPad.IsButtonJustPressed(GbaInput.Up) || JoyPad.IsButtonJustPressed(GbaInput.Down))
                {
                    if (Rom.Platform == Platform.GBA)
                    {
                        if (SelectedOption == 0)
                            PauseSelection.CurrentAnimation = 40 + Localization.LanguageUiIndex;
                        else
                            PauseSelection.CurrentAnimation = 30 + Localization.LanguageUiIndex;
                    }
                    else if (Rom.Platform == Platform.NGage)
                    {
                        if (SelectedOption == 0)
                            PauseSelection.CurrentAnimation = 20 + Localization.LanguageUiIndex;
                        else
                            PauseSelection.CurrentAnimation = 15 + Localization.LanguageUiIndex;
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    PrevSelectedOption = SelectedOption;
                    SelectedOption = SelectedOption == 0 ? 1 : 0;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.B) || (JoyPad.IsButtonJustPressed(GbaInput.A) && SelectedOption == resumeOptionIndex))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);

                    if (Rom.Platform == Platform.GBA)
                    {
                        PauseSelection.CurrentAnimation = 20 + Localization.LanguageUiIndex;
                        PrevSelectedOption = SelectedOption;
                        SelectedOption = 2;
                    }
                    else if (Rom.Platform == Platform.NGage)
                    {
                        // Unhide
                        MusicVolume.VisibleSpriteChannels = UInt32.MaxValue;
                        SfxVolume.VisibleSpriteChannels = UInt32.MaxValue;

                        PauseSelection.CurrentAnimation = 10 + Localization.LanguageUiIndex;
                        PrevSelectedOption = SelectedOption;
                        SelectedOption = 3;
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    goBack = true;
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.A) && SelectedOption == quitOptionIndex)
                {
                    GameTime.Resume();

                    if (Rom.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                    {
                        SoundEventsManager.StopAllSongs();
                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = 1;
                        FrameManager.SetNextFrame(new GameCubeMenu());
                    }
                    else
                    {
                        SoundEventsManager.StopAllSongs();
                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = 1;
                        FrameManager.SetNextFrame(new MenuAll(MenuAll.Page.SelectGameMode));
                    }

                    if (Rom.Platform == Platform.GBA)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                }

                if (goBack)
                {
                    State.MoveTo(Fsm_CheckSelection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}