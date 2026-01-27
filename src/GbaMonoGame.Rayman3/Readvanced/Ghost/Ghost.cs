using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public sealed class Ghost : BaseActor
{
    public Ghost(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(null);
    }

    public void ApplyFrame(GhostActorFrame frame)
    {
        Position = new Vector2(frame.XPosition, frame.YPosition);
        AnimatedObject.CurrentAnimation = frame.AnimationId;
        AnimatedObject.CurrentFrame = frame.FrameId;
        AnimatedObject.ActiveChannels = frame.ActiveChannels;
        AnimatedObject.FlipX = frame.FlipX;
        AnimatedObject.FlipY = frame.FlipY;
    }
}