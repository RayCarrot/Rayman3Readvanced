using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed class ScalemanShadow : BaseActor
{
    public ScalemanShadow(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource)
    {
        State.SetTo(null);
    }
}