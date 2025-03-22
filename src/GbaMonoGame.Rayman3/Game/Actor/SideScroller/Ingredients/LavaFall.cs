using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class LavaFall : InteractableActor
{
    public LavaFall(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        BubbleSoundCountdown = 0;
        AnimatedObject.ObjPriority = 48;
        
        State.SetTo(Fsm_Flow);
    }

    public byte Timer { get; set; }
    public bool ShouldDraw { get; set; }
    public byte BubbleSoundCountdown { get; set; }

    public override void Step()
    {
        base.Step();
        GameInfo.ActorSoundFlags &= ~ActorSoundFlags.LavaFall;
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (ShouldDraw)
            base.Draw(animationPlayer, forceDraw);
    }
}