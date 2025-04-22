using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: BombeVolante
public sealed partial class FlyingBomb : MovableActor
{
    public FlyingBomb(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        SoundDelay = 0;
        Destroyed = false;

        Action firstActionId = (Action)actorResource.FirstActionId;

        // Hard-code an object to be removed. Odd that it's not a level change - maybe a last minute decision?
        if (GameInfo.MapId == MapId.SanctuaryOfStoneAndFire_M1)
        {
            if (InstanceId == 63)
                ProcessMessage(this, Message.Destroy);
        }
        else if (GameInfo.MapId == MapId.BeneathTheSanctuary_M1)
        {
            if (InstanceId == 68)
            {
                ActionId = Action.Stationary;
                firstActionId = Action.Stationary;
            }
        }

        if (firstActionId == Action.Wait)
        {
            State.SetTo(Fsm_Wait);
        }
        else if (firstActionId == Action.Stationary)
        {
            State.SetTo(Fsm_Stationary);
        }
        else
        {
            CurrentDirectionalType = firstActionId switch
            {
                Action.Move_Left => PhysicalTypeValue.Enemy_Left,
                Action.Move_Right => PhysicalTypeValue.Enemy_Right,
                Action.Move_Up => PhysicalTypeValue.Enemy_Up,
                Action.Move_Down => PhysicalTypeValue.Enemy_Down,
                _ => throw new Exception("Invalid initial action for the flying bomb")
            };
            State.SetTo(Fsm_Move);
        }
    }

    public PhysicalTypeValue? CurrentDirectionalType { get; set; } // Null is used for the boss machine level - games uses 0xFE
    public byte SoundDelay { get; set; }
    public bool Destroyed { get; set; }

    private bool HitWall()
    {
        Box vulnerabilityBox = GetVulnerabilityBox();
        vulnerabilityBox.Left -= 8;
        vulnerabilityBox.Top -= 8;
        vulnerabilityBox.Right += 8;
        vulnerabilityBox.Bottom -= 8;

        // Check bottom-right
        PhysicalType type = Scene.GetPhysicalType(vulnerabilityBox.BottomRight);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        // Check middle-right
        type = Scene.GetPhysicalType(vulnerabilityBox.MiddleRight);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        // Check top-right
        type = Scene.GetPhysicalType(vulnerabilityBox.TopRight);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        // Check top-center
        type = Scene.GetPhysicalType(vulnerabilityBox.TopCenter);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        // Check top-left
        type = Scene.GetPhysicalType(vulnerabilityBox.TopLeft);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        // Check middle-left
        type = Scene.GetPhysicalType(vulnerabilityBox.MiddleLeft);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        // Check bottom-left
        type = Scene.GetPhysicalType(vulnerabilityBox.BottomLeft);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        // Check bottom-center
        type = Scene.GetPhysicalType(vulnerabilityBox.BottomCenter);

        if (type.IsSolid || type == PhysicalTypeValue.MoltenLava)
            return true;

        return false;
    }

    public override void Step()
    {
        base.Step();
        GameInfo.ActorSoundFlags &= ~ActorSoundFlags.FlyingBomb;
    }
}