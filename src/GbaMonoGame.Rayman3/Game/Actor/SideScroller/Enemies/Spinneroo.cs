using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: CagoulardCanopy
public sealed partial class Spinneroo : MovableActor
{
    public Spinneroo(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        TauntTimer = 0;
        IsObjectCollisionXOnly = true;

        State.SetTo(Fsm_Wait);
    }

    public byte TauntTimer { get; set; }

    private bool ShouldTurnAround()
    {
        PhysicalType type = Scene.GetPhysicalType(Position + Tile.Up);
        
        if (IsFacingRight)
            return type == PhysicalTypeValue.Enemy_Left;
        else
            return type == PhysicalTypeValue.Enemy_Right;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Hit:
                // Reset hitpoints since you can't deal damage by hitting the enemy
                HitPoints = 5;

                if (State == Fsm_Walk && TauntTimer == 0)
                {
                    State.MoveTo(Fsm_Taunt);
                    TauntTimer = 180;
                }
                return false;

            default:
                return false;
        }
    }
}