using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class UnusedEnemyMode7 : Mode7Actor
{
    public UnusedEnemyMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.BgPriority = 0;
        Direction = 0;
        ZPos = 0;
        RenderHeight = 32;

        State.SetTo(Fsm_Default);
    }
}