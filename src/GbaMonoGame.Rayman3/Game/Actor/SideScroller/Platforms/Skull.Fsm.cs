using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Skull
{
    public bool Fsm_Spawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Position = InitialPosition;
                ActionId = Action.Spawn;

                if (AnimatedObject.IsFramed)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkulInit_Mix04);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkulInit_Mix04);
                }
                break;

            case FsmAction.Step:
                bool isActionFinished = IsActionFinished;

                if (isActionFinished && InitialAction == Action.SpinStart)
                {
                    State.MoveTo(Fsm_Rotate);
                    return false;
                }

                if (isActionFinished)
                {
                    State.MoveTo(Fsm_Move);
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
    public bool Fsm_Rotate(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = InitialAction;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (IsHit() && ActionId is Action.SpinStart or Action.Spin_6)
                    Timer |= 0xf000;

                if (IsActionFinished && ActionId == Action.SpinStart)
                {
                    ActionId = Action.Spin_6;

                    if ((Timer & 0xf000) == 0)
                        ChangeAction();
                }
                else
                {
                    Timer++;

                    if ((Timer & 0xfff) > 270 && ActionId == Action.Spin_6)
                    {
                        ActionId = Action.Spin_7;

                        if ((Timer & 0xf000) == 0)
                            ChangeAction();
                    }
                }

                if (IsActionFinished && (Timer & 0xf000) != 0)
                {
                    State.MoveTo(Fsm_Stationary);
                    return false;
                }

                if (IsActionFinished && ActionId == Action.Spin_7)
                {
                    State.MoveTo(Fsm_Despawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = InitialAction;
                break;

            case FsmAction.Step:
                bool isHit = IsHit();
                PhysicalType type = Scene.GetPhysicalType(Position);

                if (type == PhysicalTypeValue.Enemy_Up)
                {
                    CheckAgainstObjectCollision = true;
                    State.MoveTo(Fsm_Despawn);
                    return false;
                }

                if (isHit)
                {
                    if (AnimatedObject.IsFramed)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkullHit_Mix02);

                    State.MoveTo(Fsm_Stationary);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Stationary(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Stationary;
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                // Shake after 4.5 seconds
                if (Timer > 270 && ActionId != Action.StationaryShake)
                {
                    ActionId = Action.StationaryShake;
                    ChangeAction();

                    if (AnimatedObject.IsFramed)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkulShak_Mix01);
                }

                if (IsActionFinished && ActionId == Action.StationaryShake)
                {
                    if (AnimatedObject.IsFramed)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkulShak_Mix01);
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Main_LinkMovement, this);
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                    }
                }

                if (Timer > 360)
                {
                    State.MoveTo(Fsm_Despawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Despawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (AnimatedObject.IsFramed)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkullEnd_Mix02);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkullEnd_Mix02);
                }

                ActionId = Action.Despawn;

                if (Scene.IsDetectedMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Main_AllowCoyoteJump, this);
                
                Timer = 0;
                break;

            case FsmAction.Step:
                // Don't allow standing on after 15 frames
                if (Timer < 15)
                {
                    Timer++;

                    if (Timer == 15)
                    {
                        if (Scene.MainActor.LinkedMovementActor == this)
                            Scene.MainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                    }
                }

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Spawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SolidMove(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.SolidMove_Wait;
                break;

            case FsmAction.Step:
                PhysicalType type = Scene.GetPhysicalType(Position);

                Timer++;

                // Change direction
                if (type == PhysicalTypeValue.Enemy_Left)
                {
                    if (ActionId == Action.SolidMove_Right)
                    {
                        ActionId = Action.SolidMove_Stationary;
                        Timer = 0;
                    }
                    else if (ActionId == Action.SolidMove_Stationary && Timer > 180)
                    {
                        ActionId = Action.SolidMove_Left;
                    }
                }
                else if (type == PhysicalTypeValue.Enemy_Right)
                {
                    if (ActionId == Action.SolidMove_Left)
                    {
                        ActionId = Action.SolidMove_Stationary;
                        Timer = 0;
                    }
                    else if (ActionId == Action.SolidMove_Stationary && Timer > 180)
                    {
                        ActionId = Action.SolidMove_Right;
                    }
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Main_LinkMovement, this);

                    if (ActionId == Action.SolidMove_Wait)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkullHit_Mix02);
                        ActionId = Action.SolidMove_Right;
                    }
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}