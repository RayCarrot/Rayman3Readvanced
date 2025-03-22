using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Leaf : MovableActor
{
    public Leaf(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Default);
    }

    public int Delay { get; set; }
    public int AnimationSet { get; set; }
}