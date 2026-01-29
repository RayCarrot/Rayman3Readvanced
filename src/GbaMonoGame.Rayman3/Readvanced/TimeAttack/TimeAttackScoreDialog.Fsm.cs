using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class TimeAttackScoreDialog
{
    public bool Fsm_ShowTargets(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                // Wait 50 frames for each target to move in
                if (Timer == 50)
                {
                    // Start playing music
                    Rayman3SoundEvent musicEvent = NewRecord ? Rayman3SoundEvent.Play__barrel_BA : Rayman3SoundEvent.Play__barrel;
                    if (!SoundEventsManager.IsSongPlaying(musicEvent))
                        SoundEventsManager.ProcessEvent(musicEvent);

                    // Check if the target time was beaten
                    if (TimeAttackInfo.Timer <= TimeTargets[TimeTargetTransitionIndex].Time.Time)
                    {
                        TimeTargets[TimeTargetTransitionIndex].TransitionIn();
                        TimeTargetTransitionIndex++;
                    }
                    // Otherwise we force end the transitions
                    else
                    {
                        TimeTargetTransitionIndex = TimeTargets.Length;
                    }

                    Timer = 0;
                }

                if (TimeTargetTransitionIndex == TimeTargets.Length)
                {
                    State.MoveTo(Fsm_ShowCurrentTime);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ShowCurrentTime(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                CurrentTimeTextSpeed = 0.2f;
                break;

            case FsmAction.Step:
                CurrentTimeText.ScreenPos += new Vector2(0, CurrentTimeTextSpeed);
                CurrentTimeTextSpeed += 0.03f;

                if (CurrentTimeText.ScreenPos.Y >= 30)
                {
                    State.MoveTo(Fsm_ShowRecordTime);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ShowRecordTime(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                // Wait 30 frames
                if (Timer == 30)
                {
                    DrawNewRecord = true;

                    if (NewRecord)
                    {
                        // Save the record time
                        TimeAttackInfo.SaveTime();

                        // Play sound
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
                    }

                    State.MoveTo(Fsm_ShowOptions);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ShowOptions(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                SetSelectedOption(NewRecord ? 1 : 0, animate: false);
                break;

            case FsmAction.Step:
                if (Timer < 10)
                {
                    Timer++;

                    if (Timer == 10)
                        DrawOptions = true;
                }

                // Wait 10 frames
                if (Timer == 10)
                {
                    if (IsTransitioningOut)
                    {
                        if (StepCircleTransition())
                        {
                            SoundEventsManager.StopAllSongs();
                            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                            Gfx.Fade = AlphaCoefficient.Max;

                            TimeAttackInfo.UnInit();
                            TimeAttackInfo.Init();
                            TimeAttackInfo.LoadLevel(MapId, TimeAttackInfo.GhostType);
                        }
                    }
                    else
                    {
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
                        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                        {
                            // Restart
                            if (SelectedOption == 0)
                            {
                                BeginCircleTransition();
                            }
                            // Continue
                            else if (SelectedOption == 1)
                            {
                                SoundEventsManager.StopAllSongs();
                                Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                                Gfx.Fade = AlphaCoefficient.Max;

                                if (Engine.ActiveConfig.Tweaks.UseModernMainMenu)
                                    FrameManager.SetNextFrame(new ModernMenuAll(InitialMenuPage.TimeAttack));
                                else
                                    FrameManager.SetNextFrame(new MenuAll(InitialMenuPage.GameMode));
                            }

                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                        }
                    }

                    ManageCursor();
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}