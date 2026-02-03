using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class BarrelSplash : BaseActor
{
    public BarrelSplash(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.CurrentAnimation = 0;
        AnimatedObject.ObjPriority = 31;

        State.SetTo(_Fsm_Default);
    }
}