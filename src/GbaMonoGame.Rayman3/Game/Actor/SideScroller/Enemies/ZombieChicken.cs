using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class ZombieChicken : MovableActor
{
    public ZombieChicken(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        TurnAround = false;
        InitialYPosition = Position.Y;
        LastMoveFrame = 0;

        State.SetTo(Fsm_Idle);
    }

    public float InitialYPosition { get; }
    public bool TurnAround { get; set; }
    public int LastMoveFrame { get; set; }
    public bool HasPlayedSound { get; set; }
    public bool FaceLeftWhenDying { get; set; }

    private void ManageDirection()
    {
        PhysicalType type = Scene.GetPhysicalType(Position);

        if (IsFacingRight)
        {
            if (type == PhysicalTypeValue.Enemy_Left)
            {
                TurnAround = true;
                LastMoveFrame = 0;
            }
        }
        else
        {
            if (type == PhysicalTypeValue.Enemy_Right)
            {
                TurnAround = true;
                LastMoveFrame = 0;
            }
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_Hit:
                FaceLeftWhenDying = ((BaseActor)param).IsFacingRight;
                return false;

            default:
                return false;
        }
    }
}