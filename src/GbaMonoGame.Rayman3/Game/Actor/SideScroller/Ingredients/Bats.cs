using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Bats : ActionActor
{
    public Bats(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialAction = (Action)actorResource.FirstActionId;

        if (InitialAction is Action.Fly_HorizontallyStart1 or Action.Fly_VerticallyStart or Action.Fly_HorizontallyStart2)
            State.SetTo(Fsm_FlyWait);
        else if (InitialAction == Action.Stationary_Flap)
            State.SetTo(Fsm_StationaryWait);
        else
            throw new Exception("Invalid starting animation for bats");
    }

    public Action InitialAction { get; }
    public bool HorizontalFlip { get; set; }
    public Vector2 FlyPosition { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        base.Draw(animationPlayer, forceDraw);

        if (State == Fsm_FlyAway && !AnimatedObject.IsFramed)
            ProcessMessage(this, Message.Destroy);
    }
}