using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class LavaSplash : MovableActor
{
    public LavaSplash(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.ObjPriority = 5;
        State.SetTo(_Fsm_Default);
    }
}