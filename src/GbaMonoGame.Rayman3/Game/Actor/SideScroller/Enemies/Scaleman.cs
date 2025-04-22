using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: There's a visual bug with the shadow when the Scaleman fall down. The last frame has a 1-pixel gap between the sprites due to the scaling.
public sealed partial class Scaleman : MovableActor
{
    public Scaleman(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource)
    {
        IsInvulnerable = true;

        ScalemanShadow = null;
        RedLum = null;
        Timer = 0;
        AirAttackTimer = 0;
        HitTimer = 101;
        CenterCamera = true;

        State.SetTo(Fsm_PreInit);
    }

    public ScalemanShadow ScalemanShadow { get; set; }
    public Lums RedLum { get; set; }
    public ushort Timer { get; set; }
    public ushort AirAttackTimer { get; set; }
    public ushort HitTimer { get; set; }
    public bool CenterCamera { get; set; } // Always true

    private bool IsSecondPhase() => HitPoints <= 3;

    private void CreateRedLum()
    {
        RedLum = Scene.CreateProjectile<Lums>(ActorType.Lums);
        if (RedLum != null)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Sparkles_Mix01);
            RedLum.ProcessMessage(this, Message.Lum_ToggleVisibility);
            RedLum.AnimatedObject.CurrentAnimation = 3;
            RedLum.ActionId = Lums.Action.RedLum;
            RedLum.Position = Position - new Vector2(0, 80);
        }
    }

    private void SpawnRedLum()
    {
        RedLum?.ProcessMessage(this, Message.Lum_ToggleVisibility);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // This is one of the few actors which actually returns true in here

        if (base.ProcessMessageImpl(sender, message, param))
            return true;

        switch (message)
        {
            case Message.Actor_Hit:
                RaymanBody raymanBody = (RaymanBody)param;

                // Shrink when hit while big
                if (ActionId is Action.Idle_Right or Action.Idle_Left ||
                    (ActionId is Action.Emerge_Right or Action.Emerge_Left && AnimatedObject.CurrentFrame >= 8))
                {
                    if (raymanBody.BodyPartType is RaymanBody.RaymanBodyPartType.Fist or RaymanBody.RaymanBodyPartType.SecondFist)
                    {
                        if (Position.X < raymanBody.Position.X)
                            ActionId = IsFacingRight ? Action.Hit_Right : Action.HitBehind_Left;
                        else
                            ActionId = IsFacingRight ? Action.HitBehind_Right : Action.Hit_Left;

                        State.MoveTo(Fsm_Shrink);

                        ChangeAction();

                        if (ScalemanShadow != null)
                        {
                            ScalemanShadow.ProcessMessage(this, Message.Destroy);
                            ScalemanShadow = null;
                        }
                    }
                }
                // Take damage when hit while small
                else if (ActionId is 
                         Action.Small_Idle_Right or Action.Small_Idle_Left or 
                         Action.Small_Run_Right or Action.Small_Run_Left or 
                         Action.Small_RunFast_Right or Action.Small_RunFast_Left or
                         Action.Small_ChangeDirection_Right or Action.Small_ChangeDirection_Left or
                         Action.Small_Hop_Right or Action.Small_Hop_Left)
                {
                    if (raymanBody.BodyPartType == RaymanBody.RaymanBodyPartType.Torso)
                    {
                        ((FrameSideScroller)Frame.Current).UserInfo.BossHit();
                        State.MoveTo(Fsm_SmallHit);
                        ChangeAction();
                    }
                }
                return true;

            default:
                return false;
        }
    }

    public override void Step()
    {
        ((CameraSideScroller)Scene.Camera).HorizontalOffset = CenterCamera ? CameraOffset.Center : CameraOffset.Default;

        if (HitTimer <= 100)
            HitTimer++;

        SpawnRedLum();
        
        base.Step();
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        bool doNotDraw = HitTimer <= 100 && HitPoints != 0 && (GameTime.ElapsedFrames & 1) == 0;

        if ((Scene.Camera.IsActorFramed(this) || forceDraw) && !doNotDraw)
        {
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.PlayChannelSound(animationPlayer);
            AnimatedObject.ComputeNextFrame();
        }
    }
}