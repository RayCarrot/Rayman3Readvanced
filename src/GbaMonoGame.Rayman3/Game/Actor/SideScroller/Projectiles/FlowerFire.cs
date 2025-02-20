using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: FleurFeu
public sealed partial class FlowerFire : BaseActor
{
    public FlowerFire(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.CurrentAnimation = 0;
        AnimatedObject.ObjPriority = 15;

        State.SetTo(Fsm_Default);
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
            case Message.LightOnFire_Right:
                State.MoveTo(Fsm_End);
                return false;

            default:
                return false;
        }
    }
}