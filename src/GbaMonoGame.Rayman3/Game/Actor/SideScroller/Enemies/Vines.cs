using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Vines : InteractableActor
{
    public Vines(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Init);
    }

    public bool IsFacingDown { get; set; }
    public byte Timer { get; set; }
}