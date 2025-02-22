using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class WaterSplashMode7
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (IsActionFinished && ActionId == Action.Splash)
                    ProcessMessage(this, Message.Destroy);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}