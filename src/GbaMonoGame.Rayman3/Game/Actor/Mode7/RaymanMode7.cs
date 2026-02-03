using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class RaymanMode7 : Mode7Actor
{
    public RaymanMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        SamActorId = actorResource.Links[0]!.Value;
        PrevCamAngle = Angle256.Zero;
        
        State.SetTo(_Fsm_Default);

        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 64;
        IsAffine = false; // NOTE: The game doesn't set this, but in the custom Draw implementation it doesn't use affine rendering

        SlowDown = false;
        PrevHitPoints = HitPoints;
        ProcessJoypad = false;
        
        GameInfo.IsInWorldMap = false;
    }

    public float ZPosSpeed { get; set; }
    public float ZPosDeacceleration { get; set; }

    public int SamActorId { get; }
    public bool ProcessJoypad { get; set; }
    public float MoveSpeed { get; set; }
    public bool SlowDown { get; set; }

    public int PrevHitPoints { get; set; }
    public byte InvulnerabilityTimer { get; set; }

    public Angle256 PrevCamAngle { get; set; }

    private void DoNoClipBehavior()
    {
        Position = Scene.GetGameObject(SamActorId).Position;
    }

    private void ScrollClouds()
    {
        TgxPlayfieldMode7 playfield = (TgxPlayfieldMode7)Scene.Playfield;
        TgxCameraMode7 cam = playfield.Camera;

        float x = (PrevCamAngle - cam.Direction.Inverse()).SignedValue;

        // NOTE: The game scrolls by 1 every 4 frames
        x += 0.25f;

        playfield.TextLayers[1].ScrolledPosition += new Vector2(x, 0);

        PrevCamAngle = cam.Direction.Inverse();
    }

    private void UpdateJump(float height)
    {
        TgxPlayfieldMode7 playfield = (TgxPlayfieldMode7)Scene.Playfield;
        TgxCameraMode7 cam = playfield.Camera;

        cam.Horizon = 67 + height / 8;

        float y = 11 - height / 8;

        // The game doesn't do this, but because we use floats we don't want it to go below 0
        if (y < 0)
            y = 0;

        playfield.TextLayers[0].ScrolledPosition = playfield.TextLayers[0].ScrolledPosition with { Y = y };
        playfield.TextLayers[1].ScrolledPosition = playfield.TextLayers[1].ScrolledPosition with { Y = y };
        playfield.TextLayers[2].ScrolledPosition = playfield.TextLayers[2].ScrolledPosition with { Y = y };
        playfield.TextLayers[3].ScrolledPosition = playfield.TextLayers[3].ScrolledPosition with { Y = y };
    }

    // Unused
    private PhysicalType GetCurrentPhysicalType()
    {
        return Scene.GetPhysicalType(Position);
    }

    // Unused
    private bool IsDead()
    {
        return HitPoints == 0 || GetCurrentPhysicalType() == PhysicalTypeValue.InstaKill;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Rayman_CollectMode7YellowLum:
                ((FrameWaterSkiMode7)Frame.Current).UserInfo.LumsBar.AddLums(1);
                return true;

            case Message.Rayman_FinishLevel:
                ProcessJoypad = false;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                LevelMusicManager.HasOverridenLevelMusic = false;
                return true;

            case Message.Actor_Start:
                ProcessJoypad = true;
                return true;

            case Message.RaymanMode7_ShowTextBox:
                // Unused and does nothing in the final game. This used to trigger the tutorial textbox with Murfy.
                return true;

            default:
                return false;
        }
    }

    // Disable collision when debug mode is on
    public override Box GetAttackBox() => Scene.GetGameObject<SamMode7>(SamActorId).Debug_NoClip ? Box.Empty : base.GetAttackBox();
    public override Box GetVulnerabilityBox() => Scene.GetGameObject<SamMode7>(SamActorId).Debug_NoClip ? Box.Empty : base.GetVulnerabilityBox();
    public override Box GetDetectionBox() => Scene.GetGameObject<SamMode7>(SamActorId).Debug_NoClip ? Box.Empty : base.GetDetectionBox();
    public override Box GetActionBox() => Scene.GetGameObject<SamMode7>(SamActorId).Debug_NoClip ? Box.Empty : base.GetActionBox();

    public override void DoBehavior()
    {
        if (Scene.GetGameObject<SamMode7>(SamActorId).Debug_NoClip)
            DoNoClipBehavior();
        else
            base.DoBehavior();
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
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }
}