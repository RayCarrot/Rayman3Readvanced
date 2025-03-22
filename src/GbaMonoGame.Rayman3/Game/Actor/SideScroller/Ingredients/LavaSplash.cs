using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class LavaSplash : MovableActor
{
    public LavaSplash(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.ObjPriority = 5;
        State.SetTo(Fsm_Default);
    }
}