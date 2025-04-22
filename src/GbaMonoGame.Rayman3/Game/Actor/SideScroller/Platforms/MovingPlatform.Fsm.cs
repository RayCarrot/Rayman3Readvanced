using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class MovingPlatform
{
    private bool FsmStep_UpdatePreviousValidDirectionalType()
    {
        PhysicalType type = Scene.GetPhysicalType(Position);

        if (IsDirectionalType(type))
            PreviousValidDirectionalType = type;

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Action value = DirectionalTypeToMoveAction(CurrentDirectionalType);
                if (ActionId == Action.Impact)
                {
                    if (ActionAfterImpact != value)
                    {
                        ActionAfterImpact = value;
                        int frame = AnimatedObject.CurrentFrame;
                        ActionId = value;
                        ChangeAction();
                        ActionId = Action.Impact;
                        if (frame < AnimatedObject.GetAnimation().FramesCount)
                            AnimatedObject.CurrentFrame = frame;
                    }
                }
                else
                {
                    if (ActionId != value)
                        ActionId = value;
                }
                ChangeAction();
                break;

            case FsmAction.Step:
                if (!FsmStep_UpdatePreviousValidDirectionalType())
                    return false;

                // Return to normal action if the impact has finished
                if (ActionId == Action.Impact && IsActionFinished)
                {
                    ActionId = ActionAfterImpact;
                    ChangeAction();
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    if (InitialAction is Action.WaitForContact or Action.WaitForContactWithReturn)
                        IsActivated = true;

                    mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);

                    // Check for impact if the main actor is moving downwards
                    if (mainActor.Speed.Y > 0)
                    {
                        ActionAfterImpact = ActionId;
                        ActionId = Action.Impact;
                        ChangeAction();
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__VibraFLW_Mix02);
                    }
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }

                PhysicalType type = Scene.GetPhysicalType(Position);

                // Check for rotational type
                if (PreviousPhysicalType != type && IsRotationalType(type))
                {
                    PhysicalType newType = Rotate(type);
                    CurrentDirectionalType = newType;
                    PreviousValidDirectionalType = newType;
                }
                else
                {
                    CurrentDirectionalType = type;
                }

                PreviousPhysicalType = type;

                // If the new type if empty then we keep the last direction
                if (CurrentDirectionalType == PhysicalTypeValue.None)
                    CurrentDirectionalType = PreviousValidDirectionalType;

                if (InitialAction == Action.WaitForProximity)
                {
                    if (Scene.IsDetectedMainActor(this))
                    {
                        IsActivated = true;
                    }
                    else if (mainActor.Position.X >= Position.X - 100 &&
                             Position.X + 100 >= mainActor.Position.X &&
                             mainActor.Position.Y >= Position.Y &&
                             Position.Y + 200 >= mainActor.Position.Y)
                    {
                        IsActivated = true;
                    }
                }

                // Burn up if we stop moving
                if (CurrentDirectionalType == PhysicalTypeValue.MovingPlatform_Stop && Fire == null)
                {
                    Fire = Scene.CreateProjectile<FlowerFire>(ActorType.FlowerFire);

                    Debug.Assert(Fire != null, "The flower fire projectile could not be created");
                    
                    AnimatedObject.CurrentAnimation = 5;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BBQ_Mix10);

                    if (Fire != null)
                        Fire.Platform = this;
                }

                // Burn if we have a fire projectile
                if (Fire != null)
                {
                    State.MoveTo(Fsm_Burn);
                    return false;
                }

                // Return platform if main actor has moved away
                if (InitialAction == Action.WaitForContactWithReturn && IsActivated && mainActor.Position.X < ReturnXPosition)
                {
                    PreviousPhysicalType = PhysicalTypeValue.None;
                    State.MoveTo(Fsm_MoveReverse);
                    return false;
                }

                if (IsDirectionalType(CurrentDirectionalType) && IsActivated)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }

                if (CurrentDirectionalType == PhysicalTypeValue.MovingPlatform_FullStop)
                {
                    State.MoveTo(Fsm_Stop);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveReverse(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Action value = DirectionalTypeToMoveAction(CurrentDirectionalType);
                value = ReverseDirection(value);
                if (ActionId == Action.Impact)
                {
                    if (ActionAfterImpact != value)
                    {
                        ActionAfterImpact = value;
                        int frame = AnimatedObject.CurrentFrame;
                        ActionId = value;
                        ChangeAction();
                        ActionId = Action.Impact;
                        if (frame < AnimatedObject.GetAnimation().FramesCount)
                            AnimatedObject.CurrentFrame = frame;
                    }
                }
                else
                {
                    if (ActionId != value)
                        ActionId = value;
                }
                ChangeAction();
                break;

            case FsmAction.Step:
                if (!FsmStep_UpdatePreviousValidDirectionalType())
                    return false;

                // Return to normal action if the impact has finished
                if (ActionId == Action.Impact && IsActionFinished)
                {
                    ActionId = ActionAfterImpact;
                    ChangeAction();
                }

                MovableActor mainActor = Scene.MainActor;
                bool collided = false;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    if (InitialAction is Action.WaitForContact or Action.WaitForContactWithReturn)
                        IsActivated = true;

                    mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
                    collided = true;

                    // Check for impact if the main actor is moving downwards
                    if (mainActor.Speed.Y > 0)
                    {
                        ActionAfterImpact = ActionId;
                        ActionId = Action.Impact;
                        ChangeAction();
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__VibraFLW_Mix02);
                    }
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }

                PhysicalType type = Scene.GetPhysicalType(Position);

                // Check for rotational type
                if (PreviousPhysicalType != type && IsRotationalType(type))
                {
                    PhysicalType newType = Rotate(type);
                    CurrentDirectionalType = newType;
                    PreviousValidDirectionalType = newType;
                }
                else
                {
                    CurrentDirectionalType = type;
                }

                PreviousPhysicalType = type;

                // Reverse movement if finished
                if (CurrentDirectionalType == PhysicalTypeValue.MovingPlatform_FullStop)
                    Position -= Speed;

                // Finished reversing
                if (collided || CurrentDirectionalType == PhysicalTypeValue.MovingPlatform_FullStop)
                {
                    if (!collided)
                        IsActivated = false;

                    CurrentDirectionalType = PhysicalTypeValue.MovingPlatform_Stop;
                    PreviousPhysicalType = PhysicalTypeValue.None;
                    State.MoveTo(Fsm_Move);
                    return false;
                }

                if (IsDirectionalType(CurrentDirectionalType) && IsActivated)
                {
                    State.MoveTo(Fsm_MoveReverse);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Stop(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (ActionId == Action.Impact && IsActionFinished)
                    ActionAfterImpact = Action.Stationary;
                else
                    ActionId = Action.Stationary;
                ChangeAction();
                break;

            case FsmAction.Step:
                if (ActionId == Action.Impact && IsActionFinished)
                {
                    ActionId = ActionAfterImpact;
                    ChangeAction();
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);

                    // Check for impact if the main actor is moving downwards
                    if (mainActor.Speed.Y > 0)
                    {
                        ActionAfterImpact = ActionId;
                        ActionId = Action.Impact;
                        ChangeAction();
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__VibraFLW_Mix02);
                    }
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Burn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;
            
            case FsmAction.Step:
                PhysicalType type = Scene.GetPhysicalType(Position);

                if (PreviousPhysicalType != type && IsRotationalType(type))
                    CurrentDirectionalType = Rotate(type);
                else
                    CurrentDirectionalType = type;

                PreviousPhysicalType = type;

                if (CurrentDirectionalType == PhysicalTypeValue.MovingPlatform_FullStop)
                {
                    MechModel.Speed = Vector2.Zero;
                }
                else if (IsDirectionalType(CurrentDirectionalType))
                {
                    // Change action while retaining the animation
                    int originalAnim = AnimatedObject.CurrentAnimation;
                    int originalFrame = AnimatedObject.CurrentFrame;
                    int originalTimer = AnimatedObject.Timer;
                    bool originalIsDelayMode = AnimatedObject.IsDelayMode;

                    Action directionalAction = DirectionalTypeToMoveAction(CurrentDirectionalType);
                    if (directionalAction != ActionId)
                        ActionId = directionalAction;
                    ChangeAction();

                    AnimatedObject.CurrentAnimation = originalAnim;
                    AnimatedObject.CurrentFrame = originalFrame;
                    AnimatedObject.Timer = originalTimer;
                    AnimatedObject.IsDelayMode = originalIsDelayMode;
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }

                UpdateFire();
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Respawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ShouldDraw = false;
                break;

            case FsmAction.Step:
                Timer++;
                if (Timer == 120)
                {
                    Position = Resource.Pos.ToVector2();
                    ActionId = Action.MoveAccelerated_Left;
                    ShouldDraw = true;
                }
                else if (Timer == 121)
                {
                    if (AnimatedObject.IsFramed)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Appear_SocleFX1_Mix01);
                }

                if (IsActionFinished && ActionId == Action.MoveAccelerated_Left)
                    Setup();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveAccelerated(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Action value = DirectionalTypeToMoveAccelerated(CurrentDirectionalType);
                if (ActionId == Action.Impact)
                {
                    if (ActionAfterImpact != value)
                    {
                        ActionAfterImpact = value;
                        int frame = AnimatedObject.CurrentFrame;
                        ActionId = value;
                        ChangeAction();
                        ActionId = Action.Impact;
                        if (frame < AnimatedObject.GetAnimation().FramesCount)
                            AnimatedObject.CurrentFrame = frame;
                        PlatformSpeed = 0;
                        IsAccelerating = true;
                    }
                }
                else
                {
                    if (ActionId != value)
                    {
                        ActionId = value;
                        PlatformSpeed = 0;
                        IsAccelerating = true;
                    }
                }
                ChangeAction();
                break;
            
            case FsmAction.Step:
                Action currentActionId = ActionId == Action.Impact ? ActionAfterImpact : ActionId;

                if (currentActionId is Action.MoveAccelerated_Left or Action.MoveAccelerated_Right)
                    MechModel.Speed = MechModel.Speed with { X = PlatformSpeed };
                else
                    MechModel.Speed = MechModel.Speed with { Y = PlatformSpeed };

                if (ActionId == Action.Impact && IsActionFinished)
                {
                    ActionId = ActionAfterImpact;
                    ChangeAction();
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);

                    // Check for impact if the main actor is moving downwards
                    if (mainActor.Speed.Y > 0)
                    {
                        ActionAfterImpact = ActionId;
                        ActionId = Action.Impact;
                        ChangeAction();
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__VibraFLW_Mix02);
                    }
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }

                CurrentDirectionalType = Scene.GetPhysicalType(Position);

                if (IsAccelerating)
                {
                    if (Math.Abs(PlatformSpeed) < 1)
                        PlatformSpeed += _acceleratedInfos[currentActionId].Acceleration;
                }
                else
                {
                    if (Math.Abs(PlatformSpeed) > 0.0625)
                        PlatformSpeed -= _acceleratedInfos[currentActionId].Deceleration;
                }

                if (IsAccelerating)
                {
                    PhysicalType type = Scene.GetPhysicalType(Position + _acceleratedInfos[currentActionId].DecelerationDistance);
                    if (IsDirectionalType(type))
                        IsAccelerating = false;
                }

                if (CurrentDirectionalType == _acceleratedInfos[currentActionId].NextType)
                {
                    State.MoveTo(Fsm_MoveAccelerated);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}