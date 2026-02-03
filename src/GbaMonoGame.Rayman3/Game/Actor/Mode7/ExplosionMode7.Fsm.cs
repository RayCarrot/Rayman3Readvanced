using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class ExplosionMode7
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Explode;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                    State.MoveTo(_Fsm_Default);
                break;

            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}