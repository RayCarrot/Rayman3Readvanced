namespace GbaMonoGame.Rayman3;

public partial class Snail
{
    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                MechModel.Speed = MechModel.Speed with { Y = 2 };
                break;

            case FsmAction.Step:
                bool turnAround = (IsFacingRight && Position.X > InitialXPosition + 100) ||
                                  (IsFacingLeft && Position.X < InitialXPosition - 100);

                if (IsActionFinished)
                    ActionId = ActionId;

                if (turnAround)
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
                ActionId = IsFacingRight ? Action.TurnAround_Right : Action.TurnAround_Left;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ActionId = IsFacingRight ? Action.Move_Left : Action.Move_Right;
                break;
        }

        return true;
    }
}