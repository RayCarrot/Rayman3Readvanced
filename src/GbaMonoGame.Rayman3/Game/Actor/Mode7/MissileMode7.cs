using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: Implement
public sealed partial class MissileMode7 : Mode7Actor
{
    public MissileMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        // NOTE: Temp code for testing so that the actor faces away from the camera
        AnimatedObject.CurrentAnimation = 25;
    }
}