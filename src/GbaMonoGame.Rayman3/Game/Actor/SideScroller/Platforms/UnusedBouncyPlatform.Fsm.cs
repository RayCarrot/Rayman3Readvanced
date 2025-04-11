using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class UnusedBouncyPlatform
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Bounce);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Bounce(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Bounce;
                Scene.MainActor.ProcessMessage(this, Message.Main_Bounce);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_DealDamage);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_DealDamage(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.DealDamage;
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}