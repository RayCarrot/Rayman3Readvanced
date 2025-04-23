using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public sealed partial class BoulderMode7 : Mode7Actor
{
    public BoulderMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 64;
        Rotation = 0;
        Scale = Vector2.One;

        if ((Action)actorResource.FirstActionId == Action.Bounce)
            State.SetTo(Fsm_Bounce);
        else
            State.SetTo(Fsm_Move);
    }

    private const float COLLISION_HEIGHT = 16;
    private const float ROTATION_SPEED = 3;
    private const float INITIAL_BOUNCE_SPEED = -8;
    private const float BOUNCE_SPEED_ACCELERATION = 1 / 4f;
    private const float SCALE_DELTA_X = -1 / 32f;
    private const float SCALE_DELTA_Y = 1 / 4f;
    private const float MAX_SCALE_Y = 2.5f;

    public float Rotation { get; set; }
    public Vector2 Scale { get; set; }
    public float BounceSpeed { get; set; }
    public bool IsSquashing { get; set; }

    private void CheckPlayerCollision(MovableActor player)
    {
        Box playerViewBox = player.GetViewBox();
        Box viewBox = GetViewBox();

        if (viewBox.Intersects(playerViewBox))
        {
            // Get the angle
            Angle256 angle = MathHelpers.Atan2_256(TgxCameraMode7.ToMathSpace(Position - player.Position));
            
            // Rotate 180 degrees
            angle += Angle256.Half;

            Vector2 angleVector = MathHelpers.DirectionalVector256(angle);

            float force = angleVector.X * (player.Speed.X - Speed.X) * 2 - 
                          angleVector.Y * (player.Speed.Y - Speed.Y) * 2;

            if (force < 0)
            {
                player.MechModel.Speed -= TgxCameraMode7.ToGameSpace(angleVector) * force;
                player.MechModel.Acceleration = Vector2.Zero;

                if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PinBall_Mix02))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Mix02);
            }
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // NOTE: In the original game it re-implements IsActorFramed so that it can apply a custom rotation and multiply in the custom scale
        AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, Scale.X, Scale.Y);
        base.Draw(animationPlayer, forceDraw);
    }
}