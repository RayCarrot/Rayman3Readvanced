using System;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class MovingPlatform : MovableActor
{
    public MovingPlatform(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Resource = actorResource;
        
        AnimatedObject.ObjPriority = 50;
        ReturnXPosition = Position.X - 48;
        
        Setup();
    }

    // Not used in any level
    private readonly Dictionary<Action, AcceleratedInfo> _acceleratedInfos = new()
    {
        [Action.MoveAccelerated_Left] = new AcceleratedInfo(
            Acceleration: MathHelpers.FromFixedPoint(-0x800), 
            Deceleration: MathHelpers.FromFixedPoint(-0x500), 
            DecelerationDistance: new Vector2(-24, 0), 
            NextType: PhysicalTypeValue.MovingPlatform_Right),
        [Action.MoveAccelerated_Right] = new AcceleratedInfo(
            Acceleration: MathHelpers.FromFixedPoint(0x800), 
            Deceleration: MathHelpers.FromFixedPoint(0x500), 
            DecelerationDistance: new Vector2(24, 0), 
            NextType: PhysicalTypeValue.MovingPlatform_Left),
        [Action.MoveAccelerated_Up] = new AcceleratedInfo(
            Acceleration: MathHelpers.FromFixedPoint(-0x800), 
            Deceleration: MathHelpers.FromFixedPoint(-0x500), 
            DecelerationDistance: new Vector2(0, -24), 
            NextType: PhysicalTypeValue.MovingPlatform_Down),
        [Action.MoveAccelerated_Down] = new AcceleratedInfo(
            Acceleration: MathHelpers.FromFixedPoint(0x800), 
            Deceleration: MathHelpers.FromFixedPoint(0x500), 
            DecelerationDistance: new Vector2(0, 24), 
            NextType: PhysicalTypeValue.MovingPlatform_Up),
    };

    public ActorResource Resource { get; }
    public float ReturnXPosition { get; }
    public float PlatformSpeed { get; set; }
    public FlowerFire Fire { get; set; }
    public Action InitialAction { get; set; }
    public byte Timer { get; set; }
    public PhysicalTypeValue CurrentDirectionalType { get; set; }
    public PhysicalTypeValue PreviousPhysicalType { get; set; }
    public PhysicalTypeValue PreviousValidDirectionalType { get; set; }
    public Action ActionAfterImpact { get; set; }

    // Flags
    public bool IsActivated { get; set; }
    public bool IsAccelerating { get; set; }
    public bool ShouldDraw { get; set; }

    private bool IsDirectionalType(PhysicalTypeValue value)
    {
        return value is PhysicalTypeValue.MovingPlatform_Left or
            PhysicalTypeValue.MovingPlatform_Right or
            PhysicalTypeValue.MovingPlatform_Up or
            PhysicalTypeValue.MovingPlatform_Down or
            PhysicalTypeValue.MovingPlatform_DownLeft or
            PhysicalTypeValue.MovingPlatform_DownRight or
            PhysicalTypeValue.MovingPlatform_UpRight or
            PhysicalTypeValue.MovingPlatform_UpLeft;
    }

    private bool IsRotationalType(PhysicalTypeValue value)
    {
        return value is PhysicalTypeValue.MovingPlatform_CounterClockwise45 or
            PhysicalTypeValue.MovingPlatform_CounterClockwise90 or
            PhysicalTypeValue.MovingPlatform_CounterClockwise135 or
            PhysicalTypeValue.MovingPlatform_CounterClockwise180 or
            PhysicalTypeValue.MovingPlatform_CounterClockwise225 or
            PhysicalTypeValue.MovingPlatform_CounterClockwise270 or
            PhysicalTypeValue.MovingPlatform_CounterClockwise315;
    }

    private Action DirectionalTypeToMoveAction(PhysicalTypeValue value)
    {
        return value switch
        {
            PhysicalTypeValue.MovingPlatform_Stop => Action.Stationary,
            PhysicalTypeValue.MovingPlatform_Left => Action.Move_Left,
            PhysicalTypeValue.MovingPlatform_Right => Action.Move_Right,
            PhysicalTypeValue.MovingPlatform_Up => Action.Move_Up,
            PhysicalTypeValue.MovingPlatform_Down => Action.Move_Down,
            PhysicalTypeValue.MovingPlatform_DownLeft => Action.Move_DownLeft,
            PhysicalTypeValue.MovingPlatform_DownRight => Action.Move_DownRight,
            PhysicalTypeValue.MovingPlatform_UpRight => Action.Move_UpRight,
            PhysicalTypeValue.MovingPlatform_UpLeft => Action.Move_UpLeft,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    private Action DirectionalTypeToMoveAccelerated(PhysicalTypeValue value)
    {
        return value switch
        {
            PhysicalTypeValue.MovingPlatform_Left => Action.MoveAccelerated_Left,
            PhysicalTypeValue.MovingPlatform_Right => Action.MoveAccelerated_Right,
            PhysicalTypeValue.MovingPlatform_Up => Action.MoveAccelerated_Up,
            PhysicalTypeValue.MovingPlatform_Down => Action.MoveAccelerated_Down,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    private PhysicalTypeValue ReverseDirection(PhysicalTypeValue value)
    {
        return value switch
        {
            PhysicalTypeValue.MovingPlatform_Left => PhysicalTypeValue.MovingPlatform_Right,
            PhysicalTypeValue.MovingPlatform_Right => PhysicalTypeValue.MovingPlatform_Left,
            PhysicalTypeValue.MovingPlatform_Up => PhysicalTypeValue.MovingPlatform_Down,
            PhysicalTypeValue.MovingPlatform_Down => PhysicalTypeValue.MovingPlatform_Up,
            PhysicalTypeValue.MovingPlatform_DownLeft => PhysicalTypeValue.MovingPlatform_UpRight,
            PhysicalTypeValue.MovingPlatform_DownRight => PhysicalTypeValue.MovingPlatform_UpLeft,
            PhysicalTypeValue.MovingPlatform_UpRight => PhysicalTypeValue.MovingPlatform_DownLeft,
            PhysicalTypeValue.MovingPlatform_UpLeft => PhysicalTypeValue.MovingPlatform_DownRight,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    private PhysicalType Rotate(PhysicalTypeValue type)
    {
        Action currentActionId = ActionId == Action.Impact ? ActionAfterImpact : ActionId;

        PhysicalTypeValue[] rotationTable =
        [
            PhysicalTypeValue.MovingPlatform_DownLeft,
            PhysicalTypeValue.MovingPlatform_Down,
            PhysicalTypeValue.MovingPlatform_DownRight,
            PhysicalTypeValue.MovingPlatform_Right,
            PhysicalTypeValue.MovingPlatform_UpRight,
            PhysicalTypeValue.MovingPlatform_Up,
            PhysicalTypeValue.MovingPlatform_UpLeft,
            PhysicalTypeValue.MovingPlatform_Left
        ];

        int tableStartIndex = currentActionId switch
        {
            Action.Move_Left => 0,
            Action.Move_Right => 4,
            Action.Move_Up => 6,
            Action.Move_Down => 2,
            Action.Move_DownLeft => 1,
            Action.Move_DownRight => 3,
            Action.Move_UpRight => 5,
            Action.Move_UpLeft => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(currentActionId), currentActionId, null),
        };

        int tableStep = type switch
        {
            PhysicalTypeValue.MovingPlatform_CounterClockwise45 => 0,
            PhysicalTypeValue.MovingPlatform_CounterClockwise90 => 1,
            PhysicalTypeValue.MovingPlatform_CounterClockwise135 => 2,
            PhysicalTypeValue.MovingPlatform_CounterClockwise180 => 3,
            PhysicalTypeValue.MovingPlatform_CounterClockwise225 => 4,
            PhysicalTypeValue.MovingPlatform_CounterClockwise270 => 5,
            PhysicalTypeValue.MovingPlatform_CounterClockwise315 => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return rotationTable[(tableStartIndex + tableStep) % 8];
    }

    private PhysicalType RotateReverse(PhysicalTypeValue type)
    {
        Action currentActionId = ActionId == Action.Impact ? ActionAfterImpact : ActionId;

        PhysicalTypeValue[] rotationTable =
        [
            PhysicalTypeValue.MovingPlatform_DownRight,
            PhysicalTypeValue.MovingPlatform_Down,
            PhysicalTypeValue.MovingPlatform_DownLeft,
            PhysicalTypeValue.MovingPlatform_Left,
            PhysicalTypeValue.MovingPlatform_UpLeft,
            PhysicalTypeValue.MovingPlatform_Up,
            PhysicalTypeValue.MovingPlatform_UpRight,
            PhysicalTypeValue.MovingPlatform_Right
        ];

        int tableStartIndex = currentActionId switch
        {
            Action.Move_Left => 0,
            Action.Move_Right => 4,
            Action.Move_Up => 2,
            Action.Move_Down => 6,
            Action.Move_DownLeft => 7,
            Action.Move_DownRight => 5,
            Action.Move_UpRight => 3,
            Action.Move_UpLeft => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(currentActionId), currentActionId, null),
        };

        int tableStep = type switch
        {
            PhysicalTypeValue.MovingPlatform_CounterClockwise45 => 0,
            PhysicalTypeValue.MovingPlatform_CounterClockwise90 => 1,
            PhysicalTypeValue.MovingPlatform_CounterClockwise135 => 2,
            PhysicalTypeValue.MovingPlatform_CounterClockwise180 => 3,
            PhysicalTypeValue.MovingPlatform_CounterClockwise225 => 4,
            PhysicalTypeValue.MovingPlatform_CounterClockwise270 => 5,
            PhysicalTypeValue.MovingPlatform_CounterClockwise315 => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return rotationTable[(tableStartIndex + tableStep) % 8];
    }

    private void UpdateFire()
    {
        if (Fire == null)
            return;

        Timer++;
        Fire.Position = Position;

        if (AnimatedObject.EndOfAnimation)
            AnimatedObject.CurrentAnimation = 6;

        if (Timer == 120)
        {
            Fire.ProcessMessage(this, Message.Actor_LightOnFireRight);

            if (Scene.MainActor.LinkedMovementActor == this)
            {
                Scene.MainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
            }
            else
            {
                Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
                if (explosion != null)
                    explosion.Position = Position;
                Destroy();
            }
        }
        else if (Timer == 121)
        {
            Scene.MainActor.ProcessMessage(this, Message.Rayman_AllowSafetyJump, this);
            Scene.MainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);

            Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
            if (explosion != null)
                explosion.Position = Position - new Vector2(0, 8);
            Destroy();
        }
    }

    private void Setup()
    {
        PreviousPhysicalType = 0;
        CurrentDirectionalType = PhysicalTypeValue.MovingPlatform_Stop;
        IsActivated = true;
        IsAccelerating = true;
        Fire = null;
        Timer = 0;
        ShouldDraw = true;
        InitialAction = (Action)Resource.FirstActionId;

        // Set default direction to move
        if (InitialAction is Action.Move_Left or Action.MoveAccelerated_Left or Action.Unused_Left)
        {
            CurrentDirectionalType = PhysicalTypeValue.MovingPlatform_Left;
        }
        else if (InitialAction is Action.Move_Right or Action.MoveAccelerated_Right or Action.Unused_Right)
        {
            CurrentDirectionalType = PhysicalTypeValue.MovingPlatform_Right;
        }
        else if (InitialAction is Action.Move_Up or Action.MoveAccelerated_Up or Action.Unused_Up)
        {
            CurrentDirectionalType = PhysicalTypeValue.MovingPlatform_Up;
        }
        else if (InitialAction is Action.Move_Down or Action.MoveAccelerated_Down or Action.Unused_Down)
        {
            CurrentDirectionalType = PhysicalTypeValue.MovingPlatform_Down;
        }
        else if (InitialAction is Action.WaitForProximity or Action.WaitForContact or Action.WaitForContactWithReturn)
        {
            IsActivated = false;
        }

        if (IsDirectionalType(CurrentDirectionalType))
            PreviousValidDirectionalType = CurrentDirectionalType;

        // Only the normal move state is used in the game
        if (InitialAction is Action.Unused_Left or Action.Unused_Right or Action.Unused_Up or Action.Unused_Down)
            State.SetTo(null);
        else if (InitialAction is Action.MoveAccelerated_Left or Action.MoveAccelerated_Right or Action.MoveAccelerated_Up or Action.MoveAccelerated_Down)
            State.SetTo(_Fsm_MoveAccelerated);
        else
            State.SetTo(_Fsm_Move);
    }

    public void Destroy()
    {
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);

        if (ResurrectsImmediately)
            State.MoveTo(_Fsm_Respawn);
        else
            ProcessMessage(this, Message.Destroy);
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (ShouldDraw)
            base.Draw(animationPlayer, forceDraw);
    }

    private record AcceleratedInfo(float Acceleration, float Deceleration, Vector2 DecelerationDistance, PhysicalTypeValue NextType);
}