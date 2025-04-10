using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class BreakableWall : InteractableActor
{
    public BreakableWall(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Idle);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Hit:
                RaymanBody bodyPart = (RaymanBody)param;

                if (State == Fsm_Idle && 
                    bodyPart.BodyPartType is RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist)
                    ActionId = Action.Break;
                return false;

            default:
                return false;
        }
    }
}