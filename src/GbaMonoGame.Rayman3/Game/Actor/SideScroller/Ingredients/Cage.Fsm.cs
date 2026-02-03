using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Cage
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsGrounded ? Action.GroundedIdle : Action.HangingIdle;
                Timer = 0;
                break;
            
            case FsmAction.Step:
                Timer++;

                // Check if damaged
                if (HitPoints != PrevHitPoints)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(_Fsm_Damaged);
                    return false;
                }

                // Change idle state after 2 seconds
                if (Timer >= 120)
                {
                    State.MoveTo(_Fsm_Blink);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Blink(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // If all objects are kept active we only want to make this sound when framed
                if (!Scene.KeepAllObjectsActive || AnimatedObject.IsFramed)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CageSnd1_Mix02__or__CageSnd2_Mix02);
                ActionId = IsGrounded ? Action.GroundedBlink : Action.HangingBlink;
                break;

            case FsmAction.Step:
                // Check if damaged
                if (HitPoints != PrevHitPoints)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(_Fsm_Damaged);
                    return false;
                }

                // Go back to the default idle animation when finished
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Idle);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Damaged(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CageHit_Mix07);

                if (IsGrounded)
                    ActionId = IsHitToLeft ? Action.GroundedHitLeft : Action.GroundedHitRight;
                else
                    ActionId = IsHitToLeft ? Action.HangingHitLeft : Action.HangingHitRight;
                
                PrevHitPoints = HitPoints;
                break;

            case FsmAction.Step:
                if (IsActionFinished && HitPoints != PrevHitPoints)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(_Fsm_Destroyed);
                    return false;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_IdleDamaged);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_IdleDamaged(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsGrounded ? Action.GroundedBlinkDamaged : Action.HangingBlinkDamaged;
                break;

            case FsmAction.Step:
                if (PrevHitPoints != HitPoints)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CageHit_Mix07);

                    if (IsGrounded)
                        ActionId = IsHitToLeft ? Action.GroundedHitLeft : Action.GroundedHitRight;
                    else
                        ActionId = IsHitToLeft ? Action.HangingHitLeft : Action.HangingHitRight;

                    PrevHitPoints = HitPoints;
                }

                if (IsActionFinished && ActionId is Action.GroundedHitRight or Action.GroundedHitLeft or Action.HangingHitRight or Action.HangingHitLeft)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(_Fsm_Destroyed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Destroyed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                GameInfo.KillCage(CageId);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CageTrsh_Mix05);
                ActionId = IsGrounded ? Action.GroundedBreak : Action.HangingBreak;
                Scene.MainActor.ProcessMessage(this, Message.Rayman_CollectCage);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}