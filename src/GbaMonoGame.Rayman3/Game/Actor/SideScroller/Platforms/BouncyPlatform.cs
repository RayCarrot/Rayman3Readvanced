using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class BouncyPlatform : InteractableActor
{
    public BouncyPlatform(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        HasTrap = (Action)actorResource.FirstActionId != Action.Idle;
        State.SetTo(_Fsm_Idle);
    }

    public bool HasTrap { get; } // Unused trap behavior
    public byte Timer { get; set; }
    public bool HasTriggeredBounce { get; set; }
    public Vector2 InitialMainActorSpeed { get; set; }

    // These are added on N-Gage to support bouncy platforms used in multiplayer levels
    public byte MultiplayerCooldown { get; set; }
    public MovableActor TriggeredActor { get; set; }
    public MovableActor[] DetectedActors { get; } = new MovableActor[RSMultiplayer.PlayersCount];
}