using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class CaptureTheFlagFlag : MovableActor
{
    public CaptureTheFlagFlag(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.Palettes = Scene.MainActor.AnimatedObject.Palettes;

        Unused = null;
        AttachedPlayer = null;
        BaseActorId = actorResource.Links[0]!.Value;

        State.SetTo(Fsm_Wait);
    }

    public Rayman AttachedPlayer { get; set; }
    public object Unused { get; set; } // Unused
    public bool IsMovingUp { get; set; }
    public int BaseActorId { get; set; }
    public int SavedPaletteIndex { get; set; } // Used, but has no purpose

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.CaptureTheFlagFlag_AttachToPlayer:
                AttachedPlayer = (Rayman)param;
                return false;

            case Message.CaptureTheFlagFlag_Drop:
                State.MoveTo(Fsm_Dropped);
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (State == Fsm_Dropped)
            base.Draw(animationPlayer, forceDraw);
    }
}