using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class FallingBridge
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsLeftBridgePart ? Action.Idle_Left : Action.Idle_Right;
                Position = InitialPosition;
                IsSolid = true;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    if (Link != null)
                    {
                        GameObject linkedObj = Scene.GetGameObject(Link.Value);
                        linkedObj.ProcessMessage(this, Message.Actor_Fall);
                    }

                    Scene.MainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
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
                ActionId = IsLeftBridgePart ? Action.Shake_Left : Action.Shake_Right;

                if (AnimatedObject.IsFramed && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PF2Crac_Mix02__or__RootOut_Pitch))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PF2Crac_Mix02__or__RootOut_Pitch);
                break;

            case FsmAction.Step:
                Timer++;

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }

                if (Timer > 180)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsLeftBridgePart ? Action.Fall_Left : Action.Fall_Right;

                if (Scene.MainActor.LinkedMovementActor == this)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_AllowSafetyJump, this);
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                }

                if (AnimatedObject.IsFramed && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PF2Fall_Mix03))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PF2Fall_Mix03);
                break;

            case FsmAction.Step:
                PhysicalType type = Scene.GetPhysicalType(Position);

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }

                if (type.IsSolid || !AnimatedObject.IsFramed)
                {
                    State.MoveTo(Fsm_Break);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Break(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsLeftBridgePart ? Action.Break_Left : Action.Break_Right;
                IsSolid = false;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
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