using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class PumpkinMode7
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this) && ((Mode7Actor)Scene.MainActor).ZPos < 24)
                {
                    State.MoveTo(Fsm_Break);
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
                Scene.MainActor.ReceiveDamage(AttackPoints);
                ActionId = Action.Break;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PlumSnd1_Mix03);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                    ProcessMessage(this, Message.Destroy);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}