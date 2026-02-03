using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class KegDebris : ActionActor
{
    public KegDebris(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Timer = 0;
        State.SetTo(_Fsm_Default);
    }

    public byte Timer { get; set; }
}