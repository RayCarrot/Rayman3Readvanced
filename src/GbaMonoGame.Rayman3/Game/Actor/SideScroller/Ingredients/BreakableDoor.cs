using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class BreakableDoor : InteractableActor
{
    public BreakableDoor(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.ObjPriority = 63;
        State.SetTo(_Fsm_Idle);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_Hurt:
                if (State == _Fsm_Idle)
                    ActionId = IsFacingRight ? Action.Break_Right : Action.Break_Left;
                return false;

            case Message.Actor_Hit:
                RaymanBody bodyPart = (RaymanBody)param;

                if (bodyPart.BodyPartType is not (RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodImp_Mix03);

                ActionId = IsFacingRight ? Action.Shake_Right : Action.Shake_Left;
                return false;

            default:
                return false;
        }
    }
}