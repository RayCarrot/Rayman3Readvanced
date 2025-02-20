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
                ActionId = InitialActionId;
                Timer = 0;
                break;
            
            case FsmAction.Step:
                Timer++;

                // Check if damaged
                if (HitPoints != PrevHitPoints)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(Fsm_Damaged);
                    return false;
                }

                // Change idle state after 2 seconds
                if (Timer >= 120)
                {
                    State.MoveTo(Fsm_Blink);
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
                ActionId = InitialActionId + 1;
                break;

            case FsmAction.Step:
                // Check if damaged
                if (HitPoints != PrevHitPoints)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(Fsm_Damaged);
                    return false;
                }

                // Go back to the default idle animation when finished
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Idle);
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
                ActionId = InitialActionId + 2 + HitAction;
                PrevHitPoints = HitPoints;
                break;

            case FsmAction.Step:
                if (IsActionFinished && HitPoints != PrevHitPoints)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(Fsm_Destroyed);
                    return false;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_IdleDamaged);
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
                ActionId = InitialActionId + 3;
                break;

            case FsmAction.Step:
                if (PrevHitPoints != HitPoints)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CageHit_Mix07);
                    ActionId = InitialActionId + 2 + HitAction;
                    PrevHitPoints = HitPoints;
                }

                if (IsActionFinished && ActionId is 2 or 5 or 8 or 11)
                {
                    PrevHitPoints = HitPoints;
                    State.MoveTo(Fsm_Destroyed);
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
                ActionId = InitialActionId + 4;
                Scene.MainActor.ProcessMessage(this, Message.Main_CollectedCage);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Idle);
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