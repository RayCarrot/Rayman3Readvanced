namespace GbaMonoGame.Rayman3;

public partial class RaymanMode7
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // TODO: Implement
                break;

            case FsmAction.Step:
                // TODO: Implement
                return true;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}