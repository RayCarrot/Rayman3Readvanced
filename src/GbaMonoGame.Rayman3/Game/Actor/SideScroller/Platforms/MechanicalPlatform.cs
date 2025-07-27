using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class MechanicalPlatform : MovableActor
{
    public MechanicalPlatform(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialPosition = actorResource.Pos.ToVector2();
        IsInvulnerable = true;
        AnimatedObject.ObjPriority = 50;
        
        SpeedPointer = new AnimatedObject(actorResource.Model.AnimatedObject, false)
        {
            CurrentAnimation = 3,
            BgPriority = 1,
            ObjPriority = 49,
            AffineMatrix = new AffineMatrix(190, 1, 1),
            RenderContext = AnimatedObject.RenderContext,
        };
        SpeedPointer.SetFlagUseRotationScaling(true);

        MechModel.Speed = Vector2.Zero;

        State.SetTo(Fsm_Default);
    }

    public Vector2 InitialPosition { get; }
    public AnimatedObject SpeedPointer { get; }
    public float SpeedY { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_Hit:
                RaymanBody body = (RaymanBody)param;
                if ((Scene.MainActor.LinkedMovementActor == this || IsSolid) && 
                    body.State != body.Fsm_MoveBackwards)
                {
                    float yDist = InitialPosition.Y - Position.Y;

                    if (body.HasCharged)
                    {
                        SpeedY = -8;
                        ActionId = IsFacingRight ? Action.HardHit_Right : Action.HardHit_Left;

                        if (body.BodyPartType is not (RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist))
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Mix02);
                    }
                    else
                    {
                        SpeedY = -4;
                        ActionId = IsFacingRight ? Action.SoftHit_Right : Action.SoftHit_Left;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Low);
                    }

                    ChangeAction();

                    if (yDist >= 5)
                    {
                        if (yDist >= 75)
                            SpeedY = 2;
                        else
                            SpeedY /= 2;
                    }

                    MechModel.Speed = MechModel.Speed with { Y = SpeedY };
                }
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Scene.Camera.IsActorFramed(this) || forceDraw)
        {
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);

            SpeedPointer.IsFramed = true;
            animationPlayer.Play(SpeedPointer);

            float yDist = InitialPosition.Y - Position.Y;

            Angle256 rotation = 206;
            if (yDist >= 1)
                rotation += yDist * 113 * MathHelpers.FromFixedPoint(0x16c);

            SpeedPointer.AffineMatrix = new AffineMatrix(rotation, 1, 1);

            Angle256 angle = rotation - 61;
            float radius = MathHelpers.FromFixedPoint(0xe0900);
            SpeedPointer.ScreenPos = AnimatedObject.ScreenPos + angle.ToDirectionalVector() * radius + new Vector2(1, 8);

            // NOTE: In the game it checks if the rotation is equal to 62, but since we're using floats we can't do that, so
            //       we check with a tolerance of 1.0 to get it close to it, which is good enough. It's supposed to trigger
            //       when it has rotated all the way, i.e. yDist is at its max. It's however bugged in the original game as
            //       it only triggers every second time the platform is punched. This is because the platform doesn't land
            //       at the same height from the ground each time.
            if (Math.Abs(rotation.Value - 62) < 1.0 && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__Cloche01_Mix01))
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Cloche01_Mix01);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();

            SpeedPointer.IsFramed = false;
            SpeedPointer.ComputeNextFrame();
        }
    }
}