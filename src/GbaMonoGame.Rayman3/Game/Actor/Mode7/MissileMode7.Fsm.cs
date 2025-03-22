using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class MissileMode7
{
    private bool FsmStep_CheckDeath()
    {
        if (BoostTimer != 0)
        {
            BoostTimer--;

            if (BoostTimer == 0)
                CollectedBlueLums = 0;

            if (BoostTimer == 176)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWoHoo_Mix01);
        }

        if (WahooSoundTimer != 0)
        {
            if (WahooSoundTimer == 1)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWoHoo_Mix01);

            WahooSoundTimer--;
        }

        if (JumpSoundTimer != 0)
        {
            if (JumpSoundTimer == 1)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);

            JumpSoundTimer--;
        }

        if (!GameInfo.IsCheatEnabled(Cheat.Invulnerable))
        {
            InvulnerabilityTimer++;

            if (IsInvulnerable && InvulnerabilityTimer > 180)
                IsInvulnerable = false;

            if (Scene.IsHitMainActor(this))
            {
                ReceiveDamage(AttackPoints);
            }
            else
            {
                // Get the current physical type
                MissileMode7PhysicalTypeDefine physicalType = MissileMode7PhysicalTypeDefine.FromPhysicalType(Scene.GetPhysicalType(Position));

                if (physicalType.Damage && State != Fsm_Jump && !IsInvulnerable)
                {
                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__SplshGen_Mix04) &&
                        InstanceId == Scene.Camera.LinkedObject.InstanceId)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SplshGen_Mix04);
                    }

                    ReceiveDamage(1);
                }
            }

            if (HitPoints < PrevHitPoints)
            {
                PrevHitPoints = HitPoints;
                
                if (InstanceId == Scene.Camera.LinkedObject.InstanceId)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoRcvH1_Mix04);

                InvulnerabilityTimer = 0;
                IsInvulnerable = true;
            }

            if (HitPoints == 0 && RSMultiplayer.IsActive)
            {
                State.MoveTo(FUN_08085394);
                return false;
            }

            if (HitPoints == 0)
            {
                State.MoveTo(Fsm_Dying);
                return false;
            }
        }
        else
        {
            IsInvulnerable = true;
        }

        return true;
    }

    public bool Fsm_Start(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                GameInfo.IsInWorldMap = false;
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);
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
                // Do nothing
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                // Get the current physical type
                MissileMode7PhysicalTypeDefine physicalType = MissileMode7PhysicalTypeDefine.FromPhysicalType(Scene.GetPhysicalType(Position));

                if (physicalType.BumperLeft && !PrevPhysicalType.BumperLeft)
                {
                    MechModel.Speed = -new Vector2(MechModel.Speed.Y, MechModel.Speed.X);
                    Direction = Angle256.FromVector(MechModel.Speed * new Vector2(1, -16));

                    if (InstanceId == Scene.Camera.LinkedObject.InstanceId)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Mix02);
                        WahooSoundTimer = 15;
                    }
                }
                else if (physicalType.BumperRight && !PrevPhysicalType.BumperRight) 
                {
                    MechModel.Speed = new Vector2(MechModel.Speed.Y, MechModel.Speed.X);
                    Direction = Angle256.FromVector(MechModel.Speed * new Vector2(1, -16));

                    if (InstanceId == Scene.Camera.LinkedObject.InstanceId)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Mix02);
                        WahooSoundTimer = 15;
                    }

                }

                bool result;
                if (RSMultiplayer.IsActive)
                    result = DoMultiRace();
                else
                    result = DoSingleRace();

                if (result)
                {
                    // Update the animation
                    SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);

                    // Accelerate when holding A
                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.A))
                    {
                        MechModel.Acceleration = Direction.ToDirectionalVector() * Acceleration * new Vector2(1, -1);

                        if (BoostTimer != 0)
                            MechModel.Acceleration *= 2;
                    }
                    // End acceleration
                    else if (MultiJoyPad.IsButtonJustReleased(InstanceId, GbaInput.A))
                    {
                        MechModel.Acceleration = Vector2.Zero;
                    }

                    // Round speed to 0 if too low
                    float minSpeed = MathHelpers.FromFixedPoint(0x40); // 0.0009765625
                    if (MechModel.Speed.X < minSpeed &&
                        MechModel.Speed.X > -minSpeed &&
                        MechModel.Speed.Y < minSpeed &&
                        MechModel.Speed.Y > -minSpeed)
                    {
                        MechModel.Speed = Vector2.Zero;
                    }

                    // Brake when holding B or on damage tiles
                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B) || physicalType.Damage)
                    {
                        float factor = MathHelpers.FromFixedPoint(0xC00); // 0.046875
                        MechModel.Speed -= MechModel.Speed * factor;
                    }
                    // Apply friction
                    else
                    {
                        float factor = MathHelpers.FromFixedPoint(0x500); // 0.01953125
                        MechModel.Speed -= MechModel.Speed * factor;
                    }

                    // Update speed
                    MechModel.Speed += MechModel.Acceleration;

                    // Move to the side with L and R
                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && !MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.R))
                    {
                        MechModel.Speed = MechModel.Speed with { X = MechModel.Speed.X + MechModel.Speed.Y / 64 };
                        MechModel.Speed = MechModel.Speed with { Y = MechModel.Speed.Y - MechModel.Speed.X / 64 };
                    }
                    else if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.R) && !MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L))
                    {
                        MechModel.Speed = MechModel.Speed with { X = MechModel.Speed.X - MechModel.Speed.Y / 64 };
                        MechModel.Speed = MechModel.Speed with { Y = MechModel.Speed.Y + MechModel.Speed.X / 64 };
                    }

                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.Left))
                        Direction += 2;

                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.Right))
                        Direction -= 2;

                    if (CustomScaleTimer == 0)
                    {
                        // Gradually return to normal scale
                        if (1 < Scale.Y)
                            Scale += new Vector2(8, -16) / 256;
                        else
                            Scale = Vector2.One;
                    }
                    else
                    {
                        CustomScaleTimer--;
                    }

                    if (physicalType.Bounce)
                    {
                        State.MoveTo(Fsm_Jump);
                        return false;
                    }
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
                if (InstanceId == Scene.Camera.LinkedObject.InstanceId) 
                {
                    JumpSoundTimer = 15;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Mix02);
                }

                ZPosSpeed = 8;
                ZPosDeacceleration = 0.375f;
                Scale = new Vector2(288, 224) / 256;
                field_0x9c = 1;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                bool result;
                if (RSMultiplayer.IsActive)
                    result = DoMultiRace();
                else
                    result = DoSingleRace();

                if (result)
                {
                    // Update the animation
                    SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);

                    // Update the jump
                    UpdateJump();

                    if (MultiJoyPad.IsButtonJustReleased(InstanceId, GbaInput.A))
                        MechModel.Acceleration = Vector2.Zero;

                    if (ZPos <= 0)
                    {
                        State.MoveTo(Fsm_Default);
                        return false;
                    }
                }
                break;

            case FsmAction.UnInit:
                Scale = new Vector2(192, 284) / 256;
                field_0x9c = 0;
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Motor01_Mix12);
                InvulnerabilityTimer = 0;
                GameInfo.ModifyLives(-1);
                ReceiveDamage(255);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RaDeath_Mix03);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);
                InvulnerabilityTimer++;

                if (InvulnerabilityTimer == 90)
                {
                    ((FrameMode7)Frame.Current).TransitionsFX.FadeOutInit(2 / 16f);
                    ((FrameMode7)Frame.Current).CanPause = false;
                }

                if (InvulnerabilityTimer == 120)
                {
                    if (GameInfo.PersistentInfo.Lives == 0)
                        FrameManager.SetNextFrame(new GameOver());
                    else
                        FrameManager.ReloadCurrentFrame();
                }

                // TODO: The game doesn't have this >30 check and sets ZPos=InvulnerabilityTimer*8
                //       The problem is we calculate the ZPos in 3D while the game does it in screen
                //       coordinates (more or less), which makes us travel upwards at a much slower pace.
                //       This sort of replicates the original look, but not fully (you still move up too fast).
                if (InvulnerabilityTimer > 30)
                {
                    Scale = Vector2.Zero;
                }
                else
                {
                    Scale = Vector2.One + new Vector2(InvulnerabilityTimer * 64, InvulnerabilityTimer * -8) / 256;
                    ZPos = MathF.Pow(InvulnerabilityTimer, 2);
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_FinishedRace(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (!RSMultiplayer.IsActive || InstanceId == Scene.Camera.LinkedObject.InstanceId)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Motor01_Mix12);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);

                    if (RSMultiplayer.IsActive)
                    {
                        // TODO: Implement
                    }
                    else
                    {
                        Scene.Camera.ProcessMessage(this, Message.CamMode7_Spin, true);
                    }
                }

                if (RSMultiplayer.IsActive)
                    InvulnerabilityTimer = 0;
                else
                    InvulnerabilityTimer = 800;
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);

                MechModel.Speed -= MechModel.Speed / 64;

                // Gradually return to normal scale
                if (1 < Scale.Y)
                    Scale += new Vector2(8, -16) / 256;
                else
                    Scale = Vector2.One;

                UpdateJump();

                if (RSMultiplayer.IsActive)
                {
                    // TODO: Implement
                }

                if (RSMultiplayer.IsActive)
                {
                    // TODO: Implement
                }

                InvulnerabilityTimer++;

                if (InvulnerabilityTimer > 974)
                {
                    if (InvulnerabilityTimer == 998)
                    {
                        SoundEventsManager.StopAllSongs();
                    }
                    else if (InvulnerabilityTimer == 1000)
                    {
                        Frame.Current.EndOfFrame = true;

                        if (!RSMultiplayer.IsActive)
                        {
                            if (GameInfo.PersistentInfo.LastCompletedLevel < (int)GameInfo.MapId &&
                                GameInfo.MapId < MapId.Bonus1 &&
                                GameInfo.PersistentInfo.LastCompletedLevel < (int)GameInfo.MapId)
                            {
                                GameInfo.PersistentInfo.LastCompletedLevel = (byte)GameInfo.MapId;
                            }

                            GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.MapId;
                            GameInfo.Save(GameInfo.CurrentSlot);
                        }
                    }
                }
                else if (InvulnerabilityTimer == 974 || 
                         (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.A) && InvulnerabilityTimer > 300))
                {
                    FrameMode7 frame = (FrameMode7)Frame.Current;

                    frame.TransitionsFX.FadeOutInit(2 / 16f);
                    frame.CanPause = false;

                    if (InvulnerabilityTimer < 974)
                        InvulnerabilityTimer = 975;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool FUN_08085394(FsmAction action)
    {
        // TODO: Implement

        return true;
    }
}