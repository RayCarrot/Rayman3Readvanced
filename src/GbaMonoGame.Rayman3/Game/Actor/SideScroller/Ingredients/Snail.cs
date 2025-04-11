using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Snail : MovableActor
{
    public Snail(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialXPosition = Position.X;

        State.SetTo(Fsm_Move);
    }

    public float InitialXPosition { get; }
}