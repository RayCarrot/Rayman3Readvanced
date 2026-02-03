using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class BlackLum
{
    private bool FsmStep_CheckDeath()
    {
        if (HitPoints == 0)
        {
            State.MoveTo(_Fsm_Dying);
            return false;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.SetTo(_Fsm_PrepareAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_PrepareAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.PrepareAttack_Right : Action.PrepareAttack_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.IsDetectedMainActor(this))
                {
                    State.SetTo(_Fsm_Attack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Attack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Vector2 posDiff = Scene.MainActor.Position - Position;

                if (Math.Abs(posDiff.Y) < Math.Abs(posDiff.X))
                {
                    FlySpeed = new Vector2(
                        x: posDiff.X < 0 ? -2 : 2,
                        y: posDiff.Y < 0 ? -1 : 1);
                }
                else
                {
                    FlySpeed = new Vector2(
                        x: posDiff.X < 0 ? -1 : 1,
                        y: posDiff.Y < 0 ? -2 : 2);
                }
                
                ActionId = posDiff.X < 0 ? Action.Fly_Left : Action.Fly_Right;

                ChangeAction();
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumAtk01_Mix02);
                break;

            case FsmAction.Step:
                bool isDead;
                
                if (!FsmStep_CheckDeath())
                    return false;

                Position += FlySpeed;

                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                    isDead = true;
                }
                else
                {
                    isDead = HasCollidedWithPhysical();
                }

                if (isDead)
                {
                    State.MoveTo(_Fsm_Dying);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumAtk01_Mix02);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MumuDead_Mix04);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    // Spawn red lum (unused)
                    if (Resource.Links[0] != null)
                    {
                        GameObject linkedObj = Scene.GetGameObject(Resource.Links[0].Value);
                        linkedObj.Position = Position;
                        linkedObj.ProcessMessage(this, Message.Lum_ToggleVisibility);
                    }

                    ProcessMessage(this, Message.Destroy);
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}