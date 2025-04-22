using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Slapdash
{
    private bool FsmStep_CheckDeath()
    {
        if (IsInvulnerable && GameTime.ElapsedFrames - InvulnerabilityTimer > 120)
            IsInvulnerable = false;

        if (Scene.IsHitMainActor(this) && !Scene.MainActor.IsInvulnerable)
        {
            Scene.MainActor.ReceiveDamage(AttackPoints);

            Scene.MainActor.Position -= new Vector2(0, 30);

            if (IsFacingRight)
                Scene.MainActor.Position = Scene.MainActor.Position with { X = Position.X + 10 };
            else
                Scene.MainActor.Position = Scene.MainActor.Position with { X = Position.X - 10 };

            Scene.MainActor.ProcessMessage(this, Message.Actor_HurtPassthrough, this);
        }

        if (HitPoints == 0)
        {
            IsInvulnerable = false;
            State.MoveTo(Fsm_Dying);
            return false;
        }

        return true;
    }

    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);
                
                if ((IsFacingRight && Scene.MainActor.Position.X - Position.X < Scene.Resolution.X) || 
                    (IsFacingLeft && Position.X - Scene.MainActor.Position.X < Scene.Resolution.X)) 
                {
                    State.MoveTo(Fsm_Walk);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Walk(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (ActionId is Action.Hit_Right or Action.Hit_Left)
                    ActionId = IsFacingRight ? Action.Walk_Left : Action.Walk_Right;
                else
                    ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (ShouldTurnAround())
                {
                    State.MoveTo(Fsm_TurnAround);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_BeginChargeAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_TurnAround(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__CagoAttk_Mix03);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagoTurn_Mix03);

                // The game does +2 or +4 to the action here, but it's cleaner to just handle each valid case
                if (ActionId is 
                    Action.BeginChargeAttack_Right or Action.BeginChargeAttack_Left or 
                    Action.ChargeAttack_Right or Action.ChargeAttack_Left)
                {
                    ActionId = IsFacingRight ? Action.TurnAroundFromChargeAttack_Right : Action.TurnAroundFromChargeAttack_Left;
                }
                else if (ActionId is Action.Walk_Right or Action.Walk_Left)
                {
                    ActionId = IsFacingRight ? Action.TurnAround_Right : Action.TurnAround_Left;
                }
                else
                {
                    throw new Exception("Invalid action to turn around from");
                }

                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Walk);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ActionId = IsFacingRight ? Action.Walk_Left : Action.Walk_Right;
                ChangeAction();
                break;
        }

        return true;
    }

    public bool Fsm_BeginChargeAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.BeginChargeAttack_Right : Action.BeginChargeAttack_Left;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagoAttk_Mix03);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_ChargeAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ChargeAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.ChargeAttack_Right : Action.ChargeAttack_Left;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (Timer < 32)
                    Timer++;

                bool changedDirection = false;
                bool timeOut = false;

                if (!Scene.IsDetectedMainActor(this))
                {
                    if ((IsFacingRight && Scene.MainActor.Position.X < Position.X) ||
                        (IsFacingLeft && Position.X < Scene.MainActor.Position.X))
                    {
                        changedDirection = true;
                    }
                    else if (Timer >= 32)
                    {
                        timeOut = true;
                    }
                }

                if (ShouldTurnAround())
                {
                    State.MoveTo(Fsm_TurnAround);
                    return false;
                }

                if (changedDirection)
                {
                    State.MoveTo(Fsm_TurnAroundFromChargeAttack);
                    return false;
                }

                if (timeOut)
                {
                    State.MoveTo(Fsm_Walk);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_TurnAroundFromChargeAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.BeginChargeAttack_Right : Action.BeginChargeAttack_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_TurnAround);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Hit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Hit_Right : Action.Hit_Left;
                PrevHitPoints = HitPoints;
                StartInvulnerability();
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagouHit_Mix03);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Walk);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagoDie2_Mix01);
                IsSolid = false;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Wait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                LevelMusicManager.StopSpecialMusic();
                break;
        }

        return true;
    }
}