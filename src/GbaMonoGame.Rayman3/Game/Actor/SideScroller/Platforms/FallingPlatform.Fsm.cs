using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class FallingPlatform
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                Position = InitialPosition;
                Timer = 0;
                GameTime = GbaMonoGame.GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                GameTime++;

                if (GameTime != GbaMonoGame.GameTime.ElapsedFrames)
                {
                    State.MoveTo(_Fsm_Idle);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this) && Scene.MainActor.Position.Y <= Position.Y)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
                    State.MoveTo(_Fsm_Timed);
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

                if (AnimatedObject.IsFramed && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PF2Crac_Mix02__or__RootOut_Pitch))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PF2Crac_Mix02__or__RootOut_Pitch);

                ActionId = Action.Shake;
                break;

            case FsmAction.Step:
                Timer++;
                GameTime++;

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

                if (GameTime != GbaMonoGame.GameTime.ElapsedFrames)
                {
                    State.MoveTo(_Fsm_Idle);
                    return false;
                }

                if (Timer > 80)
                {
                    State.MoveTo(_Fsm_Fall);
                    return false;
                }
                break;
            
            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_BeginFall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.BeginFall;
                break;

            case FsmAction.Step:
                GameTime++;

                if (GameTime != GbaMonoGame.GameTime.ElapsedFrames)
                {
                    State.MoveTo(_Fsm_Idle);
                    return false;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Fall);
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
                if (AnimatedObject.IsFramed && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__PF1Fall_PF2Fall_Mix03))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PF1Fall_PF2Fall_Mix03);

                ActionId = Action.Fall;
                Timer = 0;
                break;

            case FsmAction.Step:
                GameTime++;

                if (Timer != 0xFF)
                    Timer++;

                // Unlink from main actor if no longer colliding
                if (Scene.MainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || Scene.MainActor.Position.Y > Position.Y || Timer > 30)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }

                PhysicalType type = Scene.GetPhysicalType(Position);

                if (GameTime != GbaMonoGame.GameTime.ElapsedFrames || type.IsSolid || !AnimatedObject.IsFramed)
                {
                    State.MoveTo(_Fsm_Idle);
                    return false;
                }

                break;
            
            case FsmAction.UnInit:
                if (Scene.MainActor.LinkedMovementActor == this)
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);

                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}