using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Switch : InteractableActor
{
    public Switch(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Links = actorResource.Links;

        AnimatedObject.ObjPriority = 55;

        State.SetTo(_Fsm_Deactivated);
    }

    public byte?[] Links { get; }

    public void SetToActivated()
    {
        State.SetTo(_Fsm_Activated);
    }
}