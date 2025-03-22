using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Appears to serve no purpose other than to make sure the worldmap scene has a main actor in it
public sealed class RaymanWorldMap : MovableActor
{
    public RaymanWorldMap(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(null);
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // Do nothing
    }
}