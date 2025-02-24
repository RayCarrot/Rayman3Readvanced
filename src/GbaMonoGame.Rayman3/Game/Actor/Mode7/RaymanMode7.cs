using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class RaymanMode7 : Mode7Actor
{
    public RaymanMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        SamActorId = actorResource.Links[0]!.Value;
        field_0x7b = 0;
        
        State.SetTo(Fsm_Default);

        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 64;
        IsAffine = false; // NOTE: The game doesn't set this, but in the custom Draw implementation it doesn't use affine rendering

        field_0x7a = Direction;
        SlowDown = false;
        PrevHitPoints = HitPoints;
        ProcessJoypad = false;
        
        GameInfo.IsInWorldMap = false;
    }

    // TODO: Name
    public Angle256 field_0x7a { get; set; }
    public byte field_0x7b { get; set; }

    public float ZPosSpeed { get; set; }
    public float ZPosDeacceleration { get; set; }

    public int SamActorId { get; }
    public bool ProcessJoypad { get; set; }
    public float MoveSpeed { get; set; }
    public bool SlowDown { get; set; }

    public int PrevHitPoints { get; set; }
    public byte InvulnerabilityTimer { get; set; }

    private void DoNoClipBehavior()
    {
        Position = Scene.GetGameObject(SamActorId).Position;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.MainMode7_CollectedYellowLum:
                ((FrameWaterSkiMode7)Frame.Current).UserInfo.LumsBar.AddLums(1);
                return true;

            case Message.Main_LevelEnd:
                ProcessJoypad = false;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                LevelMusicManager.HasOverridenLevelMusic = false;
                return true;

            case Message.MainMode7_LevelStart:
                ProcessJoypad = true;
                return true;

            case Message.MainMode7_ShowTextBox:
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
                (GameInfo.Cheats & Cheat.Invulnerable) == 0)
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