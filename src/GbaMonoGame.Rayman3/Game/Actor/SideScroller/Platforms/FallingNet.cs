using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class FallingNet : MovableActor
{
    public FallingNet(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialPosition = Position;

        State.SetTo(Fsm_Idle);
    }

    public Vector2 InitialPosition { get; }
    public byte Timer { get; set; }

    public override void Init(ActorResource actorResource)
    {
        InitWithLink(actorResource);
    }
}