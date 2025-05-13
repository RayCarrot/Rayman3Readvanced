using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class BouncyPlatform : InteractableActor
{
    public BouncyPlatform(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        HasTrap = (Action)actorResource.FirstActionId != Action.Idle;
        State.SetTo(Fsm_Idle);
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