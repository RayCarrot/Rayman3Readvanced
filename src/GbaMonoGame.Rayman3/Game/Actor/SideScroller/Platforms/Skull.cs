using System;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

// Original name: Crane
[GenerateFsmFields]
public sealed partial class Skull : MovableActor
{
    public Skull(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        InitialPosition = Position;
        Timer = 0;
        InitialAction = (Action)actorResource.FirstActionId;

        if ((Action)actorResource.FirstActionId == Action.SolidMove_Stationary)
            State.SetTo(_Fsm_SolidMove);
        else
            State.SetTo(_Fsm_Spawn);

        if ((Action)actorResource.FirstActionId is not (
            Action.SolidMove_Stationary or 
            Action.Move_Left or 
            Action.Move_Right or 
            Action.Move_Up or 
            Action.Move_Down or 
            Action.SpinStart))
            throw new Exception("Invalid initial action for the skull actor");
    }

    public Vector2 InitialPosition { get; }
    public Action InitialAction { get; }
    public ushort Timer { get; set; }

    private bool IsHit()
    {
        Box detectionBox = GetDetectionBox();

        // Extend by 5 in all directions
        detectionBox.Left -= 5;
        detectionBox.Top -= 5;
        detectionBox.Right += 5;
        detectionBox.Bottom += 5;

        Rayman rayman = (Rayman)Scene.MainActor;

        for (int i = 0; i < 2; i++)
        {
            RaymanBody activeFist = rayman.ActiveBodyParts[i];

            if (activeFist != null && activeFist.GetDetectionBox().Intersects(detectionBox))
            {
                activeFist.ProcessMessage(this, Message.RaymanBody_FinishAttack);
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
            case Message.Actor_CollideWithSameType:
                State.MoveTo(_Fsm_FallDown);
                return false;

            default:
                return false;
        }
    }
    */
}