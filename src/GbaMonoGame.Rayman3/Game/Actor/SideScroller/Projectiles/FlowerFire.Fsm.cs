using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class FlowerFire
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimatedObject.CurrentAnimation = 2;
                break;
            
            case FsmAction.Step:
                if (AnimatedObject.EndOfAnimation)
                    AnimatedObject.CurrentAnimation = 0;

                Timer--;

                if (Timer == 0)
                {
                    AnimatedObject.CurrentAnimation = 2;
                    ProcessMessage(this, Message.Destroy);
                    Platform.Destroy();
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_End(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimatedObject.CurrentAnimation = 1;
                break;

            case FsmAction.Step:
                if (AnimatedObject.EndOfAnimation)
                {
                    State.MoveTo(_Fsm_Default);
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