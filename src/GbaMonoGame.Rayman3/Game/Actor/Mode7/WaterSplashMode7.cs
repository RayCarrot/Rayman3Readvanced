using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class WaterSplashMode7 : Mode7Actor
{
    public WaterSplashMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.BgPriority = 0;
        RenderHeight = RENDER_HEIGHT;
        Direction = Angle256.Half;
        IsAffine = false;

        State.SetTo(_Fsm_Default);
    }
}