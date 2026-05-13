using System;
using BinarySerializer.Ubisoft.GbaEngine;
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

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Intercept messages for entering/leaving the current knot so we can pause the animation
        // when outside the current knot in order to preserve the cycles in the walking shell levels
        switch (message)
        {
            case Message.Readvanced_EnterCurrentKnot:
                if (AnimatedObject.IsPaused)
                    AnimatedObject.Resume();
                break;

            case Message.Readvanced_LeaveCurrentKnot:
                if (State == _Fsm_Swing)
                    AnimatedObject.Pause();
                break;
        }

        base.ProcessMessageImpl(sender, message, param);

        return false;
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawLarge(animationPlayer, forceDraw);
    }
}