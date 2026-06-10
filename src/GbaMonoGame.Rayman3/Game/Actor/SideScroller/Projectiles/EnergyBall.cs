using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

// TODO: Despawn on Rayman respawn death
[GenerateFsmFields]
public sealed partial class EnergyBall : MovableActor
{
    public EnergyBall(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Default);
    }
}