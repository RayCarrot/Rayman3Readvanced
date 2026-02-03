using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class UnusedEnemyMode7 : Mode7Actor
{
    public UnusedEnemyMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.BgPriority = 0;
        Direction = 0;
        ZPos = 0;
        RenderHeight = 32;

        State.SetTo(_Fsm_Default);
    }
}