using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class WaterSplashMode7 : Mode7Actor
{
    public WaterSplashMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.BgPriority = 0;
        RenderHeight = 16;
        Direction = Angle256.Half;
        IsAffine = false;

        State.SetTo(Fsm_Default);
    }
}