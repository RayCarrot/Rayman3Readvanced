using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Grenade : MovableActor
{
    public Grenade(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Default);
    }

    public byte TouchingMapTimer { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            // Despawn on respawn death
            case Message.Readvanced_ResetOnRespawnDeath:
                State.MoveTo(_Fsm_Default);
                return false;

            default:
                return false;
        }
    }
}