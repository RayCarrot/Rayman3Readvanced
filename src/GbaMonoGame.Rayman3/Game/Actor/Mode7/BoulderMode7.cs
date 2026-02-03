using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class BoulderMode7 : Mode7Actor
{
    public BoulderMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 64;
        Rotation = 0;
        Scale = Vector2.One;

        if ((Action)actorResource.FirstActionId == Action.Bounce)
            State.SetTo(_Fsm_Bounce);
        else
            State.SetTo(_Fsm_Move);
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
            Angle256 angle = MathHelpers.Atan2_256((Position - player.Position).FlipY());
            
            // Rotate 180 degrees
            angle += Angle256.Half;

            Vector2 angleVector = MathHelpers.DirectionalVector256(angle);

            float force = angleVector.X * (player.Speed.X - Speed.X) * 2 - 
                          angleVector.Y * (player.Speed.Y - Speed.Y) * 2;

            if (force < 0)
            {
                player.MechModel.Speed -= angleVector.FlipY() * force;
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