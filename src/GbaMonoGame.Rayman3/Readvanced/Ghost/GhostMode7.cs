using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public sealed class GhostMode7 : Mode7Actor
{
    public GhostMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(null);
    }

    public void ApplyFrame(GhostActorFrame frame)
    {
        Position = new Vector2(frame.XPosition, frame.YPosition);
        Direction = frame.Direction;
        SetMode7DirectionalAction(frame.BaseActionId, frame.ActionsSize);
    }
}