using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: Implement
public sealed partial class MissileMode7 : Mode7Actor
{
    public MissileMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        // NOTE: Temp code for testing so that the actor faces away from the camera
        State.SetTo(action =>
        {
            switch (action)
            {
                case FsmAction.Init:
                    // Do nothing
                    break;

                case FsmAction.Step:
                    SetMode7DirectionalAction(0, 6);
                    break;

                case FsmAction.UnInit:
                    // Do nothing
                    break;
            }
            return false;
        });
    }

    public byte field_0x8d { get; set; }
    public byte field_0x8f { get; set; }
}