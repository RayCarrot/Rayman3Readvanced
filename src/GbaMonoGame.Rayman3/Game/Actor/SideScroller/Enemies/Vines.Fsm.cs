using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Vines
{
    private bool FsmStep_CheckHitMainActor()
    {
        if (Scene.IsHitMainActor(this))
        {
            Scene.MainActor.ProcessMessage(this, Message.Damaged);
            Scene.MainActor.ReceiveDamage(AttackPoints);
        }

        return true;
    }

    private bool FsmStep_CheckDeath()
    {
        if (HitPoints == 0)
        {
            State.MoveTo(Fsm_Retract);
            return false;
        }

        return true;
    }

    public bool Fsm_Init(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                IsFacingDown = ActionId == Action.Idle_Down;
                break;

            case FsmAction.Step:
                State.MoveTo(Fsm_Wait);
                return false;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingDown ? Action.Idle_Down : Action.Idle_Up;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Extend);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Extend(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__RootOut_Mix04);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__RootIn_Mix01);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RootOut_Mix04);

                ActionId = IsFacingDown ? Action.Extend_Down : Action.Extend_Up;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (!FsmStep_CheckDeath())
                    return false;

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Attack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingDown ? Action.Attack_Down : Action.Attack_Up;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (!FsmStep_CheckDeath())
                    return false;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Retract(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__RootOut_Mix04);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__RootIn_Mix01);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RootIn_Mix01);

                ActionId = IsFacingDown ? Action.Retract_Down : Action.Retract_Up;
                
                // Restore hp
                HitPoints++;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Respawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = IsFacingDown ? Action.Idle_Down : Action.Idle_Up;
                break;

            case FsmAction.Step:
                Timer++;

                if (Timer == 80)
                {
                    State.MoveTo(Fsm_Wait);
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