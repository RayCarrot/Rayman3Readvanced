using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class ZombieChicken
{
    private bool FsmStep_CheckDeath()
    {
        bool isFramed = AnimatedObject.IsFramed;

        if (isFramed && !HasPlayedSound && SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__RireMumu_Mix03))
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RireMumu_Mix03);

        HasPlayedSound = isFramed;

        if (HitPoints == 0)
        {
            State.MoveTo(Fsm_Dying);
            return false;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                break;

            case FsmAction.Step:
                Box actionBox = GetActionBox();

                if (IsFacingRight)
                {
                    actionBox.MinX -= Scene.Resolution.X;
                    actionBox.MaxX += 75;
                }
                else
                {
                    actionBox.MinX -= 75;
                    actionBox.MaxX += Scene.Resolution.X;
                }

                if (Scene.MainActor.GetDetectionBox().Intersects(actionBox))
                {
                    State.MoveTo(Fsm_MoveDown);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveDown(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.MoveDown_Right : Action.MoveDown_Left;
                ChangeAction();
                AnimatedObject.CurrentFrame = LastMoveFrame;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                ManageDirection();

                LastMoveFrame = AnimatedObject.CurrentFrame;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                if (TurnAround)
                {
                    State.MoveTo(Fsm_TurnAround);
                    return false;
                }

                if (Speed.Y >= 1)
                {
                    State.MoveTo(Fsm_MoveUp);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.MoveUp_Right : Action.MoveUp_Left;
                ChangeAction();
                AnimatedObject.CurrentFrame = LastMoveFrame;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                ManageDirection();

                LastMoveFrame = AnimatedObject.CurrentFrame;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                if (TurnAround)
                {
                    State.MoveTo(Fsm_TurnAround);
                    return false;
                }

                if (Speed.Y <= -1)
                {
                    State.MoveTo(Fsm_MoveDown);
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
                ActionId = IsFacingRight ? Action.Attack_Right : Action.Attack_Left;
                ChangeAction();
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

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

    public bool Fsm_TurnAround(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Position = Position with { Y = InitialYPosition };
                TurnAround = false;
                ActionId = IsFacingRight ? Action.TurnAround_Left : Action.TurnAround_Right;
                ChangeAction();
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_MoveDown);
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
                if (IsFacingRight)
                    ActionId = FaceLeftWhenDying ? Action.DyingFront_Left : Action.DyingBack_Right;
                else
                    ActionId = FaceLeftWhenDying ? Action.DyingBack_Left : Action.DyingFront_Right;
                
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__GhstDead_Mix05);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                    ProcessMessage(this, Message.Destroy);

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
}