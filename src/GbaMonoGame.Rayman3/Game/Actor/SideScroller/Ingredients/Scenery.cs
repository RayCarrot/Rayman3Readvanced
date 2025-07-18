using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed class Scenery : BaseActor
{
    public Scenery(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.CurrentAnimation = actorResource.FirstActionId;
        AnimatedObject.ObjPriority = 60;

        // TRAILER
        if (GameInfo.MapId == MapId.WoodLight_M1 && InstanceId is 50 or 51)
            Position += new Vector2(75, 0);
    }
}