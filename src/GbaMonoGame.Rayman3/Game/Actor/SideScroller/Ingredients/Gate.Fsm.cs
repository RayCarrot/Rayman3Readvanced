using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public partial class Gate
{
    public bool Fsm_Closed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Closed_Right : Action.Closed_Left;
                IsSolid = true;
                break;

            case FsmAction.Step:
                if (IsOpen)
                {
                    State.MoveTo(_Fsm_Opening);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Opening(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Opening_Right : Action.Opening_Left;
                IsSolid = false;

                if (AnimatedObject.IsFramed)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MetlGate_Mix01);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Open);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Open(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Open_Right : Action.Open_Left;
                break;

            case FsmAction.Step:
                if (!IsOpen)
                {
                    State.MoveTo(_Fsm_Closing);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Closing(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // This is incorrectly playing the opening animation again
                ActionId = IsFacingRight ? Action.Opening_Right : Action.Opening_Left;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Closed);
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