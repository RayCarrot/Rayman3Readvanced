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
                AddOption("OPTIONS", Fsm_Options);
                // TODO: TimeAttack ? RESTART LEVEL : VIEW ACHIEVEMENTS
                AddOption(CanExitLevel ? "EXIT LEVEL" : "QUIT GAME", Fsm_QuitGame);

                SetSelectedOption(SavedSelectedOption);
                break;

            case FsmAction.Step:
                bool hasSelectedOption = false;

                if (JoyPad.IsButtonJustPressed(GbaInput.Up))
                {
                    SetSelectedOption(SelectedOption - 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
                {
                    SetSelectedOption(SelectedOption + 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.B) || JoyPad.IsButtonJustPressed(GbaInput.Start))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    SetSelectedOption(0);
                    InvokeOption();
                    hasSelectedOption = true;
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    InvokeOption();
                    hasSelectedOption = true;
                }

                if (hasSelectedOption)
                {
                    SetOptionState();
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
                OptionsMenu.MoveIn();
                break;

            case FsmAction.Step:
                OptionsMenu.Step();
                
                if (OptionsMenu.DrawStep == PauseDialogDrawStep.Hide)
                {
                    State.MoveTo(Fsm_CheckSelection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Engine.SaveConfig();
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
                        Gfx.Fade = 1;

                        if (TimeAttackInfo.IsActive)
                            FrameManager.SetNextFrame(new ModernMenuAll(InitialMenuPage.TimeAttack));
                        else if (Rom.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                            FrameManager.SetNextFrame(new GameCubeMenu());
                        else
                            FrameManager.SetNextFrame(new ModernMenuAll(InitialMenuPage.GameMode));
                    }

                    if (Rom.Platform == Platform.GBA)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                });
                AddOption("NO", Fsm_CheckSelection, () =>
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
                            FrameManager.SetNextFrame(new GameCubeMenu());
                        }
                        else
                        {
                            GameInfo.LoadLevel(MapId.World1 + (int)GameInfo.WorldId);
                        }

                        GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.MapId;
                        GameInfo.Save(GameInfo.CurrentSlot);
                    }
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.Up))
                {
                    SetSelectedOption(SelectedOption - 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
                {
                    SetSelectedOption(SelectedOption + 1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.B))
                {
                    SetSelectedOption(1);
                    InvokeOption();
                    hasSelectedOption = true;
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    InvokeOption();
                    hasSelectedOption = true;
                }

                if (hasSelectedOption && !IsTransitioningOut)
                {
                    SetOptionState();
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