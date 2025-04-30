using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: CagoulardDeux
public sealed partial class MetalShieldedHoodboom : InteractableActor
{
    public MetalShieldedHoodboom(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        PrevHitPoints = HitPoints;
        IsHoodboomInvulnerable = false;
        EarlyAttack = false;
        HasBeenHitOnce = false;
        ActiveFists = new RaymanBody[2];
        InvulnerabilityTimer = 0;
        LastHitFistType = -1;

        IsInvulnerable = true;

        State.SetTo(Fsm_Idle);
    }

    public int PrevHitPoints { get; set; }
    public bool IsHoodboomInvulnerable { get; set; }
    public bool EarlyAttack { get; set; } // Unused
    public bool HasBeenHitOnce { get; set; }
    public RaymanBody[] ActiveFists { get; set; }
    public uint Timer { get; set; }
    public uint InvulnerabilityTimer { get; set; }
    public int LastHitFistType { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            // Hit shield
            case Message.Actor_Hit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MetlImp1_PiraHit3_Mix03);
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        CameraActor camera = Scene.Camera;

        bool draw = camera.IsActorFramed(this) || forceDraw;

        // Conditionally don't draw every second frame during invulnerability
        if (draw)
        {
            if (GameTime.ElapsedFrames - InvulnerabilityTimer < 90 &&
                HitPoints != 0 &&
                (GameTime.ElapsedFrames & 1) == 0)
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