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
                    ReadvancedSoundEvent musicEvent = NewRecord 
                        ? ReadvancedSoundEvent.Play__timeattack_score2 
                        : ReadvancedSoundEvent.Play__timeattack_score;
                    if (!Engine.Sem.IsSongPlaying(musicEvent))
                        Engine.Sem.ProcessEvent(musicEvent);

                    // Check if the target time was beaten
                    if (Rayman3.TimeAttack.Timer <= TimeTargets[TimeTargetTransitionIndex].Time.Time)
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
                    State.MoveTo(_Fsm_ShowCurrentTime);
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
                    State.MoveTo(_Fsm_ShowRecordTime);
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
                        Rayman3.TimeAttack.SaveTime();

                        // Play sound
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
                    }

                    State.MoveTo(_Fsm_ShowOptions);
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
                            Engine.Sem.StopAllSongs();
                            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                            Gfx.Fade = AlphaCoefficient.Max;

                            Rayman3.TimeAttack.End();
                            Rayman3.TimeAttack.Start();
                            Rayman3.TimeAttack.LoadLevel(MapId, Rayman3.TimeAttack.GhostType);
                        }
                    }
                    else
                    {
                        if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
                        {
                            SetSelectedOption(SelectedOption - 1);
                            Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                        }
                        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
                        {
                            SetSelectedOption(SelectedOption + 1);
                            Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
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
                                Engine.Sem.StopAllSongs();
                                Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                                Gfx.Fade = AlphaCoefficient.Max;

                                if (Engine.Settings.Active.Tweaks.UseModernMainMenu)
                                    Engine.FrameMngr.SetNextFrame(new ModernMenuAll(InitialMenuPage.TimeAttack));
                                else
                                    Engine.FrameMngr.SetNextFrame(new MenuAll(InitialMenuPage.GameMode));
                            }

                            Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
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