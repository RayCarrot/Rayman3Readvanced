using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class FlyingShell : MovableActor
{
    public FlyingShell(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        CrashTimer = 0;
        Ammo = 0;

        State.SetTo(_Fsm_Init);
    }

    public EnergyBall EnergyBall { get; set; }
    public byte CrashTimer { get; set; }
    public byte EndTimer { get; set; }
    public int Ammo { get; set; } // Unused

    public bool Debug_NoClip { get; set; } // Custom no-clip mode

    private void UpdateSoundPitch()
    {
        SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__Motor01_Mix12, 192 + (160 - Position.Y) * 8);
    }

    private bool IsCollidingWithWall()
    {
        Box detectionBox = GetDetectionBox();
        detectionBox.Bottom -= Tile.Size;

        if (Scene.GetPhysicalType(detectionBox.BottomRight) == PhysicalTypeValue.InstaKill) 
            return true;
        
        if (Scene.GetPhysicalType(detectionBox.MiddleRight) == PhysicalTypeValue.InstaKill) 
            return true;
        
        if (Scene.GetPhysicalType(detectionBox.TopRight) == PhysicalTypeValue.InstaKill) 
            return true;
        
        if (Scene.GetPhysicalType(detectionBox.TopCenter) == PhysicalTypeValue.InstaKill) 
            return true;
        
        if (Scene.GetPhysicalType(detectionBox.TopLeft) == PhysicalTypeValue.InstaKill) 
            return true;
        
        if (Scene.GetPhysicalType(detectionBox.MiddleLeft) == PhysicalTypeValue.InstaKill) 
            return true;
        
        if (Scene.GetPhysicalType(detectionBox.BottomLeft) == PhysicalTypeValue.InstaKill) 
            return true;
        
        if (Scene.GetPhysicalType(detectionBox.BottomCenter) == PhysicalTypeValue.InstaKill) 
            return true;
        
        return false;
    }

    // Unused
    private void CreateEnergyBall()
    {
        if (EnergyBall == null)
        {
            // But it's null???
            EnergyBall.MechModel.Speed = Vector2.Zero;
            EnergyBall.ActionId = EnergyBall.Action.Shot1Enemy_Right;

            EnergyBall = Scene.CreateProjectile<EnergyBall>(ActorType.EnergyBall);
        }

        if (EnergyBall != null)
        {
            if (Speed.X > 0)
            {
                EnergyBall.Position = Position + new Vector2(32, 0);

                if (ActionId != Action.FlyUp_Right)
                    EnergyBall.ActionId = EnergyBall.Action.Shot1Enemy_Right;
            }
            else
            {
                EnergyBall.Position = Position - new Vector2(32, 0);

                if (ActionId != Action.FlyUp_Left)
                    EnergyBall.ActionId = EnergyBall.Action.Shot1Enemy_Left;
            }

            EnergyBall.ChangeAction();
        }
    }

    // Unused
    private void FireEnergyBall()
    {
        if (EnergyBall != null)
        {
            Ammo--;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser3_Mix03);
            
            if (Speed.X <= 0)
            {
                EnergyBall.ActionId = EnergyBall.Action.Shot1Enemy_Left;
                EnergyBall.MechModel.Speed = new Vector2(-4, 0);
            }
            else
            {
                EnergyBall.ActionId = EnergyBall.Action.Shot1Enemy_Right;
                EnergyBall.MechModel.Speed = new Vector2(4, 0);
            }

            if (EnergyBall != null)
                EnergyBall.ChangeAction();
        }
    }

    private void ToggleNoClip()
    {
        if (Engine.ActiveConfig.Debug.DebugModeEnabled && InputManager.IsInputJustPressed(Input.Debug_ToggleNoClip))
        {
            Debug_NoClip = !Debug_NoClip;

            if (Debug_NoClip)
            {
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                ChangeAction();
                MechModel.Speed = Vector2.Zero;
            }
            else
            {
                State.MoveTo(_Fsm_Fly);
                ChangeAction();
            }
        }
    }

    private void DoNoClipBehavior()
    {
        int speed = JoyPad.IsButtonPressed(Rayman3Input.ActorJump) ? 7 : 4;

        if (JoyPad.IsButtonPressed(Rayman3Input.ActorUp))
            Position -= new Vector2(0, speed);
        else if (JoyPad.IsButtonPressed(Rayman3Input.ActorDown))
            Position += new Vector2(0, speed);

        if (JoyPad.IsButtonPressed(Rayman3Input.ActorLeft))
            Position -= new Vector2(speed, 0);
        else if (JoyPad.IsButtonPressed(Rayman3Input.ActorRight))
            Position += new Vector2(speed, 0);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Rayman_FinishLevel:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Motor01_Mix12);

                if (TimeAttackInfo.IsActive)
                {
                    TimeAttackInfo.Pause();
                    TimeAttackInfo.SetMode(TimeAttackMode.Score);
                }
                else if (Rom.Platform == Platform.GBA && GameInfo.LevelType == LevelType.GameCube)
                {
                    State.MoveTo(_Fsm_GameCubeEndMap);
                }
                else
                {
                    Frame.Current.EndOfFrame = true;

                    if (Engine.ActiveConfig.Tweaks.FixBugs)
                    {
                        if (GameInfo.IsFirstTimeCompletingLevel())
                            GameInfo.UpdateLastCompletedLevel();

                        GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.MapId;
                        GameInfo.Save(GameInfo.CurrentSlot);
                    }
                }
                return false;

            case Message.Actor_Hurt:
                if (State != _Fsm_Crash)
                    State.MoveTo(_Fsm_Crash);
                return false;

            case Message.Rayman_Stop:
                State.MoveTo(_Fsm_Stop);
                return false;

            case Message.Rayman_Resume:
                if (State != _Fsm_Crash)
                    State.MoveTo(_Fsm_Fly);
                return false;

            case Message.FlyingShell_RefillAmmo:
                Ammo = 3;
                return false;

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
        base.Step();

        if (Engine.ActiveConfig.Difficulty.OneHitPoint && HitPoints > 1)
            HitPoints = 1;

        if (IsLinkedCameraObject())
            ToggleNoClip();
    }
}