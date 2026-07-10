using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Hoodstormer
{
    private bool FsmStep_CheckDeath()
    {
        if (Scene.IsHitMainActor(this))
            Scene.MainActor.ReceiveDamage(AttackPoints);

        if (HitPoints == 0 && 
            State == _Fsm_Attack && 
            AnimatedObject.CurrentFrame is > 0 and < 4 or > 5 and < 9) 
        {
            ShootMissile();
        }

        if (HitPoints == 0)
        {
            State.MoveTo(_Fsm_Dying);
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

                // Check if in range of the main actor
                if ((IsFacingRight && Scene.MainActor.Position.X - Position.X < FLY_IN_DETECTION_DISTANCE_X) ||
                    (IsFacingLeft && Position.X - Scene.MainActor.Position.X < FLY_IN_DETECTION_DISTANCE_X))
                {
                    State.MoveTo(_Fsm_Fly);
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
    public bool Fsm_Taunt(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Taunt_Right : Action.Taunt_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Wait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Fly(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Fly_Right : Action.Fly_Left;
                MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? FLY_IN_SPEED_X : -FLY_IN_SPEED_X };
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__CagouRit_Mix03, this);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(_Fsm_Attack);
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
                ActionId = IsFacingRight ? Action.Shoot_Right : Action.Shoot_Left;
                MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? ATTACK_SPEED_X : -ATTACK_SPEED_X };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (AnimatedObject.CurrentFrame is 3 or 8 && !AnimatedObject.IsDelayMode)
                    ShootMissile();

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_FlyAway);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_FlyAway(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.FlyAway_Right : Action.FlyAway_Left;
                MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? FLY_AWAY_SPEED_X : -FLY_AWAY_SPEED_X };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (!AnimatedObject.IsFramed)
                {
                    if ((IsFacingRight && Position.X - Scene.MainActor.Position.X > FLY_AWAY_DETECTION_DISTANCE_X) ||
                        (IsFacingLeft && Scene.MainActor.Position.X - Position.X > FLY_AWAY_DETECTION_DISTANCE_X))
                    {
                        ProcessMessage(this, Message.Destroy);
                    }
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

                if (Engine.Sem.IsSongPlaying(Rayman3SoundEvent.Play__CagouRit_Mix03))
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__CagouRit_Mix03, this);

                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__CaFlyDie_Mix03, this);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Wait);
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