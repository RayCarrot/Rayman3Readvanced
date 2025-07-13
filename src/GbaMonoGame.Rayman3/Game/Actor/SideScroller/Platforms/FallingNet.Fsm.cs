using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class FallingNet
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                Position = InitialPosition;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginHang, this);
                    State.MoveTo(Fsm_Timed);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Timed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = Action.Shake;

                if (AnimatedObject.IsFramed && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PF2Crac_Mix02__or__RootOut_Pitch))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PF2Crac_Mix02__or__RootOut_Pitch);
                break;

            case FsmAction.Step:
                Timer++;

                if (Scene.IsDetectedMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginHang, this);
                else
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_EndHang, this);

                if (Timer > 90)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                if (!Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                Scene.MainActor.ProcessMessage(this, Message.Rayman_AllowSafetyJump, this);
                Scene.MainActor.ProcessMessage(this, Message.Rayman_EndHang, this);
                break;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (AnimatedObject.IsFramed && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PF2Fall_Mix03))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PF2Fall_Mix03);

                ActionId = Action.Fall;
                break;

            case FsmAction.Step:
                PhysicalType type = Scene.GetPhysicalType(Position);

                if (type.IsSolid || !AnimatedObject.IsFramed)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }

                break;
            
            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}