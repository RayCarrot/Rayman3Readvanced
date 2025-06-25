using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Teensies
{
    public bool Fsm_WaitMaster(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Init_Master_Right : Action.Init_Master_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!HasSetTextBox)
                {
                    TextBox = Scene.GetRequiredDialog<TextBoxDialog>();
                    HasSetTextBox = true;
                }

                // This is incorrectly called twice in this state
                if (!Engine.Config.Tweaks.FixBugs)
                    LevelMusicManager.PlaySpecialMusicIfDetected(this);

                SetMasterAction();

                bool requirementMet = IsMapRequirementFulfilled() && IsEnoughCagesTaken();

                if (Scene.IsDetectedMainActor(this) && InitialActionId is Action.Init_World1_Right or Action.Init_World1_Left)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginCutscene);
                    State.MoveTo(Fsm_World1IntroText);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this) && requirementMet)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginCutscene);
                    State.MoveTo(Fsm_ShowRequirementMetText);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this) && !requirementMet)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginCutscene);
                    State.MoveTo(Fsm_ShowRequirementNotMetText);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                TextBox.SetCutsceneCharacter(TextBoxCutsceneCharacter.Teensies);
                TextBox.MoveInOurOut(true);
                ((World)Frame.Current).UserInfo.Hide = true;

                // Don't allow pausing since it uses the same button as skipping
                World frame = (World)Frame.Current;
                SavedBlockPause = frame.BlockPause;
                if (Engine.Config.Tweaks.CanSkipTextBoxes)
                    frame.BlockPause = true;
                break;
        }

        return true;
    }

    public bool Fsm_World1IntroText(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Init_World1_Right : Action.Init_World1_Left;
                TextBox.SetText(0);
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
                {
                    ActionId = Random.GetNumber(3) switch
                    {
                        0 => IsFacingRight ? Action.Init_World1_Right : Action.Init_World1_Left,
                        1 => IsFacingRight ? Action.Init_World2_Right : Action.Init_World2_Left,
                        _ => IsFacingRight ? Action.Init_World3_Right : Action.Init_World3_Left
                    };
                }

                if (JoyPad.IsButtonJustPressed(GbaInput.A))
                    TextBox.MoveToNextText();

                if (Engine.Config.Tweaks.CanSkipTextBoxes && JoyPad.IsButtonJustPressed(GbaInput.Start))
                {
                    World frame = (World)Frame.Current;
                    frame.BlockPause = true;

                    TextBox.Skip();
                }

                if (TextBox.IsFinished && IsMapRequirementFulfilled() && IsEnoughCagesTaken())
                {
                    State.MoveTo(Fsm_ShowRequirementMetText);
                    return false;
                }
                
                if (TextBox.IsFinished)
                {
                    State.MoveTo(Fsm_ShowRequirementNotMetText);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ShowRequirementMetText(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.ShowRequirementMet_Right : Action.ShowRequirementMet_Left;
                IsSolid = false;
                SetRequirementMetText();
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                SetMasterAction();

                if (!IsMovingOutTextBox)
                {
                    if (TextBox.IsFinished)
                    {
                        TextBox.MoveInOurOut(false);
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
                        IsMovingOutTextBox = true;

                        // Restore being able to pause
                        if (Engine.Config.Tweaks.CanSkipTextBoxes)
                        {
                            World frame = (World)Frame.Current;
                            frame.BlockPause = SavedBlockPause;
                        }
                    }
                    else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                    {
                        TextBox.MoveToNextText();
                    }

                    if (Engine.Config.Tweaks.CanSkipTextBoxes && JoyPad.IsButtonJustPressed(GbaInput.Start))
                    {
                        World frame = (World)Frame.Current;
                        frame.BlockPause = true;

                        TextBox.Skip();
                    }
                }

                if (!TextBox.IsOnScreen())
                {
                    State.MoveTo(Fsm_ExitedRequirementMetText);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ((World)Frame.Current).UserInfo.Hide = false;
                break;
        }

        return true;
    }

    public bool Fsm_ExitedRequirementMetText(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);
                SetMasterAction();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ShowRequirementNotMetText(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.ShowRequirementNotMet_Left : Action.ShowRequirementNotMet_Right;
                SetRequirementNotMetText();
                break;

            case FsmAction.Step:
                bool finished = false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                SetMasterAction();

                if (TextBox.IsFinished)
                {
                    TextBox.MoveInOurOut(false);
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
                    finished = true;

                    // Restore being able to pause
                    if (Engine.Config.Tweaks.CanSkipTextBoxes)
                    {
                        World frame = (World)Frame.Current;
                        frame.BlockPause = SavedBlockPause;
                    }
                }
                else if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    TextBox.MoveToNextText();
                }

                if (Engine.Config.Tweaks.CanSkipTextBoxes && JoyPad.IsButtonJustPressed(GbaInput.Start))
                {
                    World frame = (World)Frame.Current;
                    frame.BlockPause = true;

                    TextBox.Skip();
                }

                if (finished)
                {
                    State.MoveTo(Fsm_WaitExitRequirementNotMetText);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_WaitExitRequirementNotMetText(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!TextBox.IsOnScreen())
                {
                    State.MoveTo(Fsm_ExitedRequirementNotMetText);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ((World)Frame.Current).UserInfo.Hide = false;
                break;
        }

        return true;
    }

    public bool Fsm_ExitedRequirementNotMetText(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                SetMasterAction();

                if (HasLeftMainActorView())
                {
                    State.MoveTo(Fsm_WaitMaster);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_VictoryDance(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                IsSolid = false;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);
                if (IsActionFinished)
                {
                    ActionId = Random.GetNumber(2) switch
                    {
                        0 => IsFacingRight ? Action.Victory1_Right : Action.Victory1_Left,
                        _ => IsFacingRight ? Action.Victory2_Right : Action.Victory2_Left
                    };
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                IsSolid = false;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}