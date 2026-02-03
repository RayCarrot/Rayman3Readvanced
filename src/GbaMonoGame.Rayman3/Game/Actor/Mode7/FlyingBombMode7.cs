using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class FlyingBombMode7 : Mode7Actor
{
    public FlyingBombMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = 0;
        RenderHeight = 32;

        switch ((Action)actorResource.FirstActionId)
        {
            default:
            case Action.Stationary:
                State.SetTo(_Fsm_Stationary);
                break;

            case Action.MoveVertical_0:
                State.SetTo(_Fsm_MoveVertical);
                break;

            case Action.MoveVertical_32:
                State.SetTo(_Fsm_MoveVertical);
                ZPos = 32;
                break;

            case Action.MoveVertical_64:
                State.SetTo(_Fsm_MoveVertical);
                ZPos = 64;
                break;

            case Action.MoveVertical_96:
                State.SetTo(_Fsm_MoveVertical);
                ZPos = 96;
                break;
            
            case Action.MoveVertical_128:
                State.SetTo(_Fsm_MoveVertical);
                ZPos = 128;
                break;

            case Action.Move_0:
            case Action.Move_1:
                State.SetTo(_Fsm_Move);
                break;

            case Action.Drop:
                ZPos = 1024;
                State.SetTo(_Fsm_Drop);
                break;
        }
    }

    public float ZPosSpeed { get; set; }
}