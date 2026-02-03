using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Spinneroo
{
    private bool FsmStep_CheckDeath()
    {
        bool fall = false;
        bool attack = false;

        if (TauntTimer != 0)
            TauntTimer--;

        if (Scene.IsHitMainActor(this))
            Scene.MainActor.ReceiveDamage(AttackPoints);

        // Check for falling
        if (Speed.Y > 0.25f)
        {
            PhysicalType type = Scene.GetPhysicalType(Position + (IsFacingRight ? Tile.Right : Tile.Left));
            if (type == PhysicalTypeValue.None)
                fall = true;
        }

        // Check for distance to attack
        if (State != _Fsm_Attack)
        {
            if (IsFacingRight && Scene.MainActor.Position.X > Position.X && Scene.MainActor.Position.X - Position.X <= 44)
                attack = true;
            else if (IsFacingLeft && Position.X > Scene.MainActor.Position.X && Position.X - Scene.MainActor.Position.X <= 44)
                attack = true;
        }

        if (HitPoints == 0)
        {
            State.MoveTo(_Fsm_Dying);
            return false;
        }

        if (fall)
        {
            State.MoveTo(_Fsm_BeginFall);
            return false;
        }

        if (attack)
        {
            State.MoveTo(_Fsm_Attack);
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
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;
                
                if (Scene.IsDetectedMainActor(this, 0, 0, -120, 120)) 
                {
                    State.MoveTo(_Fsm_Walk);
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
                ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (ShouldTurnAround())
                {
                    State.MoveTo(_Fsm_TurnAround);
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
                ActionId = IsFacingRight ? Action.Idle_Left : Action.Idle_Right;
                ChangeAction();
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);
                State.MoveTo(_Fsm_Walk);
                return false;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Taunt(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Taunt_Right : Action.Taunt_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Walk);
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
                ActionId = IsFacingRight ? Action.BeginAttack_Right : Action.BeginAttack_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (IsActionFinished)
                {
                    if (ActionId is Action.BeginAttack_Right or Action.BeginAttack_Left)
                    {
                        ActionId = IsFacingRight ? Action.Attack1_Right : Action.Attack1_Left;
                    }
                    else if (ActionId is Action.Attack1_Right or Action.Attack1_Left)
                    {
                        ActionId = IsFacingRight ? Action.Attack2_Right : Action.Attack2_Left;
                        ChangeAction();
                    }
                }

                if (ShouldTurnAround())
                {
                    State.MoveTo(_Fsm_TurnAround);
                    return false;
                }

                if (IsActionFinished && ActionId is Action.Attack2_Right or Action.Attack2_Left)
                {
                    State.MoveTo(_Fsm_Walk);
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
    public bool Fsm_Unused1(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Unused1_Right : Action.Unused1_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_Unused2(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Unused2_Right : Action.Unused2_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_BeginFall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.BeginFall_Right : Action.BeginFall_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                PhysicalType type = Scene.GetPhysicalType(Position + Tile.Down);

                if (type == PhysicalTypeValue.InstaKill || type == PhysicalTypeValue.MoltenLava)
                {
                    State.MoveTo(_Fsm_Dying);
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
                IsSolid = false;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    ProcessMessage(this, Message.Destroy);
                    LevelMusicManager.StopSpecialMusic();
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}