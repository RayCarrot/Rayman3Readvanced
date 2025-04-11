using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class UnusedBouncyPlatform : InteractableActor
{
    public UnusedBouncyPlatform(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Idle);
    }
}