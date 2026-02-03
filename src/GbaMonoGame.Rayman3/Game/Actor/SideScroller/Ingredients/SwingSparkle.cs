using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class SwingSparkle : BaseActor
{
    public SwingSparkle(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.CurrentAnimation = actorResource.FirstActionId;
        AnimatedObject.ObjPriority = 48;
        State.SetTo(_Fsm_Default);
    }

    public float Distance { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Scene.Camera.IsActorFramed(this) || forceDraw)
        {
            if (AnimatedObject.CurrentAnimation == 1 || Distance < ((Rayman)Scene.MainActor).PreviousXSpeed - 32)
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