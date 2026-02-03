using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Snail : MovableActor
{
    public Snail(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        InitialXPosition = Position.X;

        State.SetTo(_Fsm_Move);
    }

    public float InitialXPosition { get; }
}