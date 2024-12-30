using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Rayman
{
    private bool FsmStep_DoOnTheGround()
    {
        if (!FsmStep_CheckDeath())
            return false;

        CheckSlide();
        ManageSlide();

        if (ManageHit())
        {
            State.MoveTo(Fsm_Hit);
            return false;
        }

        if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && SlideType != null)
        {
            State.MoveTo(Fsm_JumpSlide);
            return false;
        }

        if (ShouldAutoJump())
        {
            PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

            State.MoveTo(Fsm_JumpSlide);
            return false;
        }

        return true;
    }

    private bool FsmStep_DoInTheAir()
    {
        if (ManageHit() &&
            (State == Fsm_StopHelico ||
             State == Fsm_Helico ||
             State == Fsm_Jump ||
             State == Fsm_JumpSlide) &&
            ActionId is not (
                Action.Damage_Knockback_Right or
                Action.Damage_Knockback_Left or
                Action.Damage_Shock_Right or
                Action.Damage_Shock_Left))
        {
            ActionId = IsFacingRight ? Action.Damage_Knockback_Right : Action.Damage_Knockback_Left;
        }

        return FsmStep_CheckDeath();
    }

    private bool FsmStep_DoStandingOnPlum()
    {
        if (Engine.Settings.Platform == Platform.GBA)
        {
            CameraSideScroller cam = (CameraSideScroller)Scene.Camera;
            cam.HorizontalOffset = CameraOffset.Center;
        }

        if (field23_0x98 != 0)
            field23_0x98--;

        ManageHit();

        if (HitPoints == 0)
        {
            State.MoveTo(Fsm_Dying);
            return false;
        }

        return true;
    }

    private bool FsmStep_CheckDeath()
    {
        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

        if (Flag1_D)
        {
            field16_0x91++;

            if (field16_0x91 > 60)
            {
                cam.HorizontalOffset = CameraOffset.Default;
                Flag1_D = false;
            }
        }

        if (IsLocalPlayer &&
            State != Fsm_Jump &&
            State != Fsm_BodyShotAttack &&
            State != Fsm_RidingWalkingShell &&
            State != Fsm_EnterLevelCurtain &&
            State != Fsm_LockedLevelCurtain &&
            !IsInFrontOfLevelCurtain)
        {
            Message message;

            if (State != Fsm_SuperHelico &&
                IsDirectionalButtonPressed(GbaInput.Down) &&
                (Speed.Y > 0 || State == Fsm_Crouch) &&
                State != Fsm_Climb)
            {
                CameraTargetY = 70;
                message = Message.Cam_FollowPositionY;
            }
            else if (IsDirectionalButtonPressed(GbaInput.Up) && (State == Fsm_Default || State == Fsm_HangOnEdge))
            {
                CameraTargetY = 160;
                message = Message.Cam_FollowPositionY;
            }
            else if (State == Fsm_Helico && !IsSuperHelicoActive)
            {
                message = Message.Cam_DoNotFollowPositionY;
            }
            else if (State == Fsm_Swing)
            {
                CameraTargetY = 65;
                message = Message.Cam_FollowPositionY;
            }
            else if (State == Fsm_Climb || State == Fsm_SuperHelico)
            {
                CameraTargetY = 112;
                message = Message.Cam_FollowPositionY;
            }
            else
            {
                CameraTargetY = 120;
                message = Message.Cam_FollowPositionY;
            }

            cam.ProcessMessage(this, message, CameraTargetY);
        }

        if (field23_0x98 != 0)
            field23_0x98--;

        if (IsDead())
        {
            if (!RSMultiplayer.IsActive)
                State.MoveTo(Fsm_Dying);
            else
                State.MoveTo(Fsm_MultiplayerDying);

            return false;
        }

        return true;
    }

    public bool Fsm_LevelStart(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // This never actually has a chance to play due to the Init function being called afterwards and overriding this
                ActionId = IsFacingRight ? Action.Spawn_Right : Action.Spawn_Left;
                ChangeAction();

                Timer = 0;

                CameraSideScroller cam = (CameraSideScroller)Scene.Camera;
                if (GameInfo.MapId == MapId.TheCanopy_M2)
                {
                    cam.HorizontalOffset = CameraOffset.Center;
                }
                else
                {
                    if (!RSMultiplayer.IsActive)
                        cam.HorizontalOffset = CameraOffset.Default;
                    else if (IsLocalPlayer)
                        cam.HorizontalOffset = CameraOffset.Multiplayer;
                }

                if (IsLocalPlayer)
                    cam.ProcessMessage(this, Message.Cam_ResetPositionX);

                if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
                    cam.HorizontalOffset = CameraOffset.Center;
                break;

            case FsmAction.Step:
                // Check if we're spawning at a curtain
                if (IsInFrontOfLevelCurtain)
                {
                    // Hide while fading and then show spawn animation
                    if (((FrameWorldSideScroller)Frame.Current).TransitionsFX.IsFadeInFinished)
                    {
                        if (ActionId is not (Action.Spawn_Curtain_Right or Action.Spawn_Curtain_Left))
                            ActionId = IsFacingRight ? Action.Spawn_Curtain_Right : Action.Spawn_Curtain_Left;
                    }
                    else
                    {
                        ActionId = IsFacingRight ? Action.Hidden_Right : Action.Hidden_Left;
                    }
                }

                Timer++;

                if (IsActionFinished && IsBossFight())
                    NextActionId = IsFacingRight ? Action.Idle_Determined_Right : Action.Idle_Determined_Left;

                if (IsActionFinished && (!RSMultiplayer.IsActive || Timer >= 210))
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                CheckSlide();

                if (IsSliding)
                {
                    SlidingOnSlippery();
                }
                else
                {
                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                    if (SlideType == null)
                        PreviousXSpeed = 0;

                    if (!IsBossFight())
                    {
                        if (NextActionId == null)
                        {
                            // Randomly show Rayman being bored
                            if (Random.GetNumber(11) < 6)
                                ActionId = IsFacingRight ? Action.Idle_Bored_Right : Action.Idle_Bored_Left;
                            else
                                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                        }
                        else
                        {
                            ActionId = NextActionId.Value;
                        }
                    }
                    else if (NextActionId is Action.Idle_Determined_Right or Action.Idle_Determined_Left)
                    {
                        ActionId = NextActionId.Value;
                    }
                    else
                    {
                        ActionId = IsFacingRight ? Action.Idle_ReadyToFight_Right : Action.Idle_ReadyToFight_Left;
                    }
                }

                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                Timer++;

                // Look up when pressing up
                if (IsDirectionalButtonPressed(GbaInput.Up))
                {
                    if (ActionId is not (Action.LookUp_Right or Action.LookUp_Left) && SlideType == null)
                    {
                        // TODO: If you look up while the grimace sfx is playing then it loop. This is a bug from the original game. Fix?
                        ActionId = IsFacingRight ? Action.LookUp_Right : Action.LookUp_Left;
                        NextActionId = null;
                    }
                }
                else
                {
                    if (ActionId is Action.LookUp_Right or Action.LookUp_Left)
                        ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                }

                // Play idle animation
                if (IsActionFinished && 
                    (ActionId == NextActionId && ActionId is not (
                        Action.Idle_Bored_Right or Action.Idle_Bored_Left or
                        Action.Idle_LookAround_Right or Action.Idle_LookAround_Left)) &&
                    (ActionId is not (
                         Action.Idle_BasketBall_Right or Action.Idle_BasketBall_Left or
                         Action.Idle_Grimace_Right or Action.Idle_Grimace_Left) ||
                     Timer > 180))
                {
                    if (!IsBossFight())
                        ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                    else
                        ActionId = IsFacingRight ? Action.Idle_ReadyToFight_Right : Action.Idle_ReadyToFight_Left;

                    NextActionId = null;

                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                }

                if (IsSliding)
                {
                    SlidingOnSlippery();
                }
                else
                {
                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                    if (NextActionId != null && NextActionId != ActionId)
                    {
                        ActionId = NextActionId.Value;
                    }
                    else if (ActionId is not (
                                 Action.LookUp_Right or Action.LookUp_Left or
                                 Action.Idle_Left or Action.Idle_Right or
                                 Action.Idle_ReadyToFight_Right or Action.Idle_ReadyToFight_Left) &&
                             ActionId != NextActionId)
                    {
                        if (!IsBossFight())
                            ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                        else
                            ActionId = IsFacingRight ? Action.Idle_ReadyToFight_Right : Action.Idle_ReadyToFight_Left;
                    }
                }

                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.Walk_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.Walk_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                // Jump
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && CanJump)
                {
                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Crouch
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    NextActionId = IsFacingRight ? Action.CrouchDown_Right : Action.CrouchDown_Left;
                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                    State.MoveTo(Fsm_Crouch);
                    return false;
                }

                // Fall
                if (Speed.Y > 1)
                {
                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Walk
                if (IsDirectionalButtonPressed(GbaInput.Left) || IsDirectionalButtonPressed(GbaInput.Right))
                {
                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                    State.MoveTo(Fsm_Walk);
                    return false;
                }

                // Punch
                if (field23_0x98 == 0 && MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B) && CanAttackWithFist(2))
                {
                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                // Walking off edge
                if (PreviousXSpeed != 0 && IsNearEdge() != 0 && !Flag1_1)
                {
                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                    Position += new Vector2(PreviousXSpeed < 0 ? -16 : 16, 0);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Standing near edge
                if (PreviousXSpeed == 0 && IsNearEdge() != 0 && !Flag1_1)
                {
                    PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);
                    State.MoveTo(Fsm_StandingNearEdge);
                    return false;
                }

                // Restart default state
                if ((IsActionFinished && ActionId is not (
                         Action.LookUp_Right or Action.LookUp_Left or
                         Action.Idle_Bored_Right or Action.Idle_Bored_Left or
                         Action.Idle_LookAround_Right or Action.Idle_LookAround_Left) &&
                     360 < Timer) ||
                    (IsActionFinished && ActionId is
                         Action.Idle_Bored_Right or Action.Idle_Bored_Left or
                         Action.Idle_LookAround_Right or Action.Idle_LookAround_Left &&
                     720 < Timer))
                {
                    SetRandomIdleAction();
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                Flag1_1 = false;
                break;

            case FsmAction.UnInit:
                if (ActionId is Action.Idle_SpinBody_Right or Action.Idle_SpinBody_Left)
                    PlaySound(Rayman3SoundEvent.Stop__RaySpin_Mix06);

                if (ActionId == NextActionId || ActionId is Action.Walk_Right or Action.Walk_Left or Action.WalkFast_Right or Action.WalkFast_Left)
                    NextActionId = null;

                if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
                {
                    CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

                    if (cam.HorizontalOffset == CameraOffset.Center)
                        cam.HorizontalOffset = CameraOffset.Default;
                }
                break;
        }

        return true;
    }

    public bool Fsm_StandingNearEdge(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__OnoEquil_Mix03))
                    PlaySound(Rayman3SoundEvent.Play__OnoEquil_Mix03);

                Timer = 120;
                NextActionId = null;

                Box detectionBox = GetDetectionBox();
                PhysicalType rightType = Scene.GetPhysicalType(new Vector2(detectionBox.MaxX, detectionBox.MaxY));

                if (rightType.IsSolid)
                    ActionId = IsFacingRight ? Action.NearEdgeBehind_Right : Action.NearEdgeFront_Left;
                else
                    ActionId = IsFacingRight ? Action.NearEdgeFront_Right : Action.NearEdgeBehind_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                Timer--;

                // Play sound every 2 seconds
                if (Timer == 0)
                {
                    Timer = 120;

                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__OnoEquil_Mix03))
                        PlaySound(Rayman3SoundEvent.Play__OnoEquil_Mix03);
                }

                // Change direction
                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.Walk_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.Walk_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                // Walk
                if (IsDirectionalButtonPressed(GbaInput.Left) || IsDirectionalButtonPressed(GbaInput.Right))
                {
                    State.MoveTo(Fsm_Walk);
                    return false;
                }

                // Crouch
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    State.MoveTo(Fsm_Crouch);
                    return false;
                }

                // Jump
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A))
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Fall
                if (Speed.Y > 1)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Punch
                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B) && CanAttackWithFist(2))
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                // Default if no longer near edge
                if (IsNearEdge() == 0)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Walk(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (IsSliding)
                {
                    SlidingOnSlippery();
                }
                else
                {
                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                    if (SlideType == null)
                        PreviousXSpeed = 0;

                    if (!RSMultiplayer.IsActive)
                    {
                        // Randomly look around for Globox in the first level
                        if (GameInfo.MapId == MapId.WoodLight_M1 && GameInfo.LastGreenLumAlive == 0)
                        {
                            if (Random.GetNumber(501) >= 401)
                                ActionId = IsFacingRight ? Action.Walk_LookAround_Right : Action.Walk_LookAround_Left;
                            else
                                ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;

                            field22_0x97 = 0;
                        }
                        else
                        {
                            ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                        }
                    }
                    else
                    {
                        UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;
                        int tagId = userInfo.GetTagId();

                        if ((MultiplayerInfo.GameType == MultiplayerGameType.RayTag && InstanceId == tagId) ||
                            (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && InstanceId != tagId) ||
                            (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && MultiplayerData.field_00 == null))
                        {
                            ActionId = IsFacingRight ? Action.WalkFast_Right : Action.WalkFast_Left;
                        }
                        else
                        {
                            ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                        }

                        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
                            MultiplayerData.field_b9 = 1;
                    }
                }

                Timer = 0;
                Charge = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                if (Speed.Y > 1 && PreviousXSpeed == 0)
                {
                    Timer++;
                    Position -= new Vector2(0, Speed.Y);
                }

                // Randomly look around for Globox in the first level
                if (GameInfo.MapId == MapId.WoodLight_M1 && GameInfo.LastGreenLumAlive == 0)
                {
                    field22_0x97++;

                    if (IsActionFinished)
                    {
                        if (ActionId is Action.Walk_Right or Action.Walk_Left &&
                            field22_0x97 > Random.GetNumber(121) + 120)
                        {
                            ActionId = IsFacingRight ? Action.Walk_LookAround_Right : Action.Walk_LookAround_Left;
                            field22_0x97 = 0;
                        }
                        else if (ActionId is Action.Walk_LookAround_Right or Action.Walk_LookAround_Left && 
                                 field22_0x97 > Random.GetNumber(121) + 60)
                        {
                            ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                            field22_0x97 = 0;
                        }
                    }
                }

                // Change walking direction
                if (ActionId is Action.Walk_LookAround_Right or Action.Walk_LookAround_Left)
                {
                    if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                    {
                        ActionId = Action.Walk_LookAround_Left;
                        ChangeAction();

                        if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                            MultiplayerData.field_b9 = 1;
                    }
                    else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                    {
                        ActionId = Action.Walk_LookAround_Right;
                        ChangeAction();

                        if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                            MultiplayerData.field_b9 = 1;
                    }
                }
                else
                {
                    if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                    {
                        ActionId = Action.Walk_Left;
                        ChangeAction();

                        if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                            MultiplayerData.field_b9 = 1;
                    }
                    else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                    {
                        ActionId = Action.Walk_Right;
                        ChangeAction();

                        if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                            MultiplayerData.field_b9 = 1;
                    }
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B))
                {
                    Charge++;
                }
                else if (MultiJoyPad.IsButtonJustReleased(InstanceId, GbaInput.B) && field23_0x98 == 0)
                {
                    Charge = 0;

                    if (CanAttackWithFist(1))
                    {
                        Attack(0, RaymanBody.RaymanBodyPartType.Fist, new Vector2(16, -16), false);
                        field23_0x98 = 0;
                    }
                    else if (CanAttackWithFist(2))
                    {
                        Attack(0, RaymanBody.RaymanBodyPartType.SecondFist, new Vector2(16, -16), false);

                        if ((GameInfo.Powers & Power.DoubleFist) == 0)
                            field23_0x98 = 0;
                    }
                }

                if (IsSliding)
                {
                    SlidingOnSlippery();
                }
                else
                {
                    if (SlideType != null && 
                        ActionId is
                            Action.Walk_Right or Action.Walk_Left or
                            Action.Walk_LookAround_Right or Action.Walk_LookAround_Left &&
                        AnimatedObject.CurrentFrame is 2 or 10 &&
                        !AnimatedObject.IsDelayMode)
                    {
                        PlaySound(Rayman3SoundEvent.Play__PlumSnd2_Mix03);
                    }

                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                    if (!RSMultiplayer.IsActive)
                    {
                        if (ActionId is not (
                            Action.Walk_Right or Action.Walk_Left or
                            Action.Walk_LookAround_Right or Action.Walk_LookAround_Left))
                        {
                            if (GameInfo.MapId == MapId.WoodLight_M1 && GameInfo.LastGreenLumAlive == 0)
                                ActionId = IsFacingRight ? Action.Walk_LookAround_Right : Action.Walk_LookAround_Left;
                            else
                                ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                        }
                    }
                    else
                    {
                        if (ActionId is not (
                            Action.Walk_Right or Action.Walk_Left or
                            Action.WalkFast_Right or Action.WalkFast_Left))
                        {
                            UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;
                            int tagId = userInfo.GetTagId();

                            if ((MultiplayerInfo.GameType == MultiplayerGameType.RayTag && InstanceId == tagId) ||
                                (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && InstanceId != tagId) ||
                                (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && MultiplayerData.field_00 == null))
                            {
                                ActionId = IsFacingRight ? Action.WalkFast_Right : Action.WalkFast_Left;
                            }
                            else
                            {
                                ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                            }

                            if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
                                MultiplayerData.field_b9 = 1;
                        }
                    }
                }

                // Return if released the left or right inputs
                if (IsDirectionalButtonButtonReleased(GbaInput.Left) && IsDirectionalButtonButtonReleased(GbaInput.Right) &&
                    ActionId is
                        Action.Walk_Right or Action.Walk_Left or
                        Action.WalkFast_Right or Action.WalkFast_Left)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Return and shout for Globox if looking for him when released the left and right inputs
                if (IsDirectionalButtonButtonReleased(GbaInput.Left) && IsDirectionalButtonButtonReleased(GbaInput.Right) &&
                    ActionId is Action.Walk_LookAround_Right or Action.Walk_LookAround_Left)
                {
                    NextActionId = IsFacingRight ? Action.Idle_Shout_Right : Action.Idle_Shout_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Crawl
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    State.MoveTo(Fsm_Crawl);
                    return false;
                }

                // Jump
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A))
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Fall
                if (PreviousXSpeed != 0 && Speed.Y > 1)
                {
                    Position += new Vector2(IsFacingLeft ? -16 : 16, 0);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Fall
                if (Speed.Y > 1 && Timer >= 8)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Charge punch
                if (field23_0x98 == 0 && Charge > 10 && IsDirectionalButtonPressed(GbaInput.B) && CanAttackWithFist(2))
                {
                    State.MoveTo(Fsm_Attack);
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
        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                if (ActionId is not (Action.BouncyJump_Right or Action.BouncyJump_Left))
                {
                    ActionId = IsFacingRight ? Action.Jump_Right : Action.Jump_Left;
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                }

                NextActionId = null;
                CameraTargetY = 70;

                if (IsLocalPlayer)
                    cam.ProcessMessage(this, Message.Cam_DoNotFollowPositionY, CameraTargetY);

                Timer = GameTime.ElapsedFrames;
                SlideType = null;
                LinkedMovementActor = null;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                float speedY = Speed.Y;

                if (IsLocalPlayer)
                    cam.ProcessMessage(this, Message.Cam_DoNotFollowPositionY, 130);

                if (ActionId is Action.Jump_Right or Action.Jump_Left &&
                    MultiJoyPad.IsButtonReleased(InstanceId, GbaInput.A) && 
                    MechModel.Speed.Y < -4 && 
                    !Flag2_0)
                {
                    MechModel.Speed = MechModel.Speed with { Y = -4 };
                    Flag2_0 = true;
                }

                if (Speed.Y == 0 && MechModel.Speed.Y < 0)
                    MechModel.Speed = MechModel.Speed with { Y = 0 };

                MoveInTheAir(PreviousXSpeed);
                SlowdownAirSpeed();
                AttackInTheAir();

                if (HasLanded())
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (GameTime.ElapsedFrames - Timer > 50)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                if (IsNearHangableEdge())
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && !IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_Helico);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }

                if (IsOnHangable())
                {
                    BeginHang();
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                if (GameTime.ElapsedFrames - Timer > 10 && IsOnClimbableVertical() == 1)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && IsOnWallJumpable())
                {
                    BeginWallJump();
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }

                if (speedY < 4 && MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.R) && HasPower(Power.BodyShot) && CanAttackWithBody())
                {
                    State.MoveTo(Fsm_BodyShotAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Flag2_0 = false;
                break;
        }

        return true;
    }

    public bool Fsm_JumpSlide(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Stop__SldGreen_SkiLoop1);
                ActionId = IsFacingRight ? Action.Sliding_Jump_Right : Action.Sliding_Jump_Left;
                NextActionId = null;
                Flag2_0 = false;
                Timer = GameTime.ElapsedFrames;
                SlideType = null;
                PreviousXSpeed /= 2;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                float speedY = Speed.Y;

                AttackInTheAir();
                MoveInTheAir(PreviousXSpeed);

                // Land
                if (HasLanded())
                {
                    PreviousXSpeed = 0;
                    NextActionId = IsFacingRight ? Action.Sliding_Land_Right : Action.Sliding_Land_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Fall
                if (speedY > 3.4375)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Hang on edge
                if (IsNearHangableEdge())
                {
                    PreviousXSpeed = 0;
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                // Helico
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && !IsSuperHelicoActive && GameTime.ElapsedFrames - Timer >= 6)
                {
                    State.MoveTo(Fsm_Helico);
                    return false;
                }

                // Super helico
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && IsSuperHelicoActive && GameTime.ElapsedFrames - Timer >= 6)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }

                // Hang
                if (GameTime.ElapsedFrames - Timer >= 11 && IsOnHangable())
                {
                    PreviousXSpeed = 0;
                    BeginHang();
                    State.MoveTo(Fsm_Hang);
                }

                if (IsOnClimbableVertical() == 1)
                {
                    PreviousXSpeed = 0;
                    State.MoveTo(Fsm_Climb);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Flag2_0 = false;
                break;
        }

        return true;
    }

    public bool Fsm_HangOnEdge(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Play__HandTap1_Mix04);
                PreviousXSpeed = 0;

                if (NextActionId is Action.HangOnEdge_EndAttack_Right or Action.HangOnEdge_EndAttack_Left)
                    ActionId = IsFacingRight ? Action.HangOnEdge_EndAttack_Right : Action.HangOnEdge_EndAttack_Left;
                else
                    ActionId = IsFacingRight ? Action.HangOnEdge_Begin_Right : Action.HangOnEdge_Begin_Left;

                SetDetectionBox(new Box(
                    minX: ActorModel.DetectionBox.MinX,
                    minY: ActorModel.DetectionBox.MaxY - 16,
                    maxX: ActorModel.DetectionBox.MaxX,
                    maxY: ActorModel.DetectionBox.MaxY + 16));
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (IsActionFinished && ActionId is not (Action.HangOnEdge_Idle_Right or Action.HangOnEdge_Idle_Left))
                {
                    ActionId = IsFacingRight ? Action.HangOnEdge_Idle_Right : Action.HangOnEdge_Idle_Left;
                    NextActionId = null;
                }

                // Move down
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    HangOnEdgeDelay = 30;
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Jump
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A))
                {
                    HangOnEdgeDelay = 30;
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Attack
                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B) && CanAttackWithFoot())
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SetDetectionBox(new Box(ActorModel.DetectionBox));
                break;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);
                ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                NextActionId = null;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                Timer++;

                if (CanSafetyJump && Timer > 15)
                    CanSafetyJump = false;

                MoveInTheAir(PreviousXSpeed);
                SlowdownAirSpeed();
                AttackInTheAir();

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && CanSafetyJump)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && !IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_Helico);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }

                if (IsNearHangableEdge())
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (HasLanded())
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (IsOnHangable())
                {
                    BeginHang();
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                if (IsOnClimbableVertical() == 1)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && IsOnWallJumpable())
                {
                    BeginWallJump();
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                CanSafetyJump = false;
                break;
        }

        return true;
    }

    public bool Fsm_Helico(FsmAction action)
    {
        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Play__Helico01_Mix10);

                if (ActionId is Action.BouncyJump_Right or Action.BouncyJump_Left)
                    ActionId = IsFacingRight ? Action.BouncyHelico_Right : Action.BouncyHelico_Left;
                else
                    ActionId = IsFacingRight ? Action.Helico_Right : Action.Helico_Left;

                NextActionId = null;
                Timer = GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                AttackInTheAir();
                SlowdownAirSpeed();
                MoveInTheAir(PreviousXSpeed);

                if (IsNearHangableEdge())
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (HasLanded())
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) || MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B))
                {
                    State.MoveTo(Fsm_StopHelico);
                    return false;
                }

                if (GameTime.ElapsedFrames - Timer > 40)
                {
                    State.MoveTo(Fsm_TimeoutHelico);
                    return false;
                }

                if (IsOnHangable())
                {
                    BeginHang();
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                if (IsOnClimbableVertical() == 1)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && IsOnWallJumpable())
                {
                    BeginWallJump();
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.R) && HasPower(Power.BodyShot) && CanAttackWithBody())
                {
                    State.MoveTo(Fsm_BodyShotAttack);
                    return false;
                }

                if (IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                PreviousXSpeed = 0;
                
                if (IsLocalPlayer)
                    cam.ProcessMessage(this, Message.Cam_ResetPositionX);

                PlaySound(Rayman3SoundEvent.Stop__Helico01_Mix10);

                if (GameTime.ElapsedFrames - Timer <= 40)
                    PlaySound(Rayman3SoundEvent.Play__HeliCut_Mix01);
                break;
        }

        return true;
    }

    public bool Fsm_SuperHelico(FsmAction action)
    {
        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Play__Helico01_Mix10);
                NextActionId = null;

                if (ActionId is Action.BouncyJump_Right or Action.BouncyJump_Left)
                    ActionId = IsFacingRight ? Action.BouncyHelico_Right : Action.BouncyHelico_Left;
                else
                    ActionId = IsFacingRight ? Action.Helico_Right : Action.Helico_Left;

                Timer = GameTime.ElapsedFrames;

                if (IsSuperHelicoActive)
                {
                    cam.HorizontalOffset = CameraOffset.Center;
                    PreviousXSpeed = 0;
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                // Begin charging fist
                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B) &&
                    CanAttackWithFist(1) &&
                    ActionId is not (
                        Action.SuperHelico_BeginThrowFist_Right or Action.SuperHelico_BeginThrowFist_Left or
                        Action.SuperHelico_ChargeFist_Right or Action.SuperHelico_ChargeFist_Left or
                        Action.SuperHelico_ChargeSuperFist_Right or Action.SuperHelico_ChargeSuperFist_Left))
                {
                    ActionId = IsFacingRight ? Action.SuperHelico_BeginThrowFist_Right : Action.SuperHelico_BeginThrowFist_Left;
                    ChangeAction();
                    Timer = 0;
                }

                // Charge fist
                if (IsActionFinished && ActionId is Action.SuperHelico_BeginThrowFist_Right or Action.SuperHelico_BeginThrowFist_Left)
                {
                    ActionId = IsFacingRight ? Action.SuperHelico_ChargeFist_Right : Action.SuperHelico_ChargeFist_Left;
                    Timer = GameTime.ElapsedFrames;
                }

                // Charge super fist
                if (HasPower(Power.SuperFist) &&
                    GameTime.ElapsedFrames - Timer > 20 &&
                    ActionId is Action.SuperHelico_ChargeFist_Right or Action.SuperHelico_ChargeFist_Left)
                {
                    ActionId = IsFacingRight ? Action.SuperHelico_ChargeSuperFist_Right : Action.SuperHelico_ChargeSuperFist_Left;
                    PlaySound(Rayman3SoundEvent.Stop__Charge_Mix05);
                    PlaySound(Rayman3SoundEvent.Play__Charge2_Mix04);
                }

                // Punch
                if (MultiJoyPad.IsButtonReleased(InstanceId, GbaInput.B) &&
                    CanAttackWithFist(1) &&
                    ActionId is
                        Action.SuperHelico_BeginThrowFist_Right or Action.SuperHelico_BeginThrowFist_Left or
                        Action.SuperHelico_ChargeFist_Right or Action.SuperHelico_ChargeFist_Left or
                        Action.SuperHelico_ChargeSuperFist_Right or Action.SuperHelico_ChargeSuperFist_Left)
                {
                    if (Timer == 0)
                        Timer = GameTime.ElapsedFrames;

                    if (ActionId is Action.SuperHelico_ChargeSuperFist_Right or Action.SuperHelico_ChargeSuperFist_Left)
                    {
                        Attack(GameTime.ElapsedFrames - Timer, RaymanBody.RaymanBodyPartType.SuperFist, new Vector2(16, -16), true);
                        PlaySound(Rayman3SoundEvent.Stop__Charge2_Mix04);
                    }
                    else
                    {
                        Attack(GameTime.ElapsedFrames - Timer, RaymanBody.RaymanBodyPartType.Fist, new Vector2(16, -16), false);
                    }

                    ActionId = IsFacingRight ? Action.SuperHelico_EndChargeFist_Right : Action.SuperHelico_EndChargeFist_Left;
                    ChangeAction();
                }

                // Finish punch
                if (IsActionFinished && ActionId is Action.SuperHelico_EndChargeFist_Right or Action.SuperHelico_EndChargeFist_Left)
                    ActionId = IsFacingRight ? Action.Helico_Right : Action.Helico_Left;

                // Move left
                if (IsDirectionalButtonPressed(GbaInput.Left))
                {
                    if (IsDirectionalButtonJustPressed(GbaInput.Left))
                        PreviousXSpeed = MathHelpers.FromFixedPoint(0x1cccc);
                    else if (PreviousXSpeed <= 0)
                        PreviousXSpeed = 0;
                    else
                        PreviousXSpeed -= 0.25f;

                    if (IsFacingRight)
                        AnimatedObject.FlipX = true;

                    MechModel.Speed = MechModel.Speed with { X = PreviousXSpeed - MathHelpers.FromFixedPoint(0x1cccc) };
                }
                // Move right
                else if (IsDirectionalButtonPressed(GbaInput.Right))
                {
                    if (IsDirectionalButtonJustPressed(GbaInput.Right))
                        PreviousXSpeed = -MathHelpers.FromFixedPoint(0x1cccc);
                    else if (PreviousXSpeed >= 0)
                        PreviousXSpeed = 0;
                    else
                        PreviousXSpeed += 0.25f;

                    if (IsFacingLeft)
                        AnimatedObject.FlipX = false;

                    MechModel.Speed = MechModel.Speed with { X = PreviousXSpeed + MathHelpers.FromFixedPoint(0x1cccc) };
                }
                // Gradually slow down horizontal movement
                else
                {
                    if (PreviousXSpeed == 0 && Speed.X != 0)
                        PreviousXSpeed = Speed.X;

                    if (Speed.X == 0 || 
                        Speed.X > 0 && PreviousXSpeed < 0 || 
                        Speed.X < 0 && PreviousXSpeed > 0)
                    {
                        PreviousXSpeed = 0;
                    }
                    else if (PreviousXSpeed <= 0)
                    {
                        PreviousXSpeed += MathHelpers.FromFixedPoint(0xf00);
                        if (PreviousXSpeed > 0)
                            PreviousXSpeed = 0;
                    }
                    else
                    {
                        PreviousXSpeed -= MathHelpers.FromFixedPoint(0xf00);
                        if (PreviousXSpeed < 0)
                            PreviousXSpeed = 0;
                    }
                    
                    MechModel.Speed = MechModel.Speed with { X = PreviousXSpeed };
                }

                // Move up
                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.A))
                {
                    MechModel.Speed = MechModel.Speed with { Y = -1 };
                }
                // Fall down
                else if (MultiJoyPad.IsButtonReleased(InstanceId, GbaInput.A))
                {
                    if (MechModel.Speed.Y >= 1)
                        MechModel.Speed = MechModel.Speed with { Y = 1 };
                    else
                        MechModel.Speed += new Vector2(0, 0.25f);
                }

                // Super helico ending
                if (IsSuperHelicoActive && 
                    MultiplayerBlueLumTimer != 1299 &&
                    ActionId is not (
                        Action.SuperHelico_BeginThrowFist_Right or Action.SuperHelico_BeginThrowFist_Left or
                        Action.SuperHelico_ChargeFist_Right or Action.SuperHelico_ChargeFist_Left or
                        Action.SuperHelico_ChargeSuperFist_Right or Action.SuperHelico_ChargeSuperFist_Left or
                        Action.SuperHelico_EndChargeFist_Right or Action.SuperHelico_EndChargeFist_Left))
                {
                    int timer = RSMultiplayer.IsActive ? MultiplayerBlueLumTimer : GameInfo.BlueLumTimer;

                    if (timer < 79 &&
                        ActionId is not (Action.HelicoTimeout_Right or Action.HelicoTimeout_Left))
                    {
                        ActionId = IsFacingRight ? Action.HelicoTimeout_Right : Action.HelicoTimeout_Left;
                    }
                    else if (timer >= 79 &&
                             ActionId is Action.HelicoTimeout_Right or Action.HelicoTimeout_Left)
                    {
                        ActionId = IsFacingRight ? Action.Helico_Right : Action.Helico_Left;
                    }
                }

                if (IsNearHangableEdge())
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (HasLanded())
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (!IsSuperHelicoActive)
                {
                    PlaySound(Rayman3SoundEvent.Play__Tag_Mix02);
                    State.MoveTo(Fsm_StopHelico);
                    return false;
                }

                if (IsOnHangable())
                {
                    BeginHang();
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                if (IsOnClimbableVertical() == 1)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && IsOnWallJumpable())
                {
                    BeginWallJump();
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                PreviousXSpeed = 0;
                cam.HorizontalOffset = CameraOffset.Default;
                PlaySound(Rayman3SoundEvent.Stop__Helico01_Mix10);
                PlaySound(Rayman3SoundEvent.Stop__Charge_Mix05);
                PlaySound(Rayman3SoundEvent.Stop__Charge2_Mix04);
                PlaySound(Rayman3SoundEvent.Play__HeliStop_Mix06);
                break;
        }

        return true;
    }

    public bool Fsm_StopHelico(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                MoveInTheAir(PreviousXSpeed);
                AttackInTheAir();

                if (IsNearHangableEdge())
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (HasLanded())
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }

                if (IsOnClimbableVertical() == 1)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && IsOnWallJumpable())
                {
                    BeginWallJump();
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_TimeoutHelico(FsmAction action)
    {
        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = IsFacingRight ? Action.HelicoTimeout_Right : Action.HelicoTimeout_Left;
                Timer = 0;
                PlaySound(Rayman3SoundEvent.Play__HeliStop_Mix06);
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                Timer++;
                AttackInTheAir();
                SlowdownAirSpeed();
                MoveInTheAir(PreviousXSpeed);

                if (IsNearHangableEdge())
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (HasLanded())
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    PlaySound(Rayman3SoundEvent.Play__HeliCut_Mix01);
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) || MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B) || Timer > 50)
                {
                    State.MoveTo(Fsm_StopHelico);
                    return false;
                }

                if (IsOnHangable())
                {
                    BeginHang();
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                if (IsOnClimbableVertical() == 1)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && IsOnWallJumpable())
                {
                    BeginWallJump();
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }

                if (IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                PlaySound(Rayman3SoundEvent.Stop__HeliStop_Mix06);
                PreviousXSpeed = 0;

                if (IsLocalPlayer)
                    cam.ProcessMessage(this, Message.Cam_ResetPositionX);

                if (Timer > 50)
                {
                    Vector2 pos = Position;

                    pos += new Vector2(0, Tile.Size);
                    if (Scene.GetPhysicalType(pos) != PhysicalTypeValue.None)
                        break;

                    pos += new Vector2(0, Tile.Size);
                    if (Scene.GetPhysicalType(pos) != PhysicalTypeValue.None)
                        break;

                    pos += new Vector2(0, Tile.Size);
                    if (Scene.GetPhysicalType(pos) != PhysicalTypeValue.None)
                        break;

                    pos += new Vector2(0, Tile.Size);
                    if (Scene.GetPhysicalType(pos) != PhysicalTypeValue.None)
                        break;

                    PlaySound(Rayman3SoundEvent.Play__OnoPeur1_Mix03);
                }
                break;
        }

        return true;
    }

    public bool Fsm_Crouch(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (IsSliding)
                {
                    ActionId = IsFacingRight ? Action.Sliding_Crouch_Right : Action.Sliding_Crouch_Left;
                }
                else
                {
                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);
                    
                    if (SlideType == null)
                        PreviousXSpeed = 0;

                    if (NextActionId == null)
                        ActionId = IsFacingRight ? Action.Crouch_Right : Action.Crouch_Left;
                    else
                        ActionId = NextActionId.Value;
                }

                // Custom detection box for when crouching
                SetDetectionBox(new Box(
                    minX: ActorModel.DetectionBox.MinX,
                    minY: ActorModel.DetectionBox.MaxY - 16,
                    maxX: ActorModel.DetectionBox.MaxX,
                    maxY: ActorModel.DetectionBox.MaxY));
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                if (IsActionFinished && ActionId is Action.CrouchDown_Right or Action.CrouchDown_Left)
                {
                    ActionId = IsFacingRight ? Action.Crouch_Right : Action.Crouch_Left;
                    NextActionId = null;
                }

                if (IsSliding)
                {
                    SlidingOnSlippery();
                }
                else
                {
                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                    if (ActionId is not (Action.Crouch_Right or Action.Crouch_Left or Action.CrouchDown_Right or Action.CrouchDown_Left))
                        ActionId = IsFacingRight ? Action.Crouch_Right : Action.Crouch_Left;
                }

                Box detectionBox = GetDetectionBox();

                PhysicalType topType = Scene.GetPhysicalType(detectionBox.TopLeft + new Vector2(1, -Tile.Size));

                if (!topType.IsSolid)
                    topType = Scene.GetPhysicalType(detectionBox.TopRight + new Vector2(-1, -Tile.Size));

                // Change direction
                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.Crawl_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.Crawl_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                // Let go of down and stop crouching
                if (IsDirectionalButtonButtonReleased(GbaInput.Down) && !topType.IsSolid)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Crawl
                if (IsDirectionalButtonPressed(GbaInput.Left) || IsDirectionalButtonPressed(GbaInput.Right))
                {
                    State.MoveTo(Fsm_Crawl);
                    return false;
                }

                // Fall
                if (Speed.Y > 1)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Jump
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && !topType.IsSolid && CanJump)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Restore detection box
                SetDetectionBox(new Box(ActorModel.DetectionBox));

                if (NextActionId == ActionId || ActionId is Action.Crawl_Right or Action.Crawl_Left)
                    NextActionId = null;
                break;
        }

        return true;
    }

    public bool Fsm_Crawl(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (IsSliding)
                {
                    ActionId = IsFacingRight ? Action.Sliding_Crouch_Right : Action.Sliding_Crouch_Left;
                }
                else
                {
                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                    if (SlideType == null)
                        PreviousXSpeed = 0;

                    if (NextActionId == null)
                        ActionId = IsFacingRight ? Action.Crawl_Right : Action.Crawl_Left;
                    else
                        ActionId = NextActionId.Value;
                }

                NextActionId = null;

                // Custom detection box for when crouching
                SetDetectionBox(new Box(
                    minX: ActorModel.DetectionBox.MinX,
                    minY: ActorModel.DetectionBox.MaxY - 16,
                    maxX: ActorModel.DetectionBox.MaxX,
                    maxY: ActorModel.DetectionBox.MaxY));
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                Box detectionBox = GetDetectionBox();

                PhysicalType topType = Scene.GetPhysicalType(detectionBox.TopLeft + new Vector2(1, -Tile.Size));

                if (!topType.IsSolid)
                    topType = Scene.GetPhysicalType(detectionBox.TopRight + new Vector2(-1, -Tile.Size));

                // Change direction
                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.Crawl_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.Crawl_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                if (IsSliding)
                {
                    SlidingOnSlippery();
                }
                else
                {
                    PlaySound(Rayman3SoundEvent.Stop__SkiLoop1);

                    if (ActionId is not (Action.Crawl_Right or Action.Crawl_Left))
                        ActionId = IsFacingRight ? Action.Crawl_Right : Action.Crawl_Left;
                }

                // Walk
                if (IsDirectionalButtonButtonReleased(GbaInput.Down) && (IsDirectionalButtonPressed(GbaInput.Left) || IsDirectionalButtonPressed(GbaInput.Right)) && !topType.IsSolid)
                {
                    State.MoveTo(Fsm_Walk);
                    return false;
                }

                // Stopped crouching/crawling
                if (IsDirectionalButtonButtonReleased(GbaInput.Down) && IsDirectionalButtonButtonReleased(GbaInput.Left) && IsDirectionalButtonButtonReleased(GbaInput.Right) && !topType.IsSolid)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Crouch
                if (IsDirectionalButtonButtonReleased(GbaInput.Right) && IsDirectionalButtonButtonReleased(GbaInput.Left))
                {
                    State.MoveTo(Fsm_Crouch);
                    return false;
                }

                // Jump
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && !topType.IsSolid)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Fall
                if (Speed.Y > 1)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Restore detection box
                SetDetectionBox(new Box(ActorModel.DetectionBox));
                break;
        }

        return true;
    }

    public bool Fsm_Attack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PreviousXSpeed = 0;
                NextActionId = null;

                // Climb
                if (ActionId is Action.Climb_BeginChargeFist_Right or Action.Climb_BeginChargeFist_Left)
                {
                    Timer = 0;
                }
                // Hang
                else if (ActionId is
                         Action.Hang_Move_Right or Action.Hang_Move_Left or
                         Action.Hang_Idle_Right or Action.Hang_Idle_Left or
                         Action.Hang_Attack_Right or Action.Hang_Attack_Left or
                         Action.Hang_EndMove_Right or Action.Hang_EndMove_Left)
                {
                    // Probably a bug in the GBA code since this causes the sound to play twice. This was fixed for N-Gage.
                    if (Engine.Settings.Platform == Platform.GBA)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Charge_Mix05);

                    ActionId = IsFacingRight ? Action.Hang_ChargeAttack_Right : Action.Hang_ChargeAttack_Left;
                    Timer = GameTime.ElapsedFrames;
                }
                // Hang on edge
                else if (ActionId is 
                         Action.HangOnEdge_Begin_Right or Action.HangOnEdge_Begin_Left or
                         Action.HangOnEdge_Idle_Right or Action.HangOnEdge_Idle_Left or
                         Action.HangOnEdge_EndAttack_Right or Action.HangOnEdge_EndAttack_Left)
                {
                    ActionId = IsFacingRight ? Action.HangOnEdge_BeginAttack_Right : Action.HangOnEdge_BeginAttack_Left;
                    Timer = 0;
                }
                // Normal fist attack
                else if (CanAttackWithFist(1))
                {
                    ActionId = IsFacingRight ? Action.BeginChargeFist_Right : Action.BeginChargeFist_Left;
                    Timer = 0;
                }
                // Second normal fist attack
                else
                {
                    ActionId = IsFacingRight ? Action.BeginChargeSecondFist_Right : Action.BeginChargeSecondFist_Left;
                    Timer = 0;
                }

                PlaySound(Rayman3SoundEvent.Play__Charge_Mix05);
                break;

            case FsmAction.Step:
                // Check for damage
                if (AttachedObject?.Type == (int)ActorType.Plum)
                {
                    if (!FsmStep_DoStandingOnPlum())
                        return false;

                    if (Engine.Settings.Platform == Platform.NGage)
                    {
                        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

                        if (IsFacingRight)
                            cam.HorizontalOffset = Speed.X < 0 ? CameraOffset.DefaultReversed : CameraOffset.Default;
                        else
                            cam.HorizontalOffset = Speed.X < 0 ? CameraOffset.Default : CameraOffset.DefaultReversed;
                    }
                }
                else
                {
                    if (!FsmStep_DoOnTheGround())
                        return false;
                }

                // Change direction (if not hanging on an edge)
                if (ActionId is not (
                    Action.HangOnEdge_ChargeAttack_Right or Action.HangOnEdge_ChargeAttack_Left or
                    Action.HangOnEdge_BeginAttack_Right or Action.HangOnEdge_BeginAttack_Left))
                {
                    if (IsDirectionalButtonPressed(GbaInput.Left))
                    {
                        if (IsFacingRight)
                            AnimatedObject.FlipX = true;
                    }
                    else if (IsDirectionalButtonPressed(GbaInput.Right))
                    {
                        if (IsFacingLeft)
                            AnimatedObject.FlipX = false;
                    }
                }

                // Update action
                if (IsActionFinished && Timer == 0)
                {
                    if (ActionId is Action.BeginChargeFist_Right or Action.BeginChargeFist_Left)
                    {
                        ActionId = IsFacingRight ? Action.ChargeFist_Right : Action.ChargeFist_Left;
                        Timer = GameTime.ElapsedFrames;
                    }
                    else if (ActionId is Action.BeginChargeSecondFist_Right or Action.BeginChargeSecondFist_Left)
                    {
                        ActionId = IsFacingRight ? Action.ChargeSecondFist_Right : Action.ChargeSecondFist_Left;
                        Timer = GameTime.ElapsedFrames;
                    }
                    else if (ActionId is Action.Climb_BeginChargeFist_Right or Action.Climb_BeginChargeFist_Left)
                    {
                        ActionId = IsFacingRight ? Action.Climb_ChargeFist_Right : Action.Climb_ChargeFist_Left;
                        Timer = GameTime.ElapsedFrames;
                    }
                    else if (ActionId is Action.HangOnEdge_BeginAttack_Right or Action.HangOnEdge_BeginAttack_Left)
                    {
                        ActionId = IsFacingRight ? Action.HangOnEdge_ChargeAttack_Right : Action.HangOnEdge_ChargeAttack_Left;
                        Timer = GameTime.ElapsedFrames;
                    }

                    if (Engine.Settings.Platform == Platform.NGage && AttachedObject?.Type == (int)ActorType.Plum && IsLocalPlayer)
                    {
                        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;
                        cam.HorizontalOffset = CameraOffset.DefaultBigger;
                    }
                }

                // TODO: Sound loops wrong
                // Super fist after 20 frames
                if (HasPower(Power.SuperFist) && GameTime.ElapsedFrames - Timer == 20)
                {
                    if (ActionId is Action.ChargeFist_Right or Action.ChargeFist_Left)
                    {
                        ActionId = IsFacingRight ? Action.ChargeSuperFist_Right : Action.ChargeSuperFist_Left;
                        PlaySound(Rayman3SoundEvent.Stop__Charge_Mix05);
                        PlaySound(Rayman3SoundEvent.Play__Charge2_Mix04);
                    }
                    else if (ActionId is Action.ChargeSecondFist_Right or Action.ChargeSecondFist_Left)
                    {
                        ActionId = IsFacingRight ? Action.ChargeSecondSuperFist_Right : Action.ChargeSecondSuperFist_Left;
                        PlaySound(Rayman3SoundEvent.Stop__Charge_Mix05);
                        PlaySound(Rayman3SoundEvent.Play__Charge2_Mix04);
                    }
                    else if (ActionId is Action.Climb_ChargeFist_Right or Action.Climb_ChargeFist_Left)
                    {
                        ActionId = IsFacingRight ? Action.Climb_ChargeSuperFist_Right : Action.Climb_ChargeSuperFist_Left;
                        PlaySound(Rayman3SoundEvent.Stop__Charge_Mix05);
                        PlaySound(Rayman3SoundEvent.Play__Charge2_Mix04);
                    }
                }

                int type = 0;

                // Stop charging and perform attack
                if (MultiJoyPad.IsButtonReleased(InstanceId, GbaInput.B))
                {
                    if (Timer == 0)
                        Timer = GameTime.ElapsedFrames;

                    uint chargePower = GameTime.ElapsedFrames - Timer;

                    // Move plum
                    if (AttachedObject?.Type == (int)ActorType.Plum)
                    {
                        // TODO: Implement and handle message to move plum
                        AttachedObject.ProcessMessage(this, IsFacingRight ? (Message)0x40c : (Message)0x40d, chargePower);
                    }

                    if (ActionId is
                        Action.ChargeFist_Right or Action.ChargeFist_Left or
                        Action.BeginChargeFist_Right or Action.BeginChargeFist_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.Fist, new Vector2(16, -16), ActionId is Action.ChargeFist_Right or Action.ChargeFist_Left);
                        NextActionId = IsFacingRight ? Action.EndChargeFist_Right : Action.EndChargeFist_Left;

                        if ((GameInfo.Powers & Power.DoubleFist) == 0)
                            field23_0x98 = 0;
                        type = 1;
                    }
                    else if (ActionId is 
                             Action.ChargeSecondFist_Right or Action.ChargeSecondFist_Left or
                             Action.BeginChargeSecondFist_Right or Action.BeginChargeSecondFist_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.SecondFist, new Vector2(16, -16), ActionId is Action.ChargeSecondFist_Right or Action.ChargeSecondFist_Left);
                        NextActionId = IsFacingRight ? Action.EndChargeSecondFist_Right : Action.EndChargeSecondFist_Left;

                        field23_0x98 = 0;
                        type = 1;
                    }
                    else if (ActionId is Action.ChargeSuperFist_Right or Action.ChargeSuperFist_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.SuperFist, new Vector2(16, -16), true);
                        NextActionId = IsFacingRight ? Action.EndChargeFist_Right : Action.EndChargeFist_Left;

                        type = 1;
                    }
                    else if (ActionId is Action.ChargeSecondSuperFist_Right or Action.ChargeSecondSuperFist_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.SecondSuperFist, new Vector2(16, -16), true);
                        NextActionId = IsFacingRight ? Action.EndChargeFist_Right : Action.EndChargeFist_Left;

                        field23_0x98 = 0;
                        type = 1;
                    }
                    else if (ActionId is Action.Hang_ChargeAttack_Right or Action.Hang_ChargeAttack_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.Foot, new Vector2(16, 0), true);
                        NextActionId = IsFacingRight ? Action.Hang_Attack_Right : Action.Hang_Attack_Left;

                        type = 2;
                    }
                    else if (ActionId is 
                             Action.HangOnEdge_ChargeAttack_Right or Action.HangOnEdge_ChargeAttack_Left or
                             Action.HangOnEdge_BeginAttack_Right or Action.HangOnEdge_BeginAttack_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.Foot, new Vector2(16, 16), true);
                        NextActionId = IsFacingRight ? Action.HangOnEdge_EndAttack_Right : Action.HangOnEdge_EndAttack_Left;

                        type = 3;
                    }
                    else if (ActionId is Action.Climb_ChargeSuperFist_Right or Action.Climb_ChargeSuperFist_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.SuperFist, new Vector2(16, -32), true);
                        NextActionId = IsFacingRight ? Action.Climb_EndChargeFist_Right : Action.Climb_EndChargeFist_Left;

                        type = 4;
                    }
                    else if (ActionId is 
                             Action.Climb_ChargeFist_Right or Action.Climb_ChargeFist_Left or
                             Action.Climb_BeginChargeFist_Right or Action.Climb_BeginChargeFist_Left)
                    {
                        Attack(chargePower, RaymanBody.RaymanBodyPartType.Fist, new Vector2(16, -32), true);
                        NextActionId = IsFacingRight ? Action.Climb_EndChargeFist_Right : Action.Climb_EndChargeFist_Left;

                        type = 4;
                    }
                }

                if (ActionId is Action.Hang_ChargeAttack_Right or Action.Hang_ChargeAttack_Left && !IsOnHangable())
                {
                    IsHanging = false;
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    State.MoveTo(Fsm_StopHelico);
                    return false;
                }

                if (type == 2)
                {
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                if (type == 4)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (type == 3)
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (type == 1 && AttachedObject?.Type == (int)ActorType.Plum)
                {
                    State.MoveTo(FUN_080224f4);
                    return false;
                }

                if (type == 1)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (Engine.Settings.Platform == Platform.NGage &&
                    ActionId is Action.Damage_Shock_Right or Action.Damage_Shock_Left)
                {
                    ActionId = IsFacingRight ? Action.Damage_Hit_Right : Action.Damage_Hit_Left;
                    State.MoveTo(Fsm_Hit);
                    return false;
                }

                if (Speed.Y > 1 && AttachedObject?.Type != (int)ActorType.Plum)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                PlaySound(Rayman3SoundEvent.Stop__Charge_Mix05);
                PlaySound(Rayman3SoundEvent.Stop__Charge2_Mix04);

                if (IsLocalPlayer && Engine.Settings.Platform == Platform.NGage)
                {
                    CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

                    if (RSMultiplayer.IsActive)
                    {
                        cam.HorizontalOffset = CameraOffset.Multiplayer;
                    }
                    else
                    {
                        cam.HorizontalOffset = CameraOffset.Default;
                        Flag1_D = true;
                        field16_0x91 = 0;
                    }
                }
                break;
        }

        return true;
    }

    public bool Fsm_BodyShotAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.BodyShot_Right : Action.BodyShot_Left;
                PlaySound(Rayman3SoundEvent.Play__BodyAtk1_Mix01);
                NextActionId = null;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (IsLocalPlayer)
                    Scene.Camera.ProcessMessage(this, Message.Cam_DoNotFollowPositionY, 130);
                
                if (AnimatedObject.CurrentFrame == 6 && CanAttackWithBody())
                    Attack(90, RaymanBody.RaymanBodyPartType.Torso, Vector2.Zero, false);

                if (IsActionFinished && !IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_TimeoutHelico);
                    return false;
                }

                if (IsActionFinished && IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_QuickFinishBodyShotAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                break;

            case FsmAction.Step:
                if (IsLocalPlayer)
                    Scene.Camera.ProcessMessage(this, Message.Cam_DoNotFollowPositionY, 130);

                MechModel.Speed = MechModel.Speed with { Y = 4 };

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_StopHelico);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_WallJump(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = Action.WallJump_Jump;
                PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (Speed.Y > 0)
                    ActionId = Action.WallJump_Fall;

                if (!IsOnWallJumpable())
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L))
                {
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_WallJumpIdle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = Action.WallJump_Move;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                // Slide down
                if ((ActionId == Action.WallJump_Idle && Timer > 30) || 
                    (ActionId == Action.WallJump_IdleStill && Timer > 90))
                {
                    Position += new Vector2(0, 0.5f);
                }
                else if ((ActionId == Action.WallJump_Idle && Timer == 30) || 
                         (ActionId == Action.WallJump_IdleStill && Timer == 90)) 
                {
                    PlaySound(Rayman3SoundEvent.Play__WallSlid_Mix02);
                }

                if (ActionId == Action.WallJump_Move && IsActionFinished)
                    ActionId = Action.WallJump_IdleStill;

                if (ActionId is Action.WallJump_IdleStill or Action.WallJump_Move && MultiJoyPad.IsButtonReleased(InstanceId, GbaInput.L))
                {
                    if (ActionId == Action.WallJump_Move && AnimatedObject.CurrentFrame < 4)
                        PlaySound(Rayman3SoundEvent.Play__HandTap2_Mix03);

                    ActionId = Action.WallJump_Idle;
                    Timer = 0;
                }

                Timer++;

                if (ActionId == Action.WallJump_Idle && MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A))
                {
                    State.MoveTo(Fsm_WallJump);
                    return false;
                }

                if ((ActionId == Action.WallJump_Idle && Timer > 60) ||
                    (ActionId == Action.WallJump_IdleStill && Timer > 120))
                {
                    State.MoveTo(Fsm_WallJumpFall);
                    return false;
                }

                if (PreventWallJumps)
                {
                    PreventWallJumps = false;
                    State.MoveTo(Fsm_WallJumpFall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                PlaySound(Rayman3SoundEvent.Stop__WallSlid_Mix02);
                break;
        }

        return true;
    }

    public bool Fsm_WallJumpFall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = Action.WallJump_Fall;
                Timer = GameTime.ElapsedFrames;
                PlaySound(Rayman3SoundEvent.Play__OnoPeur1_Mix03);
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (!IsOnWallJumpable())
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.L) && GameTime.ElapsedFrames - Timer > 20)
                {
                    State.MoveTo(Fsm_WallJumpIdle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Climb(FsmAction action)
    {
        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

        switch (action)
        {
            case FsmAction.Init:
                if (NextActionId == null)
                    ActionId = IsFacingRight ? Action.Climb_Idle_Right : Action.Climb_Idle_Left;
                else
                    ActionId = NextActionId.Value;

                PreviousXSpeed = 0;
                MechModel.Speed = Vector2.Zero;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                Timer++;

                // Keep the same frame across all climbing animations
                int animFrame = AnimatedObject.CurrentFrame;
                bool jump = MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A);

                int climbHoriontal = IsOnClimbableHorizontal();
                int climbVertical = IsOnClimbableVertical();

                PhysicalType type = PhysicalTypeValue.None;

                MechModel.Speed = MechModel.Speed with { X = 0 };

                if (IsDirectionalButtonPressed(GbaInput.Left))
                {
                    if (IsDirectionalButtonJustPressed(GbaInput.Left))
                        Timer = 0;

                    if (climbHoriontal is 4 or 6)
                    {
                        if (!RSMultiplayer.IsActive)
                        {
                            MechModel.Speed = MechModel.Speed with { X = -1.5f };

                            if (Timer > 50)
                                Timer = 0;
                        }
                        else
                        {
                            UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;
                            int tagId = userInfo.GetTagId();

                            if ((MultiplayerInfo.GameType == MultiplayerGameType.RayTag && InstanceId == tagId) ||
                                (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && InstanceId != tagId) ||
                                // NOTE: This appears to be a bug/oversight - tagId is always -1 for capture the flag
                                (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && InstanceId != tagId))
                            {
                                MechModel.Speed = MechModel.Speed with { X = -2f };
                            }
                            else
                            {
                                MechModel.Speed = MechModel.Speed with { X = -1.5f };
                            }
                        }
                    }
                    else if (Timer > 50 && !RSMultiplayer.IsActive)
                    {
                        cam.HorizontalOffset = CameraOffset.Default;
                        Timer = 0;
                    }
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right))
                {
                    if (IsDirectionalButtonJustPressed(GbaInput.Right))
                        Timer = 0;

                    if (climbHoriontal is 4 or 5)
                    {
                        if (!RSMultiplayer.IsActive)
                        {
                            MechModel.Speed = MechModel.Speed with { X = 1.5f };

                            if (Timer > 50)
                                Timer = 0;
                        }
                        else
                        {
                            UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;
                            int tagId = userInfo.GetTagId();

                            if ((MultiplayerInfo.GameType == MultiplayerGameType.RayTag && InstanceId == tagId) ||
                                (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && InstanceId != tagId) ||
                                // NOTE: This appears to be a bug/oversight - tagId is always -1 for capture the flag
                                (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && InstanceId != tagId))
                            {
                                MechModel.Speed = MechModel.Speed with { X = 2f };
                            }
                            else
                            {
                                MechModel.Speed = MechModel.Speed with { X = 1.5f };
                            }
                        }
                    }
                    else if (Timer > 50 && !RSMultiplayer.IsActive)
                    {
                        cam.HorizontalOffset = CameraOffset.Default;
                        Timer = 0;
                    }
                }
                else if (Timer > 50 && !RSMultiplayer.IsActive)
                {
                    // Center camera, only on GBA
                    if (Engine.Settings.Platform == Platform.GBA)
                        cam.HorizontalOffset = CameraOffset.Center;

                    Timer = 0;
                }

                if (IsDirectionalButtonPressed(GbaInput.Up) && climbVertical is 1 or 2)
                {
                    if (!RSMultiplayer.IsActive)
                    {
                        MechModel.Speed = MechModel.Speed with { Y = -1.5f };
                    }
                    else
                    {
                        UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;
                        int tagId = userInfo.GetTagId();

                        if ((MultiplayerInfo.GameType == MultiplayerGameType.RayTag && InstanceId == tagId) ||
                            (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && InstanceId != tagId) ||
                            // NOTE: This appears to be a bug/oversight - tagId is always -1 for capture the flag
                            (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && InstanceId != tagId))
                        {
                            MechModel.Speed = MechModel.Speed with { Y = -2f };
                        }
                        else
                        {
                            MechModel.Speed = MechModel.Speed with { Y = -1.5f };
                        }
                    }

                    if (ActionId is not (Action.Climb_Up_Right or Action.Climb_Up_Left))
                    {
                        ActionId = IsFacingRight ? Action.Climb_Up_Right : Action.Climb_Up_Left;
                        AnimatedObject.CurrentFrame = animFrame;
                    }
                }
                else if (IsDirectionalButtonPressed(GbaInput.Down) && climbVertical is 1 or 3)
                {
                    if (!RSMultiplayer.IsActive)
                    {
                        MechModel.Speed = MechModel.Speed with { Y = 1.5f };
                    }
                    else
                    {
                        UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;
                        int tagId = userInfo.GetTagId();

                        if ((MultiplayerInfo.GameType == MultiplayerGameType.RayTag && InstanceId == tagId) ||
                            (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && InstanceId != tagId) ||
                            // NOTE: This appears to be a bug/oversight - tagId is always -1 for capture the flag
                            (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && InstanceId != tagId))
                        {
                            MechModel.Speed = MechModel.Speed with { Y = 2f };
                        }
                        else
                        {
                            MechModel.Speed = MechModel.Speed with { Y = 1.5f };
                        }
                    }

                    if (ActionId is not (Action.Climb_Down_Right or Action.Climb_Down_Left))
                    {
                        ActionId = IsFacingRight ? Action.Climb_Down_Right : Action.Climb_Down_Left;
                        AnimatedObject.CurrentFrame = animFrame;
                    }
                }
                else
                {
                    MechModel.Speed = MechModel.Speed with { Y = 0 };

                    if (IsDirectionalButtonPressed(GbaInput.Left) && ActionId != Action.Climb_Side_Left && Speed.X != 0)
                    {
                        ActionId = Action.Climb_Side_Left;
                        AnimatedObject.CurrentFrame = animFrame;
                    }
                    else if (IsDirectionalButtonPressed(GbaInput.Right) && ActionId != Action.Climb_Side_Right && Speed.X != 0)
                    {
                        ActionId = Action.Climb_Side_Right;
                        AnimatedObject.CurrentFrame = animFrame;
                    }

                    if (IsDirectionalButtonPressed(GbaInput.Down) && climbVertical is not (1 or 3))
                    {
                        type = Scene.GetPhysicalType(Position + new Vector2(0, 32));
                    }
                }

                if (Speed == Vector2.Zero)
                {
                    if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                        AnimatedObject.FlipX = true;
                    else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                        AnimatedObject.FlipX = false;

                    if (ActionId == NextActionId)
                    {
                        if (IsActionFinished)
                        {
                            ActionId = IsFacingRight ? Action.Climb_Idle_Right : Action.Climb_Idle_Left;
                            NextActionId = null;
                        }
                    }
                    else
                    {
                        if (IsActionFinished && ActionId is (Action.Climb_BeginIdle_Right or Action.Climb_BeginIdle_Left))
                        {
                            ActionId = IsFacingRight ? Action.Climb_Idle_Right : Action.Climb_Idle_Left;
                        }
                        else if (ActionId is not (Action.Climb_Idle_Right or Action.Climb_Idle_Left or Action.Climb_BeginIdle_Right or Action.Climb_BeginIdle_Left))
                        {
                            ActionId = IsFacingRight ? Action.Climb_BeginIdle_Right : Action.Climb_BeginIdle_Left;
                        }
                    }
                }

                // Punch
                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B) && CanAttackWithFist(1))
                {
                    ActionId = IsFacingRight ? Action.Climb_BeginChargeFist_Right : Action.Climb_BeginChargeFist_Left;
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                // Jump left
                if (jump && IsDirectionalButtonPressed(GbaInput.Left) && climbHoriontal is not (4 or 6))
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Jump right
                if (jump && IsDirectionalButtonPressed(GbaInput.Right) && climbHoriontal is not (4 or 5))
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Jump up
                if (jump && IsDirectionalButtonPressed(GbaInput.Up) && climbVertical is not (1 or 2))
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                // Move down
                if (type.IsSolid && IsDirectionalButtonPressed(GbaInput.Down) && climbVertical is not (1 or 3))
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // Jump down
                if (jump && IsDirectionalButtonPressed(GbaInput.Down) && climbVertical is not (1 or 3))
                {
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (!RSMultiplayer.IsActive)
                    cam.HorizontalOffset = CameraOffset.Default;

                if (!MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B) && IsLocalPlayer)
                    cam.ProcessMessage(this, Message.Cam_ResetPositionX);

                if (ActionId == NextActionId)
                    NextActionId = null;
                break;
        }

        return true;
    }

    public bool Fsm_Hang(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (NextActionId != null)
                    ActionId = NextActionId.Value;
                else
                    ActionId = IsFacingRight ? Action.Hang_Idle_Right : Action.Hang_Idle_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (IsActionFinished && ActionId == NextActionId)
                {
                    ActionId = IsFacingRight ? Action.Hang_Idle_Right : Action.Hang_Idle_Left;
                    NextActionId = null;
                }

                // Change direction
                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.Hang_Move_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.Hang_Move_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                // Move
                if (IsDirectionalButtonPressed(GbaInput.Left) || IsDirectionalButtonPressed(GbaInput.Right))
                {
                    State.MoveTo(Fsm_HangMove);
                    return false;
                }

                // Move down
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    Position += new Vector2(0, Tile.Size);
                    IsHanging = false;
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // No longer hanging
                if (!IsOnHangable())
                {
                    IsHanging = false;
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Attack
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B) && CanAttackWithFoot())
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (ActionId == NextActionId || ActionId is Action.Hang_Move_Right or Action.Hang_Move_Left)
                    NextActionId = null;
                break;
        }

        return true;
    }

    public bool Fsm_HangMove(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Hang_Move_Right : Action.Hang_Move_Left;
                NextActionId = null;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (IsActionFinished && ActionId == NextActionId)
                {
                    ActionId = IsFacingRight ? Action.Hang_Move_Right : Action.Hang_Move_Left;
                    NextActionId = null;
                }

                // Change direction
                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.Hang_Move_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.Hang_Move_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                // Stop moving
                if (!IsDirectionalButtonPressed(GbaInput.Left) && !IsDirectionalButtonPressed(GbaInput.Right))
                {
                    NextActionId = IsFacingRight ? Action.Hang_EndMove_Right : Action.Hang_EndMove_Left;
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                // Move down
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    Position += new Vector2(0, Tile.Size);
                    IsHanging = false;
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                // No longer hanging
                if (!IsOnHangable())
                {
                    IsHanging = false;
                    PlaySound(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Attack
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B) && CanAttackWithFoot())
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (ActionId == NextActionId)
                    NextActionId = null;
                break;
        }

        return true;
    }

    public bool Fsm_Swing(FsmAction action)
    {
        CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Swing;
                ChangeAction();
                NextActionId = null;

                Timer = (uint)MathHelpers.Atan2_256(Position.X - AttachedObject.Position.X, Position.Y - AttachedObject.Position.Y);
                PreviousXSpeed = (Position - AttachedObject.Position).Length();

                if (Position.X < AttachedObject.Position.X)
                {
                    Flag2_1 = true;

                    if (Timer > 128)
                    {
                        AnimatedObject.CurrentFrame = 0;
                        Timer = 128;
                    }
                    else
                    {
                        AnimatedObject.CurrentFrame = (int)(Timer / 40);
                    }
                }
                else
                {
                    Flag2_1 = false;

                    if (Timer > 128)
                    {
                        AnimatedObject.CurrentFrame = 19;
                        Timer = 0;
                    }
                    else
                    {
                        AnimatedObject.CurrentFrame = (int)(Timer / 12 + 19);
                    }
                }

                cam.HorizontalOffset = AnimatedObject.CurrentFrame < 19 ? CameraOffset.Default : CameraOffset.DefaultReversed;

                CreateSwingProjectiles();
                PlaySound(Rayman3SoundEvent.Play__LumMauve_Mix02);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (AnimatedObject.CurrentFrame == 19 && Timer == 0)
                {
                    cam.HorizontalOffset = CameraOffset.DefaultReversed;
                }
                else if (AnimatedObject.CurrentFrame == 39 && Timer == 128)
                {
                    cam.HorizontalOffset = CameraOffset.Default;
                }

                if (Flag2_1)
                {
                    if (Timer == 0)
                    {
                        Flag2_1 = false;
                        Timer = 0;
                        AnimatedObject.CurrentFrame = 19;
                    }
                    else
                    {
                        float xPos = AttachedObject.Position.X + MathHelpers.Cos256(Timer) * PreviousXSpeed;
                        float yPos = AttachedObject.Position.Y + MathHelpers.Sin256(Timer) * PreviousXSpeed;
                        Position = new Vector2(xPos, yPos);

                        if (PreviousXSpeed < 80)
                        {
                            // TODO: This is a bug in the src code! It's supposed to use Timer, not ActionId. Fix?
                            int value = (int)ActionId;
                            if (value is (< 10 or > 14) and (< 27 or > 34))
                                PreviousXSpeed += 0.5f;
                            else
                                PreviousXSpeed += 4;

                            if (PreviousXSpeed > 80)
                                PreviousXSpeed = 80;
                        }
                        else if (PreviousXSpeed > 80)
                        {
                            PreviousXSpeed -= 1;

                            if (PreviousXSpeed < 80)
                                PreviousXSpeed = 80;
                        }

                        if (Timer is < 4 or >= 125)
                            Timer -= 1;
                        else if (Timer is < 25 or >= 103)
                            Timer -= 1;
                        else if (Timer is < 51 or >= 77)
                            Timer -= 2;
                        else
                            Timer -= 2;
                    }
                }
                else
                {
                    if (Timer >= 128)
                    {
                        Flag2_1 = true;
                        Timer = 128;
                        AnimatedObject.CurrentFrame = 39;
                    }
                    else
                    {
                        float xPos = AttachedObject.Position.X + MathHelpers.Cos256(Timer) * PreviousXSpeed;
                        float yPos = AttachedObject.Position.Y + MathHelpers.Sin256(Timer) * PreviousXSpeed;
                        Position = new Vector2(xPos, yPos);

                        if (PreviousXSpeed < 80)
                        {
                            // TODO: This is a bug in the src code! It's supposed to use Timer, not ActionId. Fix?
                            int value = (int)ActionId;
                            if (value is (< 10 or > 14) and (< 27 or > 34))
                                PreviousXSpeed += 0.5f;
                            else
                                PreviousXSpeed += 4;

                            if (PreviousXSpeed > 80)
                                PreviousXSpeed = 80;
                        }
                        else if (PreviousXSpeed > 80)
                        {
                            PreviousXSpeed -= 1;

                            if (PreviousXSpeed < 80)
                                PreviousXSpeed = 80;
                        }

                        if (Timer is < 4 or >= 125)
                            Timer += 1;
                        else if (Timer is < 25 or >= 103)
                            Timer += 1;
                        else if (Timer is < 51 or >= 77)
                            Timer += 2;
                        else
                            Timer += 2;
                    }
                }

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && !Scene.GetPhysicalType(Position).IsSolid)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AttachedObject = null;

                // Momentum and direction
                if (Flag2_1)
                {
                    PreviousXSpeed = 1;
                }
                else
                {
                    PreviousXSpeed = -1;
                    ActionId = Action.Jump_Left;
                    ChangeAction();
                }

                Flag2_1 = false;

                cam.HorizontalOffset = GameInfo.MapId == MapId.TheCanopy_M2 ? CameraOffset.Center : CameraOffset.Default;
                break;
        }

        return true;
    }

    public bool Fsm_Bounce(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = IsFacingRight ? Action.BeginBounce_Right : Action.BeginBounce_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (Flag1_5)
                {
                    Flag1_5 = false;

                    if (Engine.Settings.Platform == Platform.NGage)
                        ActionId = IsFacingRight ? Action.BouncyJump_Right : Action.BouncyJump_Left;

                    State.MoveTo(Fsm_Jump);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (Engine.Settings.Platform != Platform.NGage)
                    ActionId = IsFacingRight ? Action.BouncyJump_Right : Action.BouncyJump_Left;
                break;
        }

        return true;
    }

    public bool Fsm_PickUpObject(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Play__OnoEfor2_Mix03);
                ActionId = IsFacingRight ? Action.PickUpObject_Right : Action.PickUpObject_Left;
                NextActionId = null;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                // Sync the objects position with Rayman
                Vector2 objOffset = AnimatedObject.CurrentFrame switch
                {
                    0 => new Vector2(23, 19),
                    1 => new Vector2(23, 19),
                    2 => new Vector2(23, 19),
                    3 => new Vector2(23, 18),
                    4 => new Vector2(23, 18),
                    5 => new Vector2(25, 16),
                    6 => new Vector2(30, 4),
                    7 => new Vector2(23, -15),
                    8 => new Vector2(15, -15),
                    9 => new Vector2(11, -15),
                    10 => new Vector2(1, -15),
                    11 => new Vector2(-3, -1),
                    12 => new Vector2(-3, 3),
                    13 => new Vector2(-3, 2),
                    14 => new Vector2(-3, 0),
                    15 => new Vector2(-3, -2),
                    _ => throw new Exception("Invalid frame index")
                };

                if (IsFacingRight)
                    AttachedObject.Position = Position + new Vector2(objOffset.X + 6, objOffset.Y - 22);
                else
                    AttachedObject.Position = Position + new Vector2(-objOffset.X - 4, objOffset.Y - 22);

                OffsetCarryingObject();

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_CarryObject);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_CatchObject(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Play__OnoEfor2_Mix03);
                ActionId = IsFacingRight ? Action.CatchObject_Right : Action.CatchObject_Left;

                if (IsActionFinished)
                {
                    if (IsFacingRight)
                        AttachedObject.Position = AttachedObject.Position with { X = Position.X + 8 };
                    else
                        AttachedObject.Position = AttachedObject.Position with { X = Position.X - 8 };
                }

                NextActionId = null;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                // Sync the objects position with Rayman
                float objYOffset = AnimatedObject.CurrentFrame switch
                {
                    0 => -36,
                    1 => -27,
                    2 => 0,
                    3 => 7,
                    4 => 8,
                    5 => 8,
                    6 => 6,
                    7 => 0,
                    8 => 0,
                    9 => 0,
                    10 => 0,
                    _ => throw new Exception("Invalid frame index")
                };

                if (IsFacingRight)
                    AttachedObject.Position = Position + new Vector2(6, objYOffset - 20);
                else
                    AttachedObject.Position = Position + new Vector2(-4, objYOffset - 20);

                OffsetCarryingObject();

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_CarryObject);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_CarryObject(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = IsFacingRight ? Action.CarryObject_Right : Action.CarryObject_Left;

                if (IsFacingRight)
                    AttachedObject.Position = Position + new Vector2(4, -22);
                else
                    AttachedObject.Position = Position + new Vector2(-4, -22);

                OffsetCarryingObject();
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                // Change direction
                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.CarryObject_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.CarryObject_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                if (IsFacingRight)
                    AttachedObject.Position = Position + new Vector2(6, -22);
                else
                    AttachedObject.Position = Position + new Vector2(-4, -22);

                // Sync the objects position with Rayman
                float objYOffset = AnimatedObject.CurrentFrame switch
                {
                    0 or 1 => -22,
                    2 or 3 or 4 or 5 => -23,
                    6 or 7 => -22,
                    8 or 9 or 10 => -21,
                    _ => -22
                };

                AttachedObject.Position = AttachedObject.Position with { Y = Position.Y + objYOffset };

                OffsetCarryingObject();

                // Walk
                if (IsDirectionalButtonPressed(GbaInput.Left) || IsDirectionalButtonPressed(GbaInput.Right))
                {
                    State.MoveTo(Fsm_WalkWithObject);
                    return false;
                }

                // Throw
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) || MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B))
                {
                    State.MoveTo(Fsm_ThrowObject);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_WalkWithObject(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.WalkWithObject_Right : Action.WalkWithObject_Left;
                NextActionId = null;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                // Change direction
                if (IsDirectionalButtonPressed(GbaInput.Left) && IsFacingRight)
                {
                    ActionId = Action.WalkWithObject_Left;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right) && IsFacingLeft)
                {
                    ActionId = Action.WalkWithObject_Right;
                    ChangeAction();

                    if (Engine.Settings.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerData != null)
                        MultiplayerData.field_b9 = 1;
                }

                // Sync the objects position with Rayman
                Vector2 objOffset = AnimatedObject.CurrentFrame switch
                {
                    0 => new Vector2(0, 0),
                    1 => new Vector2(0, 2),
                    2 => new Vector2(-1, 4),
                    3 => new Vector2(-2, 4),
                    4 => new Vector2(-3, 2),
                    5 => new Vector2(-4, 0),
                    6 => new Vector2(-4, -2),
                    7 => new Vector2(-3, -3),
                    8 => new Vector2(-1, -3),
                    9 => new Vector2(0, -3),
                    10 => new Vector2(0, 0),
                    11 => new Vector2(0, 2),
                    12 => new Vector2(-1, 4),
                    13 => new Vector2(-2, 4),
                    14 => new Vector2(-3, 2),
                    15 => new Vector2(-4, 0),
                    16 => new Vector2(-4, -2),
                    17 => new Vector2(-3, -3),
                    18 => new Vector2(-1, -3),
                    19 => new Vector2(0, -3),
                    _ => throw new Exception("Invalid frame index")
                };

                if (IsFacingRight)
                    AttachedObject.Position = Position + new Vector2(objOffset.X + 8, objOffset.Y - 22);
                else
                    AttachedObject.Position = Position + new Vector2(-objOffset.X - 4, objOffset.Y - 22);

                if (Speed.Y > 1)
                {
                    AttachedObject.ProcessMessage(this, Message.DropObject);
                    AttachedObject = null;
                }

                OffsetCarryingObject();

                // TODO: There's a bug here which causes a crash. Same in the original game. If falling it might still enter this state with attached obj null.
                // Stop walking
                if (IsDirectionalButtonButtonReleased(GbaInput.Left) && IsDirectionalButtonButtonReleased(GbaInput.Right))
                {
                    State.MoveTo(Fsm_CarryObject);
                    return false;
                }

                // Throw
                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) || MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B))
                {
                    State.MoveTo(Fsm_ThrowObject);
                    return false;
                }

                // Falling
                if (Speed.Y > 1)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrowObject(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Play__OnoThrow_Mix02);

                if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A))
                    ActionId = IsFacingRight ? Action.ThrowObjectUp_Right : Action.ThrowObjectUp_Left;
                else
                    ActionId = IsFacingRight ? Action.ThrowObjectForward_Right : Action.ThrowObjectForward_Left;

                NextActionId = null;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoOnTheGround())
                    return false;

                if (ActionId is Action.ThrowObjectForward_Right or Action.ThrowObjectForward_Left)
                {
                    if (AnimatedObject.CurrentFrame == 0)
                    {
                        if (AttachedObject != null) // This check is only on N-Gage, but we need it to avoid null exception when animation ends
                        {
                            if (IsFacingRight)
                                AttachedObject.Position = Position + new Vector2(6, -22);
                            else
                                AttachedObject.Position = Position + new Vector2(-4, -22);
                        }
                    }
                    else if (AnimatedObject.CurrentFrame is >= 1 and < 7)
                    {
                        // Sync the objects position with Rayman
                        Vector2 objOffset = AnimatedObject.CurrentFrame switch
                        {
                            0 => new Vector2(0, 0),
                            1 => new Vector2(-3, -20),
                            2 => new Vector2(-31, -12),
                            3 => new Vector2(-40, -10),
                            4 => new Vector2(-38, -10),
                            5 => new Vector2(-33, -10),
                            6 => new Vector2(-13, -20),
                            _ => throw new Exception("Invalid frame index")
                        };

                        if (IsFacingRight)
                            AttachedObject.Position = Position + new Vector2(objOffset.X, objOffset.Y - 20);
                        else
                            AttachedObject.Position = Position + new Vector2(-objOffset.X, objOffset.Y - 20);
                    }
                    else if (AttachedObject != null)
                    {
                        PlaySound(Rayman3SoundEvent.Play__GenWoosh_LumSwing_Mix03);
                        AttachedObject.ProcessMessage(this, Message.ThrowObjectForward);
                        AttachedObject = null;
                    }
                }
                else
                {
                    if (AnimatedObject.CurrentFrame < 7)
                    {
                        if (AttachedObject != null) // This check is only on N-Gage, but we need it to avoid null exception when animation ends
                        {
                            // Sync the objects position with Rayman
                            float objYOffset = AnimatedObject.CurrentFrame switch
                            {
                                0 => 0,
                                1 => 3,
                                2 => 4,
                                3 => 9,
                                4 => 2,
                                5 => -11,
                                6 => -40,
                                _ => throw new Exception("Invalid frame index")
                            };

                            if (IsFacingRight)
                                AttachedObject.Position = Position + new Vector2(6, objYOffset - 22);
                            else
                                AttachedObject.Position = Position + new Vector2(-4, objYOffset - 22);
                        }
                    }
                    else if (AttachedObject != null)
                    {
                        AttachedObject.ProcessMessage(this, Message.ThrowObjectUp);
                        AttachedObject = null;
                    }
                }

                OffsetCarryingObject();

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B) &&
                    CanAttackWithFist(2) &&
                    field23_0x98 == 0 &&
                    ActionId is Action.ThrowObjectUp_Right or Action.ThrowObjectUp_Left &&
                    AnimatedObject.CurrentFrame > 6)
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_FlyWithKeg(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.CarryObject_Right : Action.CarryObject_Left;
                NextActionId = null;
                MechModel.Speed = MechModel.Speed with { Y = 0 };
                
                if (IsFacingRight)
                    AttachedObject.Position = Position + new Vector2(10, -22);
                else
                    AttachedObject.Position = Position + new Vector2(-8, -22);

                SetDetectionBox(new Box(
                    minX: ActorModel.DetectionBox.MinX - 10,
                    minY: ActorModel.DetectionBox.MinY - 22,
                    maxX: ActorModel.DetectionBox.MaxX,
                    maxY: ActorModel.DetectionBox.MaxY - 22));
                break;

            case FsmAction.Step:
                if (!FsmStep_DoInTheAir())
                    return false;

                if (StartFlyingWithKegRight)
                {
                    PlaySound(Rayman3SoundEvent.Play__OnoGO_Mix02);

                    if (SongAlternation)
                    {
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__barrel_BA, 3);
                        SongAlternation = false;
                    }
                    else
                    {
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__barrel, 3);
                        SongAlternation = true;
                    }

                    StartFlyingWithKegRight = false;
                    ActionId = IsFacingRight ? Action.FlyForwardWithKeg_Right : Action.FlyBackwardsWithKeg_Right;
                }
                else if (StartFlyingWithKegLeft)
                {
                    PlaySound(Rayman3SoundEvent.Play__OnoGO_Mix02);

                    if (SongAlternation)
                    {
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__barrel_BA, 3);
                        SongAlternation = false;
                    }
                    else
                    {
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__barrel, 3);
                        SongAlternation = true;
                    }

                    StartFlyingWithKegLeft = false;
                    ActionId = IsFacingRight ? Action.FlyForwardWithKeg_Left : Action.FlyBackwardsWithKeg_Left;
                }

                if (StopFlyingWithKeg)
                {
                    StopFlyingWithKeg = false;
                    Speed = Speed with { X = 0 };
                }

                if (IsFacingRight)
                    AttachedObject.Position = AttachedObject.Position with { X = Position.X + 6 };
                else
                    AttachedObject.Position = AttachedObject.Position with { X = Position.X + -4 };

                if (ActionId is Action.CarryObject_Right or Action.CarryObject_Left)
                {
                    MechModel.Speed = MechModel.Speed with { Y = 0 };

                    // Sync the objects position with Rayman
                    switch (AnimatedObject.CurrentFrame)
                    {
                        case 0:
                        case 1:
                        case 2:
                            AttachedObject.Position = AttachedObject.Position with { Y = Position.Y - 22 };
                            break;
                        
                        case 3:
                        case 4:
                            AttachedObject.Position = AttachedObject.Position with { Y = Position.Y - 23 };
                            break;
                        
                        case 5:
                        case 6:
                            AttachedObject.Position = AttachedObject.Position with { Y = Position.Y - 22 };
                            break;
                        
                        case 7:
                        case 8:
                        case 9:
                            AttachedObject.Position = AttachedObject.Position with { Y = Position.Y - 21 };
                            break;
                    }
                }
                else
                {
                    AttachedObject.Position = AttachedObject.Position with { Y = Position.Y - 22 };

                    if (IsDirectionalButtonPressed(GbaInput.Up))
                        MechModel.Speed = MechModel.Speed with { Y = -2.5f };
                    else if (IsDirectionalButtonPressed(GbaInput.Down))
                        MechModel.Speed = MechModel.Speed with { Y = 2.5f };
                    else
                        MechModel.Speed = MechModel.Speed with { Y = 0 };
                }

                if (DropObject)
                {
                    DropObject = false;
                    PreviousXSpeed = 2;
                    State.MoveTo(Fsm_Helico);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AttachedObject = null;
                CheckAgainstMapCollision = true;
                Position -= new Vector2(0, -22);
                SetDetectionBox(new Box(ActorModel.DetectionBox));
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__echocave, 3);
                break;
        }

        return true;
    }

    public bool Fsm_EndMap(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // NOTE: The game doesn't check the class, and will end up writing to other memory if of type FrameWorldSideScroller
                if (Frame.Current is FrameSideScroller sideScroller)
                    sideScroller.CanPause = false;
                Flag2_1 = false;
                NextActionId = null;
                PreviousXSpeed = 0;
                if (HasLanded())
                {
                    if (FinishedMap)
                    {
                        Timer = 0;
                        ActionId = IsFacingRight ? Action.Victory_Right : Action.Victory_Left;
                        PlaySound(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);

                        if (GameInfo.MapId != MapId.BossBadDreams &&
                            GameInfo.MapId != MapId.BossScaleMan &&
                            GameInfo.MapId != MapId.BossFinal_M1 &&
                            GameInfo.MapId != MapId.BossFinal_M2 &&
                            GameInfo.MapId != MapId.BossMachine &&
                            GameInfo.MapId != MapId.BossRockAndLava)
                        {
                            SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                        }
                        else
                        {
                            LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__Win_BOSS);
                        }
                    }
                    else
                    {
                        ActionId = IsFacingRight ? Action.ReturnFromLevel_Right : Action.ReturnFromLevel_Left;
                        Timer = 0;
                    }

                    // NOTE: The game doesn't check the class, and will end up writing to other memory if of type FrameWorldSideScroller
                    if (Frame.Current is FrameSideScroller sideScroller2)
                        sideScroller2.IsTimed = false;
                }
                else
                {
                    ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                }
                break;

            case FsmAction.Step:
                if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__win3))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__canopy);

                // Don't allow horizontal movement while falling
                if (ActionId is Action.Fall_Right or Action.Fall_Left)
                    MechModel.Speed = MechModel.Speed with { X = 0 };

                Timer++;

                if (HasLanded() && ActionId is Action.Fall_Right or Action.Fall_Left)
                {
                    ActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    return true;
                }

                if (ActionId is Action.Land_Right or Action.Land_Left && IsActionFinished)
                {
                    if (FinishedMap)
                    {
                        Timer = 0;
                        ActionId = IsFacingRight ? Action.Victory_Right : Action.Victory_Left;
                        PlaySound(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);

                        if (GameInfo.MapId != MapId.BossBadDreams &&
                            GameInfo.MapId != MapId.BossScaleMan &&
                            GameInfo.MapId != MapId.BossFinal_M1 &&
                            GameInfo.MapId != MapId.BossFinal_M2 &&
                            GameInfo.MapId != MapId.BossMachine &&
                            GameInfo.MapId != MapId.BossRockAndLava)
                        {
                            SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                        }
                        else
                        {
                            LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__Win_BOSS);
                        }
                    }
                    else
                    {
                        ActionId = IsFacingRight ? Action.ReturnFromLevel_Right : Action.ReturnFromLevel_Left;
                        Timer = 0;
                    }

                    // NOTE: The game doesn't check the class, and will end up writing to other memory if of type FrameWorldSideScroller
                    if (Frame.Current is FrameSideScroller sideScroller2)
                        sideScroller2.IsTimed = false;

                    return true;
                }

                if (ActionId is Action.Idle_Right or Action.Idle_Left or Action.ReturnFromLevel_Right or Action.ReturnFromLevel_Left &&
                    ((!FinishedMap && Timer == 150) || (FinishedMap && Timer == 100)))
                {
                    if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__Win_BOSS))
                    {
                        Timer -= 2;
                        return true;
                    }

                    if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
                        SoundEventsManager.StopAllSongs();

                    if (FinishedMap)
                        SoundEventsManager.StopAllSongs();

                    return true;
                }

                if (ActionId is not (Action.Idle_Right or Action.Idle_Left or Action.ReturnFromLevel_Right or Action.ReturnFromLevel_Left) ||
                    ((FinishedMap || Timer <= 150) && (!FinishedMap || Timer <= 100)))
                {
                    if (!IsActionFinished)
                        return true;

                    if (ActionId is (Action.Victory_Right or Action.Victory_Left))
                    {
                        ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                        Timer = 0;
                    }

                    if (Flag2_1)
                        return true;

                    Flag2_1 = true;

                    if (Engine.Settings.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                        ((FrameSideScrollerGCN)Frame.Current).FUN_0808a9f4();

                    switch (GameInfo.MapId)
                    {
                        case MapId.CavesOfBadDreams_M1:
                        case MapId.CavesOfBadDreams_M2:
                            CavesOfBadDreams cavesOfBadDreams = (CavesOfBadDreams)Frame.Current;
                            switch (cavesOfBadDreams.Mode)
                            {
                                case CavesOfBadDreams.FadeMode.FadeIn:
                                    cavesOfBadDreams.Timer = 28 - cavesOfBadDreams.Timer;
                                    cavesOfBadDreams.Mode = CavesOfBadDreams.FadeMode.TransitionOut;
                                    break;

                                case CavesOfBadDreams.FadeMode.Visible:
                                    cavesOfBadDreams.Timer = 28;
                                    cavesOfBadDreams.Mode = CavesOfBadDreams.FadeMode.TransitionOut;
                                    break;

                                case CavesOfBadDreams.FadeMode.FadeOut:
                                default:
                                    cavesOfBadDreams.Mode = CavesOfBadDreams.FadeMode.TransitionOut;
                                    break;

                                case CavesOfBadDreams.FadeMode.Invisible:
                                    cavesOfBadDreams.Mode = CavesOfBadDreams.FadeMode.Ended;
                                    ((FrameSideScroller)Frame.Current).InitNewCircleTransition(false);
                                    break;
                            }
                            break;

                        case MapId.SanctuaryOfRockAndLava_M1:
                        case MapId.SanctuaryOfRockAndLava_M2:
                        case MapId.SanctuaryOfRockAndLava_M3:
                            // TODO: Implement
                            break;

                        case MapId.Power6:
                        case MapId.World1:
                        case MapId.World2:
                        case MapId.World3:
                        case MapId.World4:
                            ((World)Frame.Current).InitExiting();
                            break;

                        // NOTE: This is unused - the Rayman actor does not appear in the worldmap
                        case MapId.WorldMap:
                            ((WorldMap)Frame.Current).TransitionsFX.FadeOutInit(1 / 16f);
                            break;

                        default:
                            ((FrameSideScroller)Frame.Current).InitNewCircleTransition(false);
                            break;
                    }

                    return true;
                }

                if (FinishedMap)
                {
                    if (Engine.Settings.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                    {
                        ((FrameSideScrollerGCN)Frame.Current).RestoreMapAndPowers();
                        int gcnMapId = ((FrameSideScrollerGCN)Frame.Current).GcnMapId;

                        if (GameInfo.PersistentInfo.CompletedGCNBonusLevels < gcnMapId + 1)
                            GameInfo.PersistentInfo.CompletedGCNBonusLevels = (byte)(gcnMapId + 1);

                        FrameManager.SetNextFrame(new GameCubeMenu());
                        GameInfo.Save(GameInfo.CurrentSlot);
                    }
                    else if (GameInfo.MapId > (MapId)GameInfo.PersistentInfo.LastCompletedLevel)
                    {
                        switch (GameInfo.MapId)
                        {
                            case MapId.WoodLight_M2:
                                GameInfo.LoadLevel(MapId.Power1);
                                break;

                            case MapId.BossMachine:
                                GameInfo.LoadLevel(MapId.Power2);
                                break;

                            case MapId.EchoingCaves_M2:
                                GameInfo.LoadLevel(MapId.Power3);
                                break;

                            case MapId.SanctuaryOfStoneAndFire_M3:
                                GameInfo.LoadLevel(MapId.Power5);
                                break;

                            case MapId.BossRockAndLava:
                                GameInfo.LoadLevel(MapId.Power4);
                                break;

                            case MapId.BossScaleMan:
                                GameInfo.LoadLevel(MapId.Power6);
                                break;

                            default:
                                Frame.Current.EndOfFrame = true;
                                GameInfo.UpdateLastCompletedLevel();
                                break;
                        }
                    }
                    else
                    {
                        Frame.Current.EndOfFrame = true;
                    }
                }
                else
                {
                    if (Engine.Settings.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                    {
                        ((FrameSideScrollerGCN)Frame.Current).RestoreMapAndPowers();
                        FrameManager.SetNextFrame(new GameCubeMenu());
                    }
                    else if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
                    {
                        if (((World)Frame.Current).FinishedTransitioningOut)
                            GameInfo.LoadLevel(MapId.WorldMap);
                    }
                    else
                    {
                        GameInfo.LoadLevel(MapId.World1 + (int)GameInfo.WorldId);
                    }
                }

                AutoSave();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Hit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                if (ActionId is not (Action.Damage_Shock_Right or Action.Damage_Shock_Left))
                    ActionId = IsFacingRight ? Action.Damage_Hit_Right : Action.Damage_Hit_Left;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_HitKnockback(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                LinkedMovementActor = null;
                Timer = 0;

                if (Flag1_4)
                {
                    CheckAgainstMapCollision = false;
                    CheckAgainstObjectCollision = false;
                    ReceiveDamage(HitPoints);
                    Flag1_4 = false;
                }

                if (!Flag1_C)
                {
                    // Due to the lack of some null checks on GBA this code works differently on GBA and N-Gage if there is no attached object
                    bool right;
                    if (Engine.Settings.Platform == Platform.GBA)
                    {
                        // On GBA there is no null check making it evaluate the condition as true if so and prioritizing a knockback to the right
                        if (AttachedObject == null || Position.X - AttachedObject.Position.X >= 0)
                            right = true;
                        else
                            right = false;
                    }
                    else if (Engine.Settings.Platform == Platform.NGage)
                    {
                        // On N-Gage there's an added null check, making it prioritize a knockback to the left
                        if (AttachedObject != null && Position.X - AttachedObject.Position.X >= 0)
                            right = true;
                        else
                            right = false;
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    // Right
                    if (right)
                        ActionId = IsFacingRight ? Action.KnockbackForwards_Right : Action.KnockbackBackwards_Left;
                    // Left
                    else
                        ActionId = IsFacingRight ? Action.KnockbackBackwards_Right : Action.KnockbackForwards_Left;
                }
                else
                {
                    if (Position.X - AttachedObject.Position.X >= 0)
                        ActionId = IsFacingRight ? Action.SmallKnockbackForwards_Right : Action.SmallKnockbackBackwards_Left;
                    else
                        ActionId = IsFacingRight ? Action.SmallKnockbackBackwards_Right : Action.SmallKnockbackForwards_Left;
                }

                NextActionId = null;
                AttachedObject = null;
                PlaySound(Rayman3SoundEvent.Stop__SldGreen_SkiLoop1);
                break;

            case FsmAction.Step:
                if (HitPoints != 0)
                {
                    if (!FsmStep_DoInTheAir())
                        return false;
                }

                if (IsDirectionalButtonPressed(GbaInput.Left))
                {
                    if (IsFacingRight)
                        AnimatedObject.FlipX = true;
                }
                else if (IsDirectionalButtonPressed(GbaInput.Right))
                {
                    if (IsFacingLeft)
                        AnimatedObject.FlipX = false;
                }

                Timer++;

                // TODO: Seems to be a bug - fix? The flag is true if you started out climbing. This should probably set it to false after 25 frames.
                if (!Flag2_1 && Timer > 25)
                    Flag2_1 = false;

                if (HitPoints == 0 && Timer > 20)
                {
                    State.MoveTo(Fsm_Dying);
                    return false;
                }

                if (HitPoints != 0 && Timer > 90)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                if (HitPoints != 0 && MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && !IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_Helico);
                    return false;
                }

                if (HitPoints != 0 && MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && IsSuperHelicoActive)
                {
                    State.MoveTo(Fsm_SuperHelico);
                    return false;
                }

                if (HitPoints != 0 && IsNearHangableEdge())
                {
                    State.MoveTo(Fsm_HangOnEdge);
                    return false;
                }

                if (HitPoints != 0 && HasLanded())
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (HitPoints != 0 && Timer > 10 && IsOnHangable())
                {
                    BeginHang();
                    State.MoveTo(Fsm_Hang);
                    return false;
                }

                if (HitPoints != 0 && !Flag2_1 && IsOnClimbableVertical() == 1)
                {
                    State.MoveTo(Fsm_Climb);
                    return false;
                }

                if (HitPoints != 0 && MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && IsOnWallJumpable())
                {
                    BeginWallJump();
                    State.MoveTo(Fsm_WallJumpIdle);
                }
                break;

            case FsmAction.UnInit:
                CheckAgainstMapCollision = true;
                CheckAgainstObjectCollision = true;
                Flag2_1 = false;
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // NOTE: The game doesn't check the class, and will end up writing to other memory if of type FrameWorldSideScroller
                if (Frame.Current is FrameSideScroller sideScroller)
                    sideScroller.CanPause = false;

                if (GameInfo.MapId is not (MapId.ChallengeLy1 or MapId.ChallengeLy2 or MapId.ChallengeLyGCN) &&
                    GameInfo.LevelType != LevelType.GameCube)
                {
                    GameInfo.ModifyLives(-1);
                }

                PlaySound(Rayman3SoundEvent.Play__RaDeath_Mix03);
                Timer = GameTime.ElapsedFrames;
                ReceiveDamage(5);

                if (ActionId is not (Action.Drown_Right or Action.Drown_Left))
                    ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;

                NextActionId = null;

                if (GameInfo.LevelType == LevelType.GameCube)
                    ((FrameSideScrollerGCN)Frame.Current).FUN_0808a9f4();

                if (GameInfo.MapId is MapId.SanctuaryOfRockAndLava_M1 or MapId.SanctuaryOfRockAndLava_M2 or MapId.SanctuaryOfRockAndLava_M3)
                {
                    // TODO: Implement
                }
                else
                {
                    ((FrameSideScroller)Frame.Current).InitNewCircleTransition(false);
                }

                if (AttachedObject != null)
                {
                    AttachedObject.ProcessMessage(this, Message.DropObject);
                    AttachedObject = null;
                }
                break;

            case FsmAction.Step:
                if (IsActionFinished && GameTime.ElapsedFrames - Timer > 120)
                {
                    if (GameInfo.PersistentInfo.Lives == 0)
                    {
                        // Game over
                        FrameManager.SetNextFrame(new GameOver());
                    }
                    else
                    {
                        // Reload current map
                        FrameManager.ReloadCurrentFrame();
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MultiplayerDying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;

                if (userInfo.GetTime(InstanceId) != 0)
                    userInfo.RemoveTime(InstanceId, 10);

                PlaySound(Rayman3SoundEvent.Play__RaDeath_Mix03);

                ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;
                NextActionId = null;
                break;

            case FsmAction.Step:
                throw new NotImplementedException();
                break;

            case FsmAction.UnInit:
                throw new NotImplementedException();
                break;
        }

        return true;
    }

    public bool Fsm_Cutscene(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                PreviousXSpeed = 0;

                if (IsOnClimbableVertical() != 0)
                {
                    ActionId = IsFacingRight ? Action.Climb_Idle_Right : Action.Climb_Idle_Left;
                    MechModel.Speed = Vector2.Zero;
                }
                else if (Scene.GetPhysicalType(Position).IsSolid || Scene.MainActor.LinkedMovementActor != null || Speed.Y == 0)
                {
                    ActionId = IsFacingRight ? Action.Idle_BeginCutscene_Right : Action.Idle_BeginCutscene_Left;
                }
                else
                {
                    ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                }
                break;

            case FsmAction.Step:
                if (ActionId is Action.Fall_Right or Action.Fall_Left)
                    MechModel.Speed = MechModel.Speed with { X = 0 };

                if (IsOnClimbableVertical() == 1)
                {
                    if (ActionId is not (Action.Climb_Idle_Right or Action.Climb_Idle_Left))
                        ActionId = IsFacingRight ? Action.Climb_Idle_Right : Action.Climb_Idle_Left;
                }
                else if (Scene.GetPhysicalType(Position).IsSolid && ActionId is Action.Fall_Right or Action.Fall_Left)
                {
                    ActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    CameraTargetY = 120;
                    Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, CameraTargetY);
                }
                else if (ActionId is Action.Land_Right or Action.Land_Left && IsActionFinished)
                {
                    ActionId = IsFacingRight ? Action.Idle_BeginCutscene_Right : Action.Idle_BeginCutscene_Left;
                }
                else if (ActionId is Action.Idle_BeginCutscene_Right or Action.Idle_BeginCutscene_Left && IsActionFinished)
                {
                    ActionId = IsFacingRight ? Action.Idle_Cutscene_Right : Action.Idle_Cutscene_Left;
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
                NextActionId = null;
                PreviousXSpeed = 0;

                if (IsOnClimbableVertical() != 0)
                {
                    ActionId = IsFacingRight ? Action.Climb_Idle_Right : Action.Climb_Idle_Left;
                    MechModel.Speed = Vector2.Zero;
                }
                else if (Scene.GetPhysicalType(Position).IsSolid || Scene.MainActor.LinkedMovementActor != null || Speed.Y == 0)
                {
                    ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                }
                else
                {
                    ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                }
                break;

            case FsmAction.Step:
                if (ActionId is Action.Fall_Right or Action.Fall_Left)
                    MechModel.Speed = MechModel.Speed with { X = 0 };

                if (IsOnClimbableVertical() == 1)
                {
                    if (ActionId is not (Action.Climb_Idle_Right or Action.Climb_Idle_Left))
                        ActionId = IsFacingRight ? Action.Climb_Idle_Right : Action.Climb_Idle_Left;
                }
                else if (Scene.GetPhysicalType(Position).IsSolid && ActionId is Action.Fall_Right or Action.Fall_Left)
                {
                    ActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    CameraTargetY = 120;
                    Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, CameraTargetY);
                }
                else if (ActionId is Action.Land_Right or Action.Land_Left && IsActionFinished)
                {
                    ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_EnterLevelCurtain(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = Action.EnterCurtain_Right;

                CameraSideScroller cam = (CameraSideScroller)Scene.Camera;
                cam.HorizontalOffset = CameraOffset.Center;
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_LockedLevelCurtain(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.LockedLevelCurtain_Right : Action.LockedLevelCurtain_Left;
                NextActionId = null;

                if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_RidingWalkingShell(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;

                if (SongAlternation)
                {
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__rocket_BA, 3);
                    SongAlternation = false;
                }
                else
                {
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__rocket, 3);
                    SongAlternation = true;
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                ManageHit();
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__bigtrees, 3);
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_Victory(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PlaySound(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);
                ActionId = IsFacingRight ? Action.Victory_Right : Action.Victory_Left;
                NextActionId = null;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_Determined(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_VeryDetermined_Right : Action.Idle_VeryDetermined_Left;
                NextActionId = null;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_Hide(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                NextActionId = null;
                ActionId = IsFacingRight ? Action.Hidden_Right : Action.Hidden_Left;
                break;

            case FsmAction.Step:
                AnimatedObject.IsFramed = false;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // TODO: Implement all of these
    public bool FUN_080224f4(FsmAction action) => true; // FUN_1004d84c
    public bool FUN_08033404(FsmAction action) => true; // FUN_1005db38
    public bool FUN_1005dea0(FsmAction action) => true;
    public bool FUN_1005dfa4(FsmAction action) => true;
    public bool FUN_1005e04c(FsmAction action) => true;
}