using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: We should maybe use some of the N-Gage specific code here if the option to fix bugs is enabled
public partial class WalkingShell
{
    private bool FsmStep_CheckDeath()
    {
        bool isDead = false;

        UpdateTypes();

        if (State == Fsm_Jump)
            Scene.Camera.ProcessMessage(this, Message.Cam_DoNotFollowPositionY, 120);
        else
            Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 120);

        if (IsRaymanMounted)
            Rayman.Position = Position;

        if (State != Fsm_Idle && State != Fsm_Loop)
        {
            Rayman rayman = (Rayman)Scene.MainActor;

            if (IsHitBreakableDoor() ||
                Speed.X == 0 ||
                CurrentType == PhysicalTypeValue.Water ||
                rayman.State == rayman.Fsm_Dying)
            {
                if (IsRaymanMounted)
                    Scene.MainActor.ReceiveDamage(3);

                IsRaymanMounted = false;

                ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.Default;

                Scene.MainActor.ProcessMessage(this, Message.Rayman_UnmountWalkingShell, this);

                Explode();
                isDead = true;
            }
        }

        if (Rom.Platform == Platform.NGage && isDead)
        {
            State.MoveTo(null);
            return false;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                // Mount Rayman on the shell
                if (Scene.IsDetectedMainActor(this) &&
                    Scene.MainActor.Position.Y <= Position.Y &&
                    Scene.MainActor.Position.X <= Position.X &&
                    Rayman.ActionId != Rayman.Action.WalkingShell_Mount)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__HorseCry_Mix02);

                    Scene.MainActor.ProcessMessage(this, Message.Rayman_MountWalkingShell, this);
                    Rayman.Position = Position;
                    IsRaymanMounted = true;

                    ActionId = Action.Mounting;
                    Rayman.ActionId = Rayman.Action.WalkingShell_Mount;
                    ChangeAction();
                    Rayman.ChangeAction();
                }

                if (Scene.IsDetectedMainActor(this) &&
                    Rayman.IsActionFinished &&
                    Rayman.ActionId == Rayman.Action.WalkingShell_Mount)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.WalkingShell;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoGO_Mix02);
                break;
        }

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SafetyJumpTimer = 0;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RocktLeg_Mix03);

                if (ActionId != Action.EndBoost)
                {
                    ActionId = Action.Walk;

                    if (IsRaymanMounted)
                        Rayman.ActionId = Rayman.Action.WalkingShell_Ride;
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (IsActionFinished)
                {
                    if (ActionId == Action.EndBoost)
                    {
                        ActionId = Action.Walk;

                        if (IsRaymanMounted)
                            Rayman.ActionId = Rayman.Action.WalkingShell_Ride;
                    }
                    else
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RocktLeg_Mix03);
                    }
                }

                // Boost
                if (IsRaymanMounted && JoyPad.IsButtonJustPressed(GbaInput.B))
                {
                    Rayman.ActionId = Rayman.Action.WalkingShell_BeginBoost;
                    ActionId = Action.BeginBoost;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoEfor2_Mix03);
                    State.MoveTo(Fsm_Boost);
                    return false;
                }

                // Fall down
                if (Speed.Y > 1 &&
                    CurrentType == PhysicalTypeValue.None &&
                    CurrentBottomType == PhysicalTypeValue.Water)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Loop
                if (DetectLoop())
                {
                    Timer = 0;
                    State.MoveTo(Fsm_Loop);
                    return false;
                }

                // Jump
                if (IsRaymanMounted && JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Boost(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                LoopAngle = 0;
                Timer = 0;
                SafetyJumpTimer = 0;

                if (ActionId != Action.BeginBoost)
                {
                    ActionId = Action.LongBoost;

                    if (IsRaymanMounted)
                        Rayman.ActionId = Rayman.Action.WalkingShell_Boost;
                }

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__RocktLeg_Mix03);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RoktSpin_Mix03);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                Timer++;

                if (ActionId == Action.BeginBoost && IsActionFinished)
                {
                    ActionId = Action.ShortBoost;

                    if (IsRaymanMounted)
                        Rayman.ActionId = Rayman.Action.WalkingShell_Boost;

                    Timer = 0;
                }

                if (Speed.Y <= 3)
                    LoopAngle = 0;
                else
                    LoopAngle++;

                if (Timer > 15 && ActionId == Action.ShortBoost && IsRaymanMounted)
                {
                    Rayman.ActionId = Rayman.Action.WalkingShell_EndBoost;
                    ActionId = Action.EndBoost;
                    State.MoveTo(Fsm_Move);
                    return false;
                }

                if (Timer > 60 && ActionId == Action.LongBoost && IsRaymanMounted)
                {
                    Rayman.ActionId = Rayman.Action.WalkingShell_EndBoost;
                    ActionId = Action.EndBoost;
                    State.MoveTo(Fsm_Move);
                    return false;
                }

                if (Speed.Y > 1 && CurrentType == PhysicalTypeValue.None && CurrentBottomType == PhysicalTypeValue.Water)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                if (DetectLoop())
                {
                    Timer = 0;
                    State.MoveTo(Fsm_Loop);
                    return false;
                }

                if (IsRaymanMounted && JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Jump(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                LoopAngle = 0;
                ActionId = Action.Jump;

                if (IsRaymanMounted)
                    Rayman.ActionId = Rayman.Action.WalkingShell_Jump;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);

                // Jump off if near breakable door
                if (IsNearBreakableDoor())
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    IsRaymanMounted = false;
                    ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.Default;
                    Rayman.PreviousXSpeed = Speed.X;
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_JumpOffWalkingShell, this);
                }
                break;

            case FsmAction.Step:
                bool isLoop = false;
                PhysicalType type = PhysicalTypeValue.None;

                if (!FsmStep_CheckDeath())
                    return false;

                if (Rom.Platform == Platform.NGage)
                {
                    Box detectionBox = GetDetectionBox();

                    Vector2 pos = detectionBox.TopCenter - new Vector2(0, 1);

                    type = Scene.GetPhysicalType(pos);
                    if (!type.IsSolid)
                    {
                        pos = pos with { X = detectionBox.Left };

                        type = Scene.GetPhysicalType(pos);
                        if (!type.IsSolid)
                        {
                            pos = pos with { X = detectionBox.Right };
                            type = Scene.GetPhysicalType(pos);
                        }
                    }
                }

                Timer++;

                if (LoopAngle == 0 && DetectLoop())
                    LoopAngle = 1;

                if (LoopAngle == 1)
                {
                    Vector2 diff = Position - LoopPosition;
                    float dist = diff.Length();

                    if (56 <= dist)
                    {
                        LoopAngle = (byte)MathHelpers.Atan2_256(Position - LoopPosition);
                        isLoop = true;
                    }
                }

                if (Rom.Platform == Platform.NGage && type.IsSolid)
                {
                    Explode();
                    IsRaymanMounted = false;
                    ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.Default;
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_UnmountWalkingShell, this);
                    State.MoveTo(null);
                    return false;
                }

                // Land
                if (Speed.Y == 0 && !isLoop)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }

                // Loop
                if (isLoop)
                {
                    Timer = 0xFF;
                    State.MoveTo(Fsm_Loop);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Loop(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (Timer != 0xFF)
                    LoopAngle = 64;

                Timer = 0;
                HasBoostedInLoop = true; // Set to false to re-enable unused behavior
                ActionId = Action.Loop;

                if (IsRaymanMounted)
                    Rayman.ActionId = Rayman.Action.WalkingShell_Loop;

                ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.Default;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumSwing_Mix03);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                Timer++;

                int frame = 17 - ((byte)(LoopAngle - 63) / 15);
                AnimatedObject.CurrentFrame = frame;
                Rayman.AnimatedObject.CurrentFrame = frame;

                // Unused - the value is always true
                if (!HasBoostedInLoop)
                {
                    LoopAngle -= 2;
                    if ((LoopAngle & 2) != 0)
                        LoopAngle -= 2;

                    if (JoyPad.IsButtonJustPressed(GbaInput.B) && LoopAngle is > 20 and < 64)
                        HasBoostedInLoop = true;
                }
                else
                {
                    if (LoopAngle is >= 160 and < 226)
                    {
                        if (LoopAngle is >= 180 and < 206)
                        {
                            LoopAngle -= 2;
                        }
                        else
                        {
                            LoopAngle -= 2;
                            if ((LoopAngle & 2) != 0)
                                LoopAngle -= 2;
                        }
                    }
                    else
                    {
                        LoopAngle -= 4;
                    }
                }

                Position = LoopPosition + new Vector2(MathHelpers.Cos256(LoopAngle) * LoopRadius, MathHelpers.Sin256(LoopAngle) * LoopRadius);

                if (LoopAngle == 192)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWoHoo_Mix01);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RaySpin_Mix06);
                }

                // Finished loop
                if (HasBoostedInLoop && LoopAngle is >= 64 and <= 70)
                {
                    State.MoveTo(Fsm_Boost);
                    return false;
                }

                // Unused, fall out of loop
                if (!IsRaymanMounted || (!HasBoostedInLoop && LoopAngle == 192))
                {
                    MechModel.Speed = MechModel.Speed with { X = -1 };
                    IsRaymanMounted = false;
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_UnmountWalkingShell, this);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Position -= new Vector2(0, 16);
                ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.WalkingShell;
                break;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SafetyJumpTimer = 10;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                Timer++;

                if (Rom.Platform == Platform.NGage && IsActionFinished)
                {
                    if (ActionId == Action.EndBoost)
                    {
                        ActionId = Action.Walk;

                        if (IsRaymanMounted)
                            Rayman.ActionId = Rayman.Action.WalkingShell_Ride;
                    }
                    else if (ActionId == Action.Walk)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RocktLeg_Mix03);
                    }
                    else if (ActionId == Action.BeginBoost && IsActionFinished)
                    {
                        ActionId = Action.ShortBoost;

                        if (IsRaymanMounted)
                            Rayman.ActionId = Rayman.Action.WalkingShell_Boost;
                    }
                }

                if (Speed.Y == 0 && 0x1e < Timer)
                {
                    IsRaymanMounted = false;
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_UnmountWalkingShell, this);
                    Explode();
                }

                if (SafetyJumpTimer != 0)
                    SafetyJumpTimer--;

                if (Speed.Y == 0)
                {
                    if (Rom.Platform == Platform.GBA)
                    {
                        State.MoveTo(Fsm_Move);
                        return false;
                    }
                    else if (Rom.Platform == Platform.NGage)
                    {
                        Explode();
                        IsRaymanMounted = false;
                        ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.Default;
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_UnmountWalkingShell, this);
                        State.MoveTo(null);
                        return false;
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }
                }

                if (SafetyJumpTimer != 0 && IsRaymanMounted && JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    State.MoveTo(Fsm_Jump);
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