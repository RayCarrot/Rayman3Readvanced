using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class RedShell : MovableActor
{
    public RedShell(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        switch ((Action)actorResource.FirstActionId)
        {
            case Action.WaitingToCharge_Right:
            case Action.WaitingToCharge_Left:
                State.SetTo(_Fsm_WaitingToCharge);
                break;

            // Unused
            case Action.Sleep_Right:
            case Action.Sleep_Left:
                State.SetTo(_Fsm_Sleeping);
                break;

            // Unused
            default:
                State.SetTo(_Fsm_WaitingToPatrol);
                break;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        DestroyIfPastLinkedCheckpoint(actorResource);
    }
}