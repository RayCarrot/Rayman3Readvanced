using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class ModernPauseDialog
{
    public bool Fsm_CheckSelection(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ClearOptions();

                AddOption("CONTINUE", null, () =>
                {
                    DrawStep = PauseDialogDrawStep.MoveOut;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

                    if (Rom.Platform == Platform.NGage)
                        ((NGageSoundEventsManager)SoundEventsManager.Current).ResumeLoopingSoundEffects();
                });
                AddOption("OPTIONS", _Fsm_Options);

                // World
                if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
                {
                    AddOption("LEVELS", _Fsm_Levels);
                }
                // Worldmap
                else if (GameInfo.MapId == MapId.WorldMap)
                {
                    // No extra option
                }
                // Time attack
                else if (Rayman3.TimeAttack.IsActive)
                {
                    AddOption("RESTART MAP", _Fsm_RestartMap); // TODO: Remove option?
                    // TODO: "RESTART LEVEL"
                }
                // Level
                else
                {
                    AddOption("RESTART MAP", _Fsm_RestartMap);
                }

                AddOption(CanExitLevel ? "EXIT LEVEL" : "QUIT GAME", _Fsm_QuitGame);

                SetSelectedOption(SavedSelectedOption);
                break;

            case FsmAction.Step:
                bool hasSelectedOption = false;

                if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
                {
                    SetSelectedOption(SelectedOption - 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
                {
                    SetSelectedOption(SelectedOption + 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.PauseMenuBack))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    SetSelectedOption(0);
                    InvokeSelectedOption();
                    hasSelectedOption = true;
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    InvokeSelectedOption();
                    hasSelectedOption = true;
                }

                if (hasSelectedOption)
                {
                    MoveToSelectedOptionState();
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SavedSelectedOption = SelectedOption;
                break;
        }

        return true;
    }

    public bool Fsm_Options(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                MoveInMenu();
                OptionsMenu.MoveIn();
                break;

            case FsmAction.Step:
                PauseDialogDrawStep prevDrawStep = OptionsMenu.DrawStep;
                OptionsMenu.Step();

                if (prevDrawStep != OptionsMenu.DrawStep && OptionsMenu.DrawStep == PauseDialogDrawStep.MoveOut)
                    MoveOutMenu();

                if (OptionsMenu.DrawStep == PauseDialogDrawStep.Hide)
                {
                    State.MoveTo(_Fsm_CheckSelection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Engine.Config.Save();
                break;
        }

        return true;
    }

    public bool Fsm_Levels(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                MoveInMenu();
                LevelsMenu.MoveIn();
                break;

            case FsmAction.Step:
                PauseDialogDrawStep prevDrawStep = LevelsMenu.DrawStep;
                LevelsMenu.Step();

                if (prevDrawStep != LevelsMenu.DrawStep && LevelsMenu.DrawStep == PauseDialogDrawStep.MoveOut)
                    MoveOutMenu();

                if (LevelsMenu.DrawStep == PauseDialogDrawStep.Hide)
                {
                    State.MoveTo(_Fsm_CheckSelection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_RestartMap(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ClearOptions();

                AddOption("YES", null, () =>
                {
                    BeginCircleTransition();

                    if (Rom.Platform == Platform.GBA)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                });
                AddOption("NO", _Fsm_CheckSelection, () =>
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                });

                SetSelectedOption(1);
                break;

            case FsmAction.Step:
                bool hasSelectedOption = false;

                if (IsTransitioningOut)
                {
                    if (StepCircleTransition())
                    {
                        // Resume
                        GameTime.Resume();
                        SoundEventsManager.StopAllSongs();

                        // Reset checkpoint
                        GameInfo.LastGreenLumAlive = 0;

                        // Reload map
                        Engine.FrameMngr.ReloadCurrentFrame();
                    }
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
                {
                    SetSelectedOption(SelectedOption - 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
                {
                    SetSelectedOption(SelectedOption + 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
                {
                    SetSelectedOption(1);
                    InvokeSelectedOption();
                    hasSelectedOption = true;
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                {
                    InvokeSelectedOption();
                    hasSelectedOption = true;
                }

                if (hasSelectedOption && !IsTransitioningOut)
                {
                    MoveToSelectedOptionState();
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_QuitGame(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ClearOptions();

                AddOption("YES", null, () =>
                {
                    // Exit level
                    if (CanExitLevel)
                    {
                        BeginCircleTransition();
                    }
                    // Quit game
                    else
                    {
                        GameTime.Resume();

                        SoundEventsManager.StopAllSongs();
                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = AlphaCoefficient.Max;

                        if (Rayman3.TimeAttack.IsActive)
                        {
                            if (Engine.Config.Active.Tweaks.UseModernMainMenu)
                                Engine.FrameMngr.SetNextFrame(new ModernMenuAll(InitialMenuPage.TimeAttack));
                            else
                                Engine.FrameMngr.SetNextFrame(new MenuAll(InitialMenuPage.GameMode));
                        }
                        else if (Rom.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                        {
                            Engine.FrameMngr.SetNextFrame(new GameCubeMenu());
                        }
                        else
                        {
                            if (Engine.Config.Active.Tweaks.UseModernMainMenu)
                                Engine.FrameMngr.SetNextFrame(new ModernMenuAll(InitialMenuPage.GameMode));
                            else
                                Engine.FrameMngr.SetNextFrame(new MenuAll(InitialMenuPage.GameMode));
                        }
                    }

                    if (Rom.Platform == Platform.GBA)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                });
                AddOption("NO", _Fsm_CheckSelection, () =>
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                });

                SetSelectedOption(1);
                break;

            case FsmAction.Step:
                bool hasSelectedOption = false;

                if (IsTransitioningOut)
                {
                    if (StepCircleTransition())
                    {
                        GameTime.Resume();
                        SoundEventsManager.StopAllSongs();

                        if (Rom.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                        {
                            ((FrameSideScrollerGCN)Frame.Current).RestoreMapAndPowers();
                            Engine.FrameMngr.SetNextFrame(new GameCubeMenu());
                        }
                        else
                        {
                            GameInfo.LoadLevel(MapId.World1 + (int)GameInfo.WorldId);
                        }

                        GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.MapId;
                        GameInfo.Save(GameInfo.CurrentSlot);
                    }
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
                {
                    SetSelectedOption(SelectedOption - 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
                {
                    SetSelectedOption(SelectedOption + 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
                {
                    SetSelectedOption(1);
                    InvokeSelectedOption();
                    hasSelectedOption = true;
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                {
                    InvokeSelectedOption();
                    hasSelectedOption = true;
                }

                if (hasSelectedOption && !IsTransitioningOut)
                {
                    MoveToSelectedOptionState();
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