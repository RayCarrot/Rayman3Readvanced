using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: Crane
public sealed partial class Skull : MovableActor
{
    public Skull(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialPosition = Position;
        Timer = 0;
        InitialAction = (Action)actorResource.FirstActionId;

        if ((Action)actorResource.FirstActionId == Action.SolidMove_Stationary)
            State.SetTo(Fsm_SolidMove);
        else
            State.SetTo(Fsm_Spawn);

        if ((Action)actorResource.FirstActionId > Action.SpinStart)
            throw new Exception("Invalid initial action for the skull actor");
    }

    public Vector2 InitialPosition { get; }
    public Action InitialAction { get; }
    public ushort Timer { get; set; }

    private bool IsHit()
    {
        Box detectionBox = GetDetectionBox();

        // Extend by 5 in all directions
        detectionBox = new Box(detectionBox.MinX - 5, detectionBox.MinY - 5, detectionBox.MaxX + 5, detectionBox.MaxY + 5);

        Rayman rayman = (Rayman)Scene.MainActor;

        for (int i = 0; i < 2; i++)
        {
            RaymanBody activeFist = rayman.ActiveBodyParts[i];

            if (activeFist != null && activeFist.GetDetectionBox().Intersects(detectionBox))
            {
                activeFist.ProcessMessage(this, Message.RaymanBody_FinishedAttack);
                return true;
            }
        }

        return false;
    }

    // Unused (unreferenced function)
    /*
    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.HitActorOfSameType:
                State.MoveTo(Fsm_FallDown);
                return false;

            default:
                return false;
        }
    }
    */
}