using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class LavaSplash
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (ActionId == Action.DrownSplash)
                {
                    if (IsActionFinished)
                    {
                        ProcessMessage(this, Message.Destroy);
                    }
                    else
                    {
                        if (LinkedMovementActor != null)
                            LinkedMovementActor = null;
                    }
                }
                else
                {
                    if (IsActionFinished && ActionId == Action.PlumLandedSplash)
                        ActionId = Action.Idle;
                    else if (Speed.X != 0 && ActionId == Action.Idle)
                        ActionId = Action.Moving;
                    else if (Speed.X == 0 && ActionId == Action.Moving)
                        ActionId = Action.Idle;
                }

                if (LinkedMovementActor != null)
                    Position = LinkedMovementActor.Position;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}