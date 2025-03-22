using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Balloon : MovableActor
{
    public Balloon(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        ShouldDraw = true;

        IsRespawnable = (Action)actorResource.FirstActionId is not (Action.Init_NotRespawnable or Action.Init_TimedNotRespawnable);
        IsTimed = (Action)actorResource.FirstActionId == Action.Init_TimedNotRespawnable;

        LinkedBalloonId = actorResource.Links[0];

        State.SetTo(Fsm_Inflate);

        ChangeAction();
    }

    public int? LinkedBalloonId { get; }
    public byte Timer { get; set; }

    // Flags
    public bool ShouldDraw { get; set; }
    public bool IsRespawnable { get; set; }
    public bool IsTimed { get; }
    public bool HasPlayedSound { get; set; }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (ShouldDraw)
            base.Draw(animationPlayer, forceDraw);
    }
}