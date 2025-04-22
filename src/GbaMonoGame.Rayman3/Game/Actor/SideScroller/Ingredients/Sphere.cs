using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Sphere : MovableActor
{
    public Sphere(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.ObjPriority = 60;
        Color = (Action)actorResource.FirstActionId == Action.Init_Purple ? SphereColor.Purple : SphereColor.Yellow;

        if (Color == SphereColor.Purple)
            AnimatedObject.BasePaletteIndex = 1;

        InitialPosition = Position;
        Timer = 0;

        State.SetTo(Fsm_Idle);
    }

    public SphereColor Color { get; }
    public Vector2 InitialPosition { get; }
    public ushort Timer { get; set; }
    public bool HasPlayedLandingSound { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_ThrowUp:
                State.MoveTo(Fsm_ThrownUp);
                return false;

            case Message.Actor_ThrowForward:
                State.MoveTo(Fsm_ThrownForward);
                return false;

            case Message.Actor_Drop:
                State.MoveTo(Fsm_Drop);
                return false;

            case Message.Actor_ReloadAnimation:
                // Don't need to do anything. The original game sets the palette index again, but we're using local indexes, so it never changes.
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (State == Fsm_Respawn)
            AnimatedObject.IsFramed = Scene.Camera.IsActorFramed(this) && (GameTime.ElapsedFrames & 1) != 0;
        else
            AnimatedObject.IsFramed = Scene.Camera.IsActorFramed(this);

        if (AnimatedObject.IsFramed)
            animationPlayer.Play(AnimatedObject);
        else
            AnimatedObject.ComputeNextFrame();
    }

    public enum SphereColor
    {
        Yellow,
        Purple,
    }
}