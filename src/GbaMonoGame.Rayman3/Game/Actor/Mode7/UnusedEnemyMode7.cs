using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class UnusedEnemyMode7 : Mode7Actor
{
    public UnusedEnemyMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = POSITION_Z;
        RenderHeight = RENDER_HEIGHT;

        State.SetTo(_Fsm_Default);
    }
}