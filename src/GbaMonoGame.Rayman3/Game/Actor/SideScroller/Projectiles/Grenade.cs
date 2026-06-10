using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

// TODO: Despawn on Rayman respawn death
[GenerateFsmFields]
public sealed partial class Grenade : MovableActor
{
    public Grenade(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Default);
    }

    public byte TouchingMapTimer { get; set; }
}