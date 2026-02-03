using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class RedShell
{
    private bool FsmStep_CheckDeath()
    {
        bool isDead = HitPoints == 0;

        if (!isDead && Scene.IsHitMainActor(this))
        {
            isDead = true;
            Scene.MainActor.ReceiveDamage(AttackPoints);
        }

        if (isDead)
        {
            Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);

            if (explosion != null)
                explosion.Position = Position - new Vector2(0, 32);

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Combust1_Mix02);
            ProcessMessage(this, Message.Destroy);
        }

        return true;
    }

    public bool Fsm_WaitingToCharge(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.WaitingToCharge_Right : Action.WaitingToCharge_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                float addLeft;
                float addRight;
                if (IsFacingRight)
                {
                    addLeft = 0;
                    addRight = 20;
                }
                else
                {
                    addLeft = -20;
                    addRight = 0;
                }

                if (Scene.IsDetectedMainActor(this, 0, 0, addLeft, addRight))
                {
                    State.MoveTo(_Fsm_ChargeAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_WaitingToPatrol(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(_Fsm_Patrolling);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Sleeping(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Sleep_Right : Action.Sleep_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                float addLeft;
                float addRight;
                if (IsFacingRight)
                {
                    addLeft = 0;
                    addRight = 30;
                }
                else
                {
                    addLeft = -30;
                    addRight = 0;
                }

                if (Scene.IsDetectedMainActor(this, 0, 0, addLeft, addRight))
                {
                    State.MoveTo(_Fsm_Patrolling);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ChargeAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                CheckAgainstObjectCollision = false;
                IsInvulnerable = true;
                ActionId = IsFacingRight ? Action.PrepareChargeAttack_Right : Action.PrepareChargeAttack_Left;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Combust1_Mix02);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                float mainActorOffset = Rom.Platform switch
                {
                    Platform.GBA => 100,
                    Platform.NGage => 200,
                    _ => throw new ArgumentOutOfRangeException()
                };
                float screenOffset = Rom.Platform switch
                {
                    Platform.GBA => 20,
                    Platform.NGage => 80,
                    _ => throw new ArgumentOutOfRangeException()
                };

                bool isOutOfRange = false;
                if (ActionId == Action.ChargeAttack_Right && Position.X > Scene.MainActor.Position.X + mainActorOffset)
                    isOutOfRange = true;
                else if (ActionId == Action.ChargeAttack_Left && Position.X < Scene.MainActor.Position.X - mainActorOffset)
                    isOutOfRange = true;

                if (ActionId is Action.ChargeAttack_Right or Action.ChargeAttack_Left &&
                    (isOutOfRange || 
                     Speed.X == 0 || 
                     AnimatedObject.ScreenPos.X < -screenOffset || 
                     AnimatedObject.ScreenPos.X > Scene.Resolution.X + screenOffset))
                {
                    Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);

                    if (explosion != null)
                        explosion.Position = Position;

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Combust1_Mix02);
                    ProcessMessage(this, Message.Destroy);
                }

                if (IsActionFinished && ActionId is Action.PrepareChargeAttack_Right or Action.PrepareChargeAttack_Left)
                    ActionId = IsFacingRight ? Action.ChargeAttack_Right : Action.ChargeAttack_Left;
                else if (ActionId is Action.PrepareChargeAttack_Right or Action.PrepareChargeAttack_Left && AnimatedObject.CurrentFrame > 3)
                    MechModel.Speed = MechModel.Speed with { X = IsFacingRight ? 1 : -1 };
                break;

            case FsmAction.UnInit:
                IsInvulnerable = false;
                break;
        }

        return true;
    }

    public bool Fsm_Patrolling(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__HorseCry_Mix02);

                if (ActionId is Action.Sleep_Right or Action.Sleep_Left)
                    ActionId = IsFacingRight ? Action.WakeUp_Right : Action.WakeUp_Left;
                else
                    ActionId = IsFacingRight ? Action.Walk_Right : Action.Walk_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (ActionId is not (Action.WakeUp_Right or Action.WakeUp_Left) && Speed.X == 0)
                {
                    Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);

                    if (explosion != null)
                        explosion.Position = Position;

                    ProcessMessage(this, Message.Destroy);
                }

                if (ActionId is Action.WakeUp_Right or Action.WakeUp_Left && IsActionFinished)
                    ActionId = IsFacingRight ? Action.FlyIntoSpace_Right : Action.FlyIntoSpace_Left;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}