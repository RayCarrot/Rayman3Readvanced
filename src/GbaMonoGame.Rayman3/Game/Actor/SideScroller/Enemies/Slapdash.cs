using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Slapdash : MovableActor
{
    public Slapdash(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        PrevHitPoints = HitPoints;
        IsObjectCollisionXOnly = true;

        State.SetTo(_Fsm_Wait);
    }

    public int PrevHitPoints { get; set; }
    public uint InvulnerabilityTimer { get; set; }
    public byte Timer { get; set; }

    private void StartInvulnerability()
    {
        InvulnerabilityTimer = GameTime.ElapsedFrames;
        IsInvulnerable = true;
    }

    private bool ShouldTurnAround()
    {
        if (Speed.X == 0)
            return true;

        PhysicalType type = Scene.GetPhysicalType(Position + Tile.Up);
        
        if (IsFacingRight)
            return type == PhysicalTypeValue.Enemy_Left;
        else
            return type == PhysicalTypeValue.Enemy_Right;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Captor_Trigger_SendMessageWithCaptorParam:
                if (Rom.Platform == Platform.GBA)
                {
                    if (IsEnabled)
                    {
                        Box mainActorDetectionBox = Scene.MainActor.GetDetectionBox();
                        Box captorBox = ((Captor)param).GetCaptorBox();

                        if (mainActorDetectionBox.Intersects(captorBox))
                            Scene.Camera.ProcessMessage(this, Message.Cam_CenterPositionX);
                    }
                }
                return false;

            case Message.Actor_Hit:
                if (State != _Fsm_Hit && !IsInvulnerable)
                {
                    Vector2 hitPos = ((GameObject)param).Position;

                    bool mainActorInFront;
                    bool hitFromBehind;

                    if (IsFacingRight)
                    {
                        hitFromBehind = hitPos.X < Position.X;
                        mainActorInFront = Position.X < Scene.MainActor.Position.X;
                    }
                    else
                    {
                        hitFromBehind = Position.X < hitPos.X;
                        mainActorInFront = Scene.MainActor.Position.X < Position.X;
                    }

                    if (ActionId is 
                        Action.TurnAround_Right or Action.TurnAround_Left or 
                        Action.TurnAroundFromChargeAttack_Right or Action.TurnAroundFromChargeAttack_Left)
                    {
                        if (3 < AnimatedObject.CurrentFrame)
                        {
                            hitFromBehind = false;
                        }
                    }

                    if (hitFromBehind && !mainActorInFront)
                        State.MoveTo(_Fsm_Hit);
                    else
                        HitPoints = PrevHitPoints;
                }
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawWithInvulnerability(animationPlayer, forceDraw);
    }
}