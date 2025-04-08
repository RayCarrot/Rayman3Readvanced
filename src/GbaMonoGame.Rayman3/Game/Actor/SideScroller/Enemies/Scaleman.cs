using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Scaleman : MovableActor
{
    public Scaleman(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource)
    {
        IsInvulnerable = true;

        ScalemanShadow = null;
        Field_64 = null;
        Timer = 0;
        Field_6A = 0;
        Field_6C = 101;
        Field_6E = 1;

        State.SetTo(Fsm_PreInit);
    }

    // TODO: Name properties
    public ScalemanShadow ScalemanShadow { get; set; }
    public object Field_64 { get; set; }
    public ushort Timer { get; set; }
    public ushort Field_6A { get; set; }
    public ushort Field_6C { get; set; }
    public byte Field_6E { get; set; }

    private bool IsSecondPhase() => HitPoints <= 3;

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // This is one of the few actors which actually returns true in here

        if (base.ProcessMessageImpl(sender, message, param))
            return true;

        switch (message)
        {
            case Message.Hit:
                RaymanBody raymanBody = (RaymanBody)param;

                // Shrink when hit while big
                if (ActionId is Action.Idle_Right or Action.Idle_Left ||
                    (ActionId is Action.Emerge_Right or Action.Emerge_Left && AnimatedObject.CurrentFrame >= 8))
                {
                    if (raymanBody.BodyPartType is RaymanBody.RaymanBodyPartType.Fist or RaymanBody.RaymanBodyPartType.SecondFist)
                    {
                        if (Position.X < raymanBody.Position.X)
                            ActionId = IsFacingRight ? Action.Hit_Right : Action.HitBehind_Left;
                        else
                            ActionId = IsFacingRight ? Action.HitBehind_Right : Action.Hit_Left;

                        State.MoveTo(Fsm_Shrink);

                        ChangeAction();

                        if (ScalemanShadow != null)
                        {
                            ScalemanShadow.ProcessMessage(this, Message.Destroy);
                            ScalemanShadow = null;
                        }
                    }
                }
                // Take damage when hit while small
                else
                {
                    // TODO: Implement
                }
                return true;

            default:
                return false;
        }
    }

    public override void Step()
    {
        // TODO: Implement
        base.Step();
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // TODO: Implement
        base.Draw(animationPlayer, forceDraw);
    }
}