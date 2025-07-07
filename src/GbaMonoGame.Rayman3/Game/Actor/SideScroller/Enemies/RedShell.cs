using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class RedShell : MovableActor
{
    public RedShell(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        switch ((Action)actorResource.FirstActionId)
        {
            case Action.WaitingToCharge_Right:
            case Action.WaitingToCharge_Left:
                State.SetTo(Fsm_WaitingToCharge);
                break;

            // Unused
            case Action.Sleep_Right:
            case Action.Sleep_Left:
                State.SetTo(Fsm_Sleeping);
                break;

            // Unused
            default:
                State.SetTo(Fsm_WaitingToPatrol);
                break;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        DestroyIfPastLinkedCheckpoint(actorResource);
    }
}