using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class SwingSparkle : BaseActor
{
    public SwingSparkle(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.CurrentAnimation = actorResource.FirstActionId;
        AnimatedObject.ObjPriority = 48;
        State.SetTo(Fsm_Default);
    }

    public float Value { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Scene.Camera.IsActorFramed(this) || forceDraw)
        {
            if (AnimatedObject.CurrentAnimation == 1 || Value < ((Rayman)Scene.MainActor).PreviousXSpeed - 32)
            {
                AnimatedObject.IsFramed = true;
                animationPlayer.Play(AnimatedObject);
            }
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }
}