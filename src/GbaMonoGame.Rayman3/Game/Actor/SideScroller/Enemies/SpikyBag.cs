using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

// Original name: SacPieux
[GenerateFsmFields]
public sealed partial class SpikyBag : InteractableActor
{
    public SpikyBag(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        CurrentSwingAnimation = 0;

        if ((Action)actorResource.FirstActionId == Action.Stationary)
            State.SetTo(_Fsm_Stationary);
        else if ((Action)actorResource.FirstActionId == Action.Swing)
            State.SetTo(_Fsm_Swing);
        else
            throw new Exception("Invalid initial action id");
    }

    public int CurrentSwingAnimation { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawLarge(animationPlayer, forceDraw);
    }
}