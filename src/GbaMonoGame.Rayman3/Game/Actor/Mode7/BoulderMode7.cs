using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class BoulderMode7 : Mode7Actor
{
    public BoulderMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Direction = 0;
        ZPos = 0;
        RenderHeight = 64;
        Rotation = 0;
        Scale = Vector2.One;

        if ((Action)actorResource.FirstActionId == Action.Bounce)
            State.SetTo(Fsm_Bounce);
        else
            State.SetTo(Fsm_Move);
    }

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
            float angle = MathHelpers.Atan2_256((Position - player.Position) * new Vector2(1, -1));
            
            // Rotate 180 degrees
            angle += 128;

            Vector2 angleVector = MathHelpers.DirectionalVector256(angle);

            float force = angleVector.X * (player.Speed.X - Speed.X) * 2 - 
                          angleVector.Y * (player.Speed.Y - Speed.Y) * 2;

            if (force < 0)
            {
                player.MechModel.Speed -= angleVector * new Vector2(1, -1) * force;
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