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
            State == Fsm_Attack && 
            AnimatedObject.CurrentFrame is > 0 and < 4 or > 5 and < 9) 
        {
            ShootMissile();
        }

        if (HitPoints == 0)
        {
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

                // Check if in range of the main actor
                if ((IsFacingRight && Scene.MainActor.Position.X - Position.X < 220) ||
                    (IsFacingLeft && Position.X - Scene.MainActor.Position.X < 220))
                {
                    State.MoveTo(Fsm_Fly);
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

    public bool Fsm_Fly(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Fly_Right : Action.Fly_Left;

                if (Rom.Platform == Platform.GBA)
                    MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? 1.5f : -1.5f };
                else if (Rom.Platform == Platform.NGage)
                    MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? 1f : -1f };
                else
                    throw new UnsupportedPlatformException();

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagouRit_Mix03);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.IsDetectedMainActor(this))
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
                ActionId = IsFacingRight ? Action.Shoot_Right : Action.Shoot_Left;

                if (Rom.Platform == Platform.GBA)
                    MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? 1.125F : -1.125F };
                else if (Rom.Platform == Platform.NGage)
                    MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? 0.75F : -0.75F };
                else
                    throw new UnsupportedPlatformException();
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (AnimatedObject.CurrentFrame is 3 or 8 && !AnimatedObject.IsDelayMode)
                    ShootMissile();

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_FlyAway);
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
                MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? 2f : -2f };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (!AnimatedObject.IsFramed)
                {
                    if ((IsFacingRight && Position.X - Scene.MainActor.Position.X > 360) ||
                        (IsFacingLeft && Scene.MainActor.Position.X - Position.X > 360))
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

                if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__CagouRit_Mix03))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__CagouRit_Mix03);

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CaFlyDie_Mix03);
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
                break;
        }

        return true;
    }
}