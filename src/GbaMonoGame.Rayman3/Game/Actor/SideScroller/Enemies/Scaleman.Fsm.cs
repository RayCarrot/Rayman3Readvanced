using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Scaleman
{
    public bool Fsm_PreInit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                break;

            case FsmAction.Step:
                State.MoveTo(Fsm_Init);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Init(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    if (ActionId is not (Action.Idle_Right or Action.Idle_Left))
                    {
                        State.MoveTo(Fsm_BallRoll);
                        return false;
                    }

                    if (Timer >= 90)
                        ActionId = IsFacingRight ? Action.Submerge_Right : Action.Submerge_Left;
                }

                Timer++;
                break;

            case FsmAction.UnInit:
                Scene.Camera.LinkedObject = Scene.MainActor;
                Scene.Camera.ProcessMessage(this, Message.Cam_MoveToLinkedObject, false);
                ((CameraSideScroller)Scene.Camera).Speed = ((CameraSideScroller)Scene.Camera).Speed with { X = -7 };
                break;
        }

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (Timer < 90)
                    ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                else
                    ActionId = IsFacingRight ? Action.Submerge_Right : Action.Submerge_Left;
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    Scene.MainActor.ProcessMessage(this, Message.Damaged);
                }

                if (IsActionFinished)
                {
                    if (ActionId is Action.Idle_Right or Action.Idle_Left)
                    {
                        if (Timer > 90)
                            ActionId = IsFacingRight ? Action.Submerge_Right : Action.Submerge_Left;
                    }
                    else
                    {
                        if (HitPoints is 6 or 3)
                        {
                            State.MoveTo(Fsm_BallRoll);
                            return false;
                        }
                        else if (HitPoints is 5 or 2)
                        {
                            // TODO: Implement
                        }
                        else if (HitPoints is 4 or 1)
                        {
                            // TODO: Implement
                        }
                    }
                }

                Timer++;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_BallRoll(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (IsSecondPhase())
                    ActionId = IsFacingRight ? Action.Ball_RollFast_Right : Action.Ball_RollFast_Left;
                else
                    ActionId = IsFacingRight ? Action.Ball_Roll_Right : Action.Ball_Roll_Left;

                Timer = 0;
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

                if (ActionId is
                    Action.Ball_Roll_Right or Action.Ball_Roll_Left or
                    Action.Ball_RollFast_Right or Action.Ball_RollFast_Left)
                {
                    PhysicalType type = Scene.GetPhysicalType(Position + (IsFacingRight ? Tile.Right : Tile.Left) * 3 + Tile.Up);
                    if (type == PhysicalTypeValue.Solid)
                    {
                        int frame = AnimatedObject.CurrentFrame;

                        // Change direction
                        if (IsSecondPhase())
                            ActionId = IsFacingRight ? Action.Ball_RollFast_Left : Action.Ball_RollFast_Right;
                        else
                            ActionId = IsFacingRight ? Action.Ball_Roll_Left : Action.Ball_Roll_Right;
                        ChangeAction();

                        AnimatedObject.CurrentFrame = frame;
                    }
                }

                if (IsActionFinished)
                {
                    // Emerge from ball
                    if (ActionId is
                        Action.Ball_Roll_Right or Action.Ball_Roll_Left or
                        Action.Ball_RollFast_Right or Action.Ball_RollFast_Left)
                    {
                        if (IsSecondPhase())
                        {
                            if (Timer >= 300)
                                ActionId = IsFacingRight ? Action.Emerge_Right : Action.Emerge_Left;
                        }
                        else
                        {
                            if (Timer >= 360)
                                ActionId = IsFacingRight ? Action.Emerge_Right : Action.Emerge_Left;
                        }
                    }
                    // Finished emerging
                    else if (ActionId is Action.Emerge_Right or Action.Emerge_Left)
                    {
                        Timer = 30;
                        State.MoveTo(Fsm_Default);
                        return false;
                    }
                }

                Timer++;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Shrink(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    Scene.MainActor.ProcessMessage(this, Message.Damaged);
                }

                if (IsActionFinished)
                {
                    if (ActionId is not (Action.Hit_Right or Action.HitBehind_Left or Action.HitBehind_Right or Action.Hit_Left))
                    {
                        State.MoveTo(Fsm_Shrunk);
                        return false;
                    }

                    if (Timer >= 30)
                        ActionId = IsFacingRight ? Action.Shrink_Right : Action.Shrink_Left;
                }

                Timer++;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Shrunk(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Small_Idle_Right : Action.Small_Idle_Left;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_SmallRun);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SmallRun(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__ScalFlee_Mix02);

                if (IsSecondPhase())
                    ActionId = IsFacingRight ? Action.Small_RunFast_Right : Action.Small_RunFast_Left;
                else
                    ActionId = IsFacingRight ? Action.Small_Run_Right : Action.Small_Run_Left;
                
                Timer = 0;
                break;

            case FsmAction.Step:
                // TODO: Implement
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__ScalFlee_Mix02);
                break;
        }

        return true;
    }
}