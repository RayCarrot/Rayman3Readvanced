using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

// Original name: FleurFeu
[GenerateFsmFields]
public sealed partial class FlowerFire : BaseActor
{
    public FlowerFire(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.CurrentAnimation = 0;
        AnimatedObject.ObjPriority = 15;

        State.SetTo(_Fsm_Default);
    }

    public byte Timer { get; set; }
    public MovingPlatform Platform { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Intercept messages
        switch (message)
        {
            case Message.ResurrectWakeUp:
                Timer = 180;
                break;
        }

        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Actor_LightOnFireRight:
                State.MoveTo(_Fsm_End);
                return false;

            default:
                return false;
        }
    }
}