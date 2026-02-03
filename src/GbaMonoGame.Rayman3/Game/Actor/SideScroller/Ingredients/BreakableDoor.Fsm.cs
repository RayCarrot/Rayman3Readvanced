using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class BreakableDoor
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (ActionId is Action.Shake_Right or Action.Shake_Left && IsActionFinished)
                    ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;

                if (ActionId is Action.Break_Right or Action.Break_Left)
                {
                    State.MoveTo(_Fsm_Break);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Break(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodBrk1_Mix04);
                break;

            case FsmAction.Step:
                if (ActionId is Action.Break_Right or Action.Break_Left && IsActionFinished)
                {
                    State.MoveTo(_Fsm_Idle);
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