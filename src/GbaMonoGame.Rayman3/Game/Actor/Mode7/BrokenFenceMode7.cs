using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class BrokenFenceMode7 : Mode7Actor
{
    public BrokenFenceMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 64;

        State.SetTo(Fsm_Default);
    }
}