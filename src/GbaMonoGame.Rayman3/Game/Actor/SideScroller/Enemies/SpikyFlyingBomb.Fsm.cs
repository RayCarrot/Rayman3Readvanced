using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class SpikyFlyingBomb
{
    private bool FsmStep_CheckDeath()
    {
        // Damage main actor
        if (Scene.IsHitMainActor(this))
        {
            Scene.MainActor.ReceiveDamage(AttackPoints);
            Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
        }
        // Destroyed by boulder
        else if (Scene.IsHitActor(this) is { Type: (int)ActorType.Boulder })
        {
            Destroyed = true;
        }

        if (Destroyed)
        {
            State.MoveTo(Fsm_Destroyed);
            return false;
        }

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = CurrentDirectionalType switch
                {
                    PhysicalTypeValue.Enemy_Left => Action.Move_Left,
                    PhysicalTypeValue.Enemy_Right => Action.Move_Right,
                    PhysicalTypeValue.Enemy_Up => Action.Move_Up,
                    PhysicalTypeValue.Enemy_Down => Action.Move_Down,
                    _ => throw new ArgumentOutOfRangeException(nameof(CurrentDirectionalType), CurrentDirectionalType, null)
                };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                CurrentDirectionalType = Scene.GetPhysicalType(Position);

                ManageSound();

                if (CurrentDirectionalType is 
                    PhysicalTypeValue.Enemy_Left or 
                    PhysicalTypeValue.Enemy_Right or 
                    PhysicalTypeValue.Enemy_Up or 
                    PhysicalTypeValue.Enemy_Down)
                {
                    State.MoveTo(Fsm_Move);
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
                Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);

                if (explosion != null)
                    explosion.Position = Position;

                ProcessMessage(this, Message.Destroy);
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_PrepareAttack);
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
    public bool Fsm_PrepareAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
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

    // Unused
    public bool Fsm_Attack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Attack;
                ChangeAction();

                Vector2 dist = Scene.MainActor.Position - Position;

                Vector2 speed = Math.Abs(dist.Y) < Math.Abs(dist.X)
                    ? new Vector2(2, 1)
                    : new Vector2(1, 2);

                if (dist.X < 0)
                    speed.X = -speed.X;

                if (dist.Y < 0)
                    speed.Y = -speed.Y;

                MechModel.Speed = speed;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                ManageSound();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Stationary(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Stationary;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                ManageSound();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}