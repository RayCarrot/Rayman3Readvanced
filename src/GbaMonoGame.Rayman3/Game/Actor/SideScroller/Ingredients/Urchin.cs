using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Urchin : InteractableActor
{
    public Urchin(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Default);
    }

    public override void Step()
    {
        base.Step();
        GameInfo.ActorSoundFlags &= ~ActorSoundFlags.Urchin;
    }
}