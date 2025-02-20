using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class EnergyBall : MovableActor
{
    public EnergyBall(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Default);
    }
}