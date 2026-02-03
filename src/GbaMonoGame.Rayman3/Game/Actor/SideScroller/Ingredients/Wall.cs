using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Wall : InteractableActor
{
    public Wall(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Timer = 0;

        if ((Action)actorResource.FirstActionId == Action.Action0)
            State.SetTo(_Fsm_Variant1State1);
        else if ((Action)actorResource.FirstActionId == Action.Action2)
            State.SetTo(_Fsm_Variant2State1);
        else
            throw new Exception("Invalid initial action for the Wall actor");
    }

    public uint Timer { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (State != _Fsm_Variant1State4)
            base.Draw(animationPlayer, forceDraw);
    }
}