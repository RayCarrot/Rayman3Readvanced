using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class WoodenShieldedHoodboom
{
    private bool FsmStep_CheckDeath()
    {
        if (IsInvulnerable && GameTime.ElapsedFrames - InvulnerabilityTimer > 120)
            IsInvulnerable = false;

        if (Scene.IsHitMainActor(this))
        {
            Scene.MainActor.ReceiveDamage(AttackPoints);
            Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt, this);
        }

        if (HitPoints == 0)
        {
            State.MoveTo(Fsm_Dying);
            return false;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (HasShield)
                {
                    ActionId = IsFacingRight ? Action.ShieldedIdle_Right : Action.ShieldedIdle_Left;
                }
                else
                {
                    if (!IsShieldDestroyed)
                    {
                        ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                    }
                    else
                    {
                        TauntFlag = TauntFlag == false;

                        if (TauntFlag)
                        {
                            ActionId = IsFacingRight ? Action.Taunt_Right : Action.Taunt_Left;
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagOno01_Mix01);
                        }
                        else
                        {
                            ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                        }
                    }
                }

                if (JustHitShield)
                {
                    DoQuickAttack = true;
                    JustHitShield = false;
                }
                else
                {
                    DoQuickAttack = false;
                }

                Timer = GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                bool readyToAttack = (!TauntFlag && IsShieldDestroyed) || 
                                     (DoQuickAttack && 25 < GameTime.ElapsedFrames - Timer) || 
                                     100 < GameTime.ElapsedFrames - Timer;

                float addLeft;
                float addRight;
                if (Position.X - Scene.MainActor.Position.X < 0)
                {
                    addLeft = 0;
                    addRight = 100;
                }
                else
                {
                    addLeft = -100;
                    addRight = 0;
                }

                bool detectedMainActor = Scene.IsDetectedMainActor(this, 0, 0, addLeft, addRight);

                if ((detectedMainActor && readyToAttack) || 
                    (IsActionFinished && IsShieldDestroyed))
                {
                    State.MoveTo(Fsm_PrepareGrenade);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_PrepareGrenade(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (HasShield)
                    ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.ShieldedThrowGrenade_Right : Action.ShieldedThrowGrenade_Left;
                else
                    ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.ThrowGrenade_Right : Action.ThrowGrenade_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (AnimatedObject.CurrentFrame == 10)
                {
                    State.MoveTo(Fsm_ThrowGrenade);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrowGrenade(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Grenade grenade = Scene.CreateProjectile<Grenade>(ActorType.Grenade);

                Debug.Assert(grenade != null, "The grenade projectile could not be created");

                if (grenade != null)
                {
                    float xPos;
                    if (IsFacingRight)
                    {
                        if (Scene.IsDetectedMainActor(this))
                            grenade.ActionId = Grenade.Action.ShortThrow_Right;
                        else if (Scene.IsDetectedMainActor(this, 0, 0, 0, 20))
                            grenade.ActionId = Grenade.Action.NormalThrow_Right;
                        else
                            grenade.ActionId = Grenade.Action.LongThrow_Right;

                        xPos = 45;
                    }
                    else
                    {
                        if (Scene.IsDetectedMainActor(this))
                            grenade.ActionId = Grenade.Action.ShortThrow_Left;
                        else if (Scene.IsDetectedMainActor(this, 0, 0, -20, 0))
                            grenade.ActionId = Grenade.Action.NormalThrow_Left;
                        else
                            grenade.ActionId = Grenade.Action.LongThrow_Left;

                        xPos = -45;
                    }

                    grenade.Position = Position + new Vector2(xPos, -85);
                    grenade.ChangeAction();
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
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

    public bool Fsm_Hit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (IsShieldDestroyed)
                {
                    // This is unused since the enemy always has 1 hit-point meaning it'll die here
                    ActionId = IsFacingRight ? Action.Dizzy_Right : Action.Dizzy_Left;
                    ChangeAction();
                    StartInvulnerability();
                }
                else
                {
                    // First hit
                    if (!JustHitShield)
                    {
                        ActionId = IsFacingRight ? Action.ShieldedHitShield_Right : Action.ShieldedHitShield_Left;
                        ChangeAction();
                        JustHitShield = true;

                        if (HitPoints != 0)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodImp_Mix03);
                    }
                    // Second hit
                    else
                    {
                        ActionId = IsFacingRight ? Action.ShieldedBreakShield_Right : Action.ShieldedBreakShield_Left;
                        ChangeAction();

                        if (HasShield)
                            PerformHitKnockback = true;

                        IsShieldDestroyed = true;
                        HasShield = false;

                        if (HitPoints != 0)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodImp_Mix03);

                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagoTurn_Mix03);
                    }
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                // Perform knock-back
                if (AnimatedObject.CurrentFrame > 2 && PerformHitKnockback) 
                {
                    if (IsFacingRight)
                        Position -= new Vector2(1, 0);
                    else
                        Position += new Vector2(1, 0);

                    // Finish knock-back
                    if (AnimatedObject.CurrentFrame == 8)
                        PerformHitKnockback = false;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                PerformHitKnockback = false;
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
                
                IsInvulnerable = false;
                IsSolid = false;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Boing_Mix02);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagoDie2_Mix01);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    ProcessMessage(this, Message.Destroy);
                    LevelMusicManager.StopSpecialMusic();
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}