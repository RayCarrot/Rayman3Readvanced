using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class CaptureTheFlagItems : ActionActor
{
    public CaptureTheFlagItems(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Timer = 0;
        InitialActionId = (Action)actorResource.FirstActionId;

        State.SetTo(Fsm_Default);
    }

    public Action InitialActionId { get; set; }
    public uint Timer { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Timer == 0)
        {
            base.Draw(animationPlayer, forceDraw);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();
        }
    }
}