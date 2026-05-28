using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Urchin : InteractableActor
{
    public Urchin(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Default);
    }

    public override void Step()
    {
        base.Step();
        Rayman3.GameInfo.ActorSoundFlags &= ~ActorSoundFlags.Urchin;
    }
}