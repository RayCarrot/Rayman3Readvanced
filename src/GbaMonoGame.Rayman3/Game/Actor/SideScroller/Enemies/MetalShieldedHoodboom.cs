using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: CagoulardDeux
public sealed partial class MetalShieldedHoodboom : InteractableActor
{
    public MetalShieldedHoodboom(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene,
        actorResource)
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
        // The ame implements this as a custom function for this class, but it's just the same as
        // the standard invulnerability drawing function but with it checking the timer instead
        DrawWithInvulnerability(animationPlayer, forceDraw, GameTime.ElapsedFrames - InvulnerabilityTimer < 90);
    }
}