using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Sphere
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this) && ((Rayman)Scene.MainActor).AttachedObject == null)
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_PickUpObject, this);

                if (Scene.IsDetectedMainActor(this) && ((Rayman)Scene.MainActor).AttachedObject == this)
                {
                    State.MoveTo(Fsm_PickedUp);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_PickedUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Drop(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Drop;
                break;

            case FsmAction.Step:
                Vector2 mapPos = new(Position.X, GetDetectionBox().MaxY);
                PhysicalType type = Scene.GetPhysicalType(mapPos);

                if (type.IsSolid)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SpherImp_Mix02);
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrownUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.ThrownUp;
                CheckAgainstObjectCollision = false;
                break;

            case FsmAction.Step:
                bool shouldRespawn = false;

                Vector2 mapPos = new(Position.X, GetDetectionBox().MaxY);
                PhysicalType type = Scene.GetPhysicalType(mapPos);

                if (type.IsSolid && ActionId != Action.Land)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SpherImp_Mix02);
                    ActionId = Action.Land;
                }

                if (Scene.IsDetectedMainActor(this) &&
                    ((Rayman)Scene.MainActor).AttachedObject == null &&
                    Speed.Y > 0 &&
                    ActionId != Action.Land)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_CatchObject, this);
                }

                if (type.Value is PhysicalTypeValue.InstaKill or PhysicalTypeValue.MoltenLava)
                    shouldRespawn = true;

                if (Speed.Y == 0 && ActionId == Action.Land)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SpherImp_Mix02);
                    State.MoveTo(Fsm_Idle);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this) && ((Rayman)Scene.MainActor).AttachedObject == this && Speed.Y > 0)
                {
                    State.MoveTo(Fsm_PickedUp);
                    return false;
                }

                if (shouldRespawn)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Tag_Mix02);
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                CheckAgainstObjectCollision = true;
                break;
        }

        return true;
    }

    public bool Fsm_ThrownForward(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                HasPlayedLandingSound = false;
                ActionId = Scene.MainActor.IsFacingRight ? Action.ThrownForward_Right : Action.ThrownForward_Left;
                break;

            case FsmAction.Step:
                bool shouldRespawn = false;

                Vector2 mapPos = new(Position.X, GetDetectionBox().MaxY);
                PhysicalType type = Scene.GetPhysicalType(mapPos);

                if (type.IsSolid && ActionId is Action.Land_Right or Action.Land_Left && !HasPlayedLandingSound)
                {
                    HasPlayedLandingSound = true;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SpherImp_Mix02);
                }

                if (type.IsSolid && ActionId is not (Action.Land_Right or Action.Land_Left))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SpherImp_Mix02);
                    ActionId = ActionId == Action.ThrownForward_Right ? Action.Land_Right : Action.Land_Left;
                }

                if (type.Value is PhysicalTypeValue.InstaKill or PhysicalTypeValue.MoltenLava)
                    shouldRespawn = true;

                if (Speed == Vector2.Zero && ActionId is Action.Land_Right or Action.Land_Left)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }

                if (shouldRespawn)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Tag_Mix02);
                    State.MoveTo(Fsm_Respawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Respawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Position = InitialPosition;
                Timer = (ushort)GameTime.ElapsedFrames;
                ActionId = Action.Respawn;
                break;

            case FsmAction.Step:
                Position = InitialPosition;

                if (ActionId != Action.Idle && GameTime.ElapsedFrames - Timer > 120)
                {
                    ActionId = Action.Idle;
                }
                else if (ActionId == Action.Idle && GameTime.ElapsedFrames - Timer == 122 && AnimatedObject.IsFramed)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Appear_SocleFX1_Mix01);
                }

                if (GameTime.ElapsedFrames - Timer > 180)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}