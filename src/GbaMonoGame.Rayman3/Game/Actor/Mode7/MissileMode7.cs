using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class MissileMode7 : Mode7Actor
{
    public MissileMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Debug.Assert(InstanceId < RSMultiplayer.MaxPlayersCount, "The main actor must be the 4 first game objects");

        if (InstanceId != 0 && InstanceId >= RSMultiplayer.PlayersCount)
            ProcessMessage(this, Message.Destroy);

        if (GameInfo.MapId == MapId.GbaMulti_MissileArena)
        {
            Direction = new Angle256(InstanceId switch
            {
                0 => Angle256.OneEighth * 7,
                1 => Angle256.OneEighth * 5,
                2 => Angle256.OneEighth * 3,
                3 => Angle256.OneEighth * 1,
                _ => throw new Exception("Invalid instance id")
            });
        }

        if (RSMultiplayer.IsActive && InstanceId != 0)
            AnimatedObject.BasePaletteIndex = InstanceId;

        CollectedBlueLums = 0;
        Unused1 = 0;
        Unused2 = 0;
        Scale = Vector2.One;
        RaceDirection = MissileMode7PhysicalTypeDefine.TypeDirection.Right;
        PrevPhysicalType = MissileMode7PhysicalTypeDefine.Empty;
        PrevHitPoints = HitPoints;
        CurrentTempLap = 0;
        BoostTimer = 0;
        Acceleration = 1 / 16f;
        WahooSoundTimer = 0;
        JumpSoundTimer = 0;
        CustomScaleTimer = 0;
        IsJumping = false;

        State.SetTo(Fsm_Start);
    }

    public int PrevHitPoints { get; set; }
    public ushort InvulnerabilityTimer { get; set; }
    
    public byte CollectedBlueLums { get; set; }
    public byte BoostTimer { get; set; }
    public byte WahooSoundTimer { get; set; }
    public byte JumpSoundTimer { get; set; }

    public bool IsJumping { get; set; }

    public float ZPosSpeed { get; set; }
    public float ZPosDeacceleration { get; set; }
    public float Acceleration { get; set; }
    
    public ushort MultiplayerDeathTimer { get; set; } // NOTE: In the original game it reuses the Acceleration value
    public bool MultiplayerDeathFadeFlag { get; set; } // NOTE: In the original game it reuses the IsOnCorrectLap value
    public int MultiplayerDeathSpectatePlayer { get; set; } // NOTE: In the original game it reuses the BoostTimer value

    public Vector2 Scale { get; set; }
    public byte CustomScaleTimer { get; set; }

    public MissileMode7PhysicalTypeDefine PrevPhysicalType { get; set; }
    public MissileMode7PhysicalTypeDefine.TypeDirection RaceDirection { get; set; }

    public byte CurrentTempLap { get; set; }
    public bool IsOnCorrectLap { get; set; }

    // Unused
    public byte Unused1 { get; set; }
    public byte Unused2 { get; set; }

    public bool Debug_NoClip { get; set; } // Custom no-clip mode

    private bool IsMovingTheRightDirection(MissileMode7PhysicalTypeDefine.TypeDirection raceDirection)
    {
        return raceDirection switch
        {
            MissileMode7PhysicalTypeDefine.TypeDirection.Right => Speed.X >= 0,
            MissileMode7PhysicalTypeDefine.TypeDirection.UpRight => Speed.X >= Speed.Y,
            MissileMode7PhysicalTypeDefine.TypeDirection.Up => Speed.Y <= 0,
            MissileMode7PhysicalTypeDefine.TypeDirection.UpLeft => Speed.X <= -Speed.Y,
            MissileMode7PhysicalTypeDefine.TypeDirection.Left => Speed.X <= 0,
            MissileMode7PhysicalTypeDefine.TypeDirection.DownLeft => Speed.Y >= Speed.X,
            MissileMode7PhysicalTypeDefine.TypeDirection.Down => Speed.Y >= 0,
            MissileMode7PhysicalTypeDefine.TypeDirection.DownRight => Speed.X >= -Speed.Y,
            _ => true
        };
    }

    private bool IsFacingTheRightDirection(MissileMode7PhysicalTypeDefine.TypeDirection raceDirection)
    {
        Angle256 v1 = (int)raceDirection * 32 + 80;
        Angle256 v2 = (int)raceDirection * 32 + (Angle256.Max - 80);
        
        if (v1 < v2)
            return v1 > Direction || Direction > v2;
        else
            return v1 > Direction && Direction > v2;
    }

    private void UpdateJump()
    {
        float zPos = ZPos + ZPosSpeed;
        ZPosSpeed -= ZPosDeacceleration;
        
        if (zPos <= 0)
        {
            ZPos = 0;
            ZPosSpeed = 0;
            ZPosDeacceleration = 0;
        }
        else
        {
            ZPos = zPos;
        }
    }

    private void RestoreScale()
    {
        // Gradually return to normal scale
        if (1 < Scale.Y)
            Scale += new Vector2(8, -16) / 256;
        else
            Scale = Vector2.One;
    }

    private bool DoSingleRace()
    {
        FrameSingleMode7 frame = (FrameSingleMode7)Frame.Current;
        RaceManager raceManager = frame.RaceManager;

        // Get the current physical type
        MissileMode7PhysicalTypeDefine physicalType = MissileMode7PhysicalTypeDefine.FromPhysicalType(Scene.GetPhysicalType(Position));

        if (physicalType.Directional && !PrevPhysicalType.Directional)
            RaceDirection = physicalType.Direction;

        bool isMovingTheRightDirection = IsMovingTheRightDirection(RaceDirection);

        float pitch = Speed.Length() * 512;
        if (IsJumping)
            pitch += 1024;
        SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__Motor01_Mix12, pitch);

        // Update if we're facing the right direction
        raceManager.DrivingTheRightWay = IsFacingTheRightDirection(RaceDirection);

        // Check if we passed the finish line
        if (physicalType.RaceEnd != PrevPhysicalType.RaceEnd)
        {
            if (isMovingTheRightDirection)
            {
                CurrentTempLap++;

                if ((CurrentTempLap & 1) == 0 && IsOnCorrectLap)
                {
                    raceManager.CurrentTempLap++;

                    bool finishedRace;

                    // New lap
                    if (raceManager.CurrentLap < raceManager.CurrentTempLap)
                    {
                        if (raceManager.CurrentTempLap == 2)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LineFX01_Mix02_P1_);
                        else if (raceManager.CurrentTempLap == 3)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LineFX01_Mix02_P2_);
                        else if (raceManager.CurrentTempLap == 4)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);

                        raceManager.CurrentLap = raceManager.CurrentTempLap;

                        // Initialize next lap
                        if (raceManager.CurrentLap <= raceManager.LapsCount)
                        {
                            raceManager.RemainingTime = raceManager.LapTimes[raceManager.CurrentLap - 1] * 60;
                            finishedRace = false;
                        }
                        // The race has been finished
                        else
                        {
                            raceManager.CurrentLap = raceManager.LapsCount;
                            raceManager.IsRacing = false;
                            finishedRace = true;
                        }
                    }
                    else
                    {
                        finishedRace = false;
                    }

                    if (finishedRace)
                    {
                        frame.SaveLums();
                        State.MoveTo(Fsm_FinishedRace);
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                        LevelMusicManager.HasOverridenLevelMusic = false;
                    }
                }

                IsOnCorrectLap = true;
            }
            else
            {
                CurrentTempLap--;
                
                if ((CurrentTempLap & 1) == 0 && !IsOnCorrectLap)
                    raceManager.CurrentTempLap--;

                IsOnCorrectLap = false;
            }
        }

        PrevPhysicalType = physicalType;

        return true;
    }

    private bool DoMultiRace()
    {
        FrameMissileMultiMode7 frame = (FrameMissileMultiMode7)Frame.Current;
        RaceManagerMulti raceManager = frame.RaceManager;

        // Get the current physical type
        MissileMode7PhysicalTypeDefine physicalType = MissileMode7PhysicalTypeDefine.FromPhysicalType(Scene.GetPhysicalType(Position));

        if (physicalType.Directional && !PrevPhysicalType.Directional)
            RaceDirection = physicalType.Direction;

        bool isMovingTheRightDirection = IsMovingTheRightDirection(RaceDirection);

        if (IsLinkedCameraObject())
        {
            float pitch = Speed.Length() * 512;
            if (IsJumping)
                pitch += 256;
            SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__Motor01_Mix12, pitch);

            // Update if we're facing the right direction
            raceManager.DrivingTheRightWay = IsFacingTheRightDirection(RaceDirection);
        }

        if (physicalType.Directional != PrevPhysicalType.Directional)
        {
            if (isMovingTheRightDirection)
                raceManager.IncDistance(InstanceId);
            else
                raceManager.DecDistance(InstanceId);
        }

        // Check if we passed the finish line
        if (physicalType.RaceEnd != PrevPhysicalType.RaceEnd)
        {
            if (isMovingTheRightDirection)
            {
                CurrentTempLap++;

                if ((CurrentTempLap & 1) == 0 && IsOnCorrectLap)
                {
                    raceManager.PlayersCurrentTempLap[InstanceId]++;

                    bool finishedRace;

                    // Finish race
                    if (raceManager.LapsCount < raceManager.PlayersCurrentTempLap[InstanceId])
                    {
                        raceManager.PlayersCurrentLap[InstanceId] = raceManager.LapsCount;
                        
                        int v = 0;

                        for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                        {
                            if (raceManager.LapsCount < raceManager.PlayersCurrentTempLap[i])
                                v += 1;
                        }

                        raceManager.PlayerDistances[InstanceId] = 2000 - v;

                        if (InstanceId == MultiplayerManager.MachineId)
                        {
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);
                            raceManager.IsRacing = false;
                        }

                        finishedRace = true;
                    }
                    // Finish lap
                    else
                    {
                        // New lap
                        if (raceManager.PlayersCurrentLap[InstanceId] < raceManager.PlayersCurrentTempLap[InstanceId])
                        {
                            if (InstanceId == MultiplayerManager.MachineId)
                            {
                                if (raceManager.PlayersCurrentTempLap[InstanceId] == 2)
                                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LineFX01_Mix02_P1_);
                                else if (raceManager.PlayersCurrentTempLap[InstanceId] == 3)
                                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LineFX01_Mix02_P2_);
                            }

                            raceManager.PlayersLastLapRaceTime[InstanceId] = raceManager.RaceTime;
                            raceManager.PlayersCurrentLap[InstanceId] = raceManager.PlayersCurrentTempLap[InstanceId];
                        }

                        finishedRace = false;
                    }

                    if (finishedRace)
                        State.MoveTo(Fsm_FinishedRace);

                    raceManager.IncDistance(InstanceId);
                }

                IsOnCorrectLap = true;
            }
            else
            {
                CurrentTempLap--;

                if ((CurrentTempLap & 1) == 0 && !IsOnCorrectLap)
                {
                    raceManager.PlayersCurrentTempLap[InstanceId]--;
                    raceManager.DecDistance(InstanceId);
                }

                IsOnCorrectLap = false;
            }
        }

        PrevPhysicalType = physicalType;

        Acceleration = MathHelpers.FromFixedPoint((raceManager.PlayerDistances[raceManager.PlayerRanks[0]] - raceManager.PlayerDistances[InstanceId]) * 0x80 + 0x1000);
        const float max = 5 / 64f;
        if (Acceleration > max)
            Acceleration = max;

        return true;
    }

    private Vector2 UpdateCollidedPosition(Vector2 pos, Vector2 speed, Box actorDetectionBox, Box otherDetectionBox)
    {
        if (!Box.Intersect(actorDetectionBox, otherDetectionBox, out Box intersectBox))
            return pos;

        if (speed.Y > 0 &&
            actorDetectionBox.Top < otherDetectionBox.Top &&
            actorDetectionBox.Bottom < otherDetectionBox.Bottom &&
            intersectBox.Height < 8)
        {
            pos -= new Vector2(0, intersectBox.Height);
        }
        else if (speed.Y < 0 && 
                 actorDetectionBox.Bottom > otherDetectionBox.Bottom && 
                 actorDetectionBox.Top > otherDetectionBox.Top)
        {
            pos += new Vector2(0, intersectBox.Height);
        }
        else if (speed.X > 0 && 
                 actorDetectionBox.Right < otherDetectionBox.Right)
        {
            pos -= new Vector2(intersectBox.Width, 0);
        }
        else if (speed.X < 0 && 
                 actorDetectionBox.Left > otherDetectionBox.Left)
        {
            pos += new Vector2(intersectBox.Width, 0);
        }

        // NOTE: The original engine casts the position to an integer here (floor if positive, ceil if negative)
        return pos;
    }

    private void ToggleNoClip()
    {
        if (Engine.Config.DebugModeEnabled && InputManager.IsButtonJustPressed(Input.Debug_ToggleNoClip))
        {
            Debug_NoClip = !Debug_NoClip;

            if (Debug_NoClip)
                MechModel.Speed = Vector2.Zero;
            else
                State.MoveTo(Fsm_Default);
        }
    }

    private void DoNoClipBehavior()
    {
        Vector2 direction = Direction.ToDirectionalVector().FlipY();
        Vector2 sideDirection = (Direction + Angle256.Quarter).ToDirectionalVector().FlipY();

        int speed = JoyPad.IsButtonPressed(GbaInput.A) ? 4 : 2;

        if (JoyPad.IsButtonPressed(GbaInput.Up))
            Position += direction * speed;
        if (JoyPad.IsButtonPressed(GbaInput.Down))
            Position -= direction * speed;

        if (JoyPad.IsButtonPressed(GbaInput.Right))
            Position -= sideDirection * speed;
        if (JoyPad.IsButtonPressed(GbaInput.Left))
            Position += sideDirection * speed;

        if (JoyPad.IsButtonPressed(GbaInput.R))
            Direction--;
        if (JoyPad.IsButtonPressed(GbaInput.L))
            Direction++;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Rayman_CollectMode7BlueLum:
                if (CollectedBlueLums < 3)
                {
                    CollectedBlueLums++;

                    // Play sound
                    if (!RSMultiplayer.IsActive || IsLinkedCameraObject())
                    {
                        if (CollectedBlueLums == 1)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumBoost_Mix01GEN_P1);
                        else if (CollectedBlueLums == 2)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumBoost_Mix01GEN_P2);
                        else if (CollectedBlueLums == 3)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumBoost_Mix01GEN_P3);
                    }
                }

                // 3 blue lums initiate the boost
                if (CollectedBlueLums == 3)
                    BoostTimer = 192;
                return true;

            case Message.Rayman_CollectRedLum:
                if (HitPoints < 5)
                    HitPoints++;

                PrevHitPoints = HitPoints;
                return true;

            case Message.Rayman_CollectMode7YellowLum:
                ((FrameSingleMode7)Frame.Current).UserInfo.LumsBar.AddLums(1);
                return true;

            case Message.MissileMode7_StartRace:
                State.MoveTo(Fsm_Default);

                if (!RSMultiplayer.IsActive || IsLinkedCameraObject())
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Motor01_Mix12);
                return true;

            case Message.MissileMode7_EndRace:
                State.MoveTo(Fsm_FinishedRace);
                return true;

            default:
                return false;
        }
    }

    // Disable collision when debug mode is on
    public override Box GetAttackBox() => Debug_NoClip ? Box.Empty : base.GetAttackBox();
    public override Box GetVulnerabilityBox() => Debug_NoClip ? Box.Empty : base.GetVulnerabilityBox();
    public override Box GetDetectionBox() => Debug_NoClip ? Box.Empty : base.GetDetectionBox();
    public override Box GetActionBox() => Debug_NoClip ? Box.Empty : base.GetActionBox();

    public override void DoBehavior()
    {
        if (Debug_NoClip)
            DoNoClipBehavior();
        else
            base.DoBehavior();
    }

    public override void Step()
    {
        // Check for collision with other karts
        if (RSMultiplayer.IsActive && InstanceId == 0)
        {
            for (int id1 = 0; id1 < MultiplayerManager.PlayersCount; id1++)
            {
                MissileMode7 actor1 = Scene.GetGameObject<MissileMode7>(id1);
                Box actor1ViewBox = actor1.GetViewBox();

                for (int id2 = id1; id2 < MultiplayerManager.PlayersCount; id2++)
                {
                    MissileMode7 actor2 = Scene.GetGameObject<MissileMode7>(id2);
                    Box actor2ViewBox = actor2.GetViewBox();

                    if (actor1ViewBox.Intersects(actor2ViewBox) && Math.Abs(actor1.ZPos - actor2.ZPos) < 16)
                    {
                        Angle256 angle = MathHelpers.Atan2_256((actor2.Position - actor1.Position).FlipY());
                        angle += Angle256.Half;
                        Vector2 directionalVector = angle.ToDirectionalVector().FlipY();

                        Vector2 speedDiff = actor1.Speed - actor2.Speed;
                        
                        float force;
                        if (GameInfo.MapId == MapId.GbaMulti_MissileArena)
                            force = (directionalVector.X * speedDiff.X + directionalVector.Y * speedDiff.Y) * 2;
                        else
                            force = directionalVector.X * speedDiff.X + directionalVector.Y * speedDiff.Y;

                        if (force < 0)
                        {
                            actor1.MechModel.Speed -= directionalVector * force;
                            actor1.MechModel.Acceleration = Vector2.Zero;

                            actor2.MechModel.Speed += directionalVector * force;
                            actor2.MechModel.Acceleration = Vector2.Zero;

                            if (actor1.IsLinkedCameraObject() || actor2.IsLinkedCameraObject()) 
                            {
                                if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PinBall_Mix02)) 
                                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Mix02);
                            }
                        }

                        actor1.Position = UpdateCollidedPosition(actor1.Position, actor1.Speed, actor1.GetDetectionBox(), actor2.GetDetectionBox());
                        actor2.Position = UpdateCollidedPosition(actor2.Position, actor2.Speed, actor2.GetDetectionBox(), actor1.GetDetectionBox());
                    }
                }
            }
        }

        base.Step();
        
        if (IsLinkedCameraObject())
            ToggleNoClip();
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        CameraActor camera = Scene.Camera;

        bool draw = camera.IsActorFramed(this) || forceDraw;

        // Conditionally don't draw every second frame during invulnerability
        if (draw)
        {
            if (IsInvulnerable &&
                HitPoints != 0 &&
                (GameTime.ElapsedFrames & 1) == 0 &&
                !GameInfo.IsCheatEnabled(Cheat.Invulnerable))
            {
                draw = false;
            }
        }

        if (draw)
        {
            AnimatedObject.AffineMatrix = new AffineMatrix(0, Scale.X, Scale.Y);
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }

        if (!IsTouchingMap)
        {
            MechModel.Speed = Speed;
            
            if (Speed.X > 7)
                Speed = Speed with { X = 7 };
            if (Speed.Y > 7)
                Speed = Speed with { Y = 7 };
        }
    }
}