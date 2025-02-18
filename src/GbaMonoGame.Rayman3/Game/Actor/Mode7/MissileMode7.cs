using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class MissileMode7 : Mode7Actor
{
    public MissileMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        if (InstanceId != 0 && InstanceId >= RSMultiplayer.PlayersCount)
            ProcessMessage(this, Message.Destroy);

        if (GameInfo.MapId == MapId.GbaMulti_MissileArena)
        {
            Direction = InstanceId switch
            {
                0 => 224,
                1 => 160,
                2 => 96,
                3 => 32,
                _ => throw new Exception("Invalid instance id")
            };
        }

        if (RSMultiplayer.IsActive && InstanceId != 0)
            AnimatedObject.BasePaletteIndex = InstanceId;

        CollectedBlueLums = 0;
        field_0x8c = 0;
        field_0x88 = 0;
        Scale = Vector2.One;
        RaceDirection = Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.Right;
        PrevPhysicalType = Mode7PhysicalTypeDefine.Empty;
        PrevHitPoints = HitPoints;
        CurrentTempLap = 0;
        BoostTimer = 0;
        Acceleration = 0.0625f;
        WahooSoundTimer = 0;
        JumpSoundTimer = 0;
        CustomScaleTimer = 0;
        field_0x9c = 0;

        State.SetTo(Fsm_Start);
    }

    public int PrevHitPoints { get; set; }
    public ushort InvulnerabilityTimer { get; set; }
    
    public byte CollectedBlueLums { get; set; }
    public byte BoostTimer { get; set; }
    public byte WahooSoundTimer { get; set; }
    public byte JumpSoundTimer { get; set; }

    public float ZPosSpeed { get; set; }
    public float ZPosDeacceleration { get; set; }
    public float Acceleration { get; set; }

    public Vector2 Scale { get; set; }
    public byte CustomScaleTimer { get; set; }

    public Mode7PhysicalTypeDefine PrevPhysicalType { get; set; }
    public Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection RaceDirection { get; set; }

    public byte CurrentTempLap { get; set; }
    public bool IsOnCorrectLap { get; set; }

    // TODO: Name
    public byte field_0x8c { get; set; }
    public byte field_0x88 { get; set; }
    public byte field_0x9c { get; set; }

    public bool Debug_NoClip { get; set; } // Custom no-clip mode

    private void SetMode7DirectionalAction()
    {
        SetMode7DirectionalAction(0, 6);
    }

    private bool IsMovingTheRightDirection(Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection raceDirection)
    {
        return raceDirection switch
        {
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.Right => Speed.X >= 0,
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.UpRight => Speed.X >= Speed.Y,
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.Up => Speed.Y <= 0,
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.UpLeft => Speed.X <= -Speed.Y,
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.Left => Speed.X <= 0,
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.DownLeft => Speed.Y >= Speed.X,
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.Down => Speed.Y >= 0,
            Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection.DownRight => Speed.X >= -Speed.Y,
            _ => true
        };
    }

    private bool IsFacingTheRightDirection(Mode7PhysicalTypeDefine.Mode7PhysicalTypeDirection raceDirection)
    {
        float v1 = (int)raceDirection * 32 + 80;
        float v2 = (int)raceDirection * 32 + 176;
        
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

    private bool DoSingleRace()
    {
        FrameSingleMode7 frame = (FrameSingleMode7)Frame.Current;
        RaceManager raceManager = frame.RaceManager;

        // Get the current physical type
        Mode7PhysicalTypeDefine physicalType = Scene.GetPhysicalType(Position).Mode7Define;

        if (physicalType.Directional && !PrevPhysicalType.Directional)
            RaceDirection = physicalType.Direction;

        bool isMovingTheRightDirection = IsMovingTheRightDirection(RaceDirection);

        // TODO: Update the sound pitch based on the speed
        //SoundEventsManager.SetSoundPitch();

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
        // TODO: Implement
        return true;
    }

    private void ToggleNoClip()
    {
        if (InputManager.IsButtonJustPressed(Input.Debug_ToggleNoClip))
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
        Vector2 direction = MathHelpers.DirectionalVector256(Direction) * new Vector2(1, -1);
        Vector2 sideDirection = MathHelpers.DirectionalVector256(Direction + 64) * new Vector2(1, -1);

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
            case Message.MissileMode7_CollectedBlueLum:
                if (CollectedBlueLums < 3)
                {
                    CollectedBlueLums++;

                    // Play sound
                    if (!RSMultiplayer.IsActive || InstanceId == Scene.Camera.LinkedObject.InstanceId)
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

            case Message.MissileMode7_CollectedRedLum:
                if (HitPoints < 5)
                    HitPoints++;

                PrevHitPoints = HitPoints;
                return true;

            case Message.MainMode7_CollectedYellowLum:
                ((FrameSingleMode7)Frame.Current).UserInfo.LumsBar.AddLums(1);
                return true;

            case Message.MissileMode7_StartRace:
                State.MoveTo(Fsm_Default);

                if (!RSMultiplayer.IsActive || InstanceId == Scene.Camera.LinkedObject.InstanceId)
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
        if (RSMultiplayer.IsActive)
        {
            // TODO: Implement
        }

        base.Step();
        
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
                (GameInfo.Cheats & Cheat.Invulnerable) == 0)
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