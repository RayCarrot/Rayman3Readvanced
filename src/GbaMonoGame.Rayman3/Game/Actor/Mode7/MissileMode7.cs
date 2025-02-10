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

        field_0x8d = 0;
        field_0x8c = 0;
        field_0x88 = 0;
        field_0x92 = 256;
        field_0x94 = 256;
        field_0x8a = 0;
        field_0x89 = Mode7PhysicalTypeDefine.Empty;
        PrevHitPoints = HitPoints;
        field_0x8e = 0;
        BoostTimer = 0;
        field_0x96 = 0.0625f;
        field_0x99 = 0;
        field_0x9a = 0;
        field_0x9b = 0;
        field_0x9c = 0;

        State.SetTo(Fsm_Start);
    }

    public int PrevHitPoints { get; set; }
    public byte BoostTimer { get; set; }
    public float ZPosSpeed { get; set; }
    public float ZPosDeacceleration { get; set; }

    // TODO: Name
    public byte field_0x8d { get; set; }
    public byte field_0x8c { get; set; }
    public byte field_0x88 { get; set; }
    public ushort field_0x92 { get; set; }
    public ushort field_0x94 { get; set; }
    public byte field_0x8a { get; set; }
    public Mode7PhysicalTypeDefine field_0x89 { get; set; }
    public byte field_0x8e { get; set; }
    public float field_0x96 { get; set; }
    public byte field_0x99 { get; set; }
    public byte field_0x9a { get; set; }
    public byte field_0x9b { get; set; }
    public byte field_0x9c { get; set; }

    public bool Debug_NoClip { get; set; } // Custom no-clip mode

    private void SetMode7DirectionalAction()
    {
        SetMode7DirectionalAction(0, 6);
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
                // TODO: Implement
                return true;

            case Message.MissileMode7_CollectedRedLum:
                if (HitPoints < 5)
                    HitPoints++;

                PrevHitPoints = HitPoints;
                return true;

            case Message.MissileMode7_CollectedYellowLum:
                ((FrameSingleMode7)Frame.Current).UserInfo.LumsBar.AddLums(1);
                return true;

            case Message.MissileMode7_StartRace:
                State.MoveTo(Fsm_Default);

                if (!RSMultiplayer.IsActive || InstanceId == Scene.Camera.LinkedObject.InstanceId)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Motor01_Mix12);
                return true;

            case Message.MissileMode7_1074:
                // TODO: Implement
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

    // TODO: Implement
    public override void Step()
    {
        base.Step();

        ToggleNoClip();
    }

    // TODO: Implement
    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        base.Draw(animationPlayer, forceDraw);
    }
}