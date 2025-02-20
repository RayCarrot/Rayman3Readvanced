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
            Scene.MainActor.ProcessMessage(this, Message.Damaged, this);
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
                    if ((Flags & 0x80) == 0)
                    {
                        ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                    }
                    else
                    {
                        Flags = (byte)((Flags + 1) & 0xf3);

                        if ((Flags & 1) != 0)
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

                if ((Flags & 0x40) != 0)
                    Flags = (byte)(Flags & 0x9f | 0x20);
                else
                    Flags = (byte)(Flags & 0xdf);

                Timer = GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                bool readyToAttack = (Flags & 0x81) == 0x80 || 
                                     ((Flags & 0x20) != 0 && 25 < GameTime.ElapsedFrames - Timer) || 
                                     100 < GameTime.ElapsedFrames - Timer;

                float addMinX;
                float addMaxX;
                if (Position.X - Scene.MainActor.Position.X < 0)
                {
                    addMinX = 0;
                    addMaxX = 100;
                }
                else
                {
                    addMinX = -100;
                    addMaxX = 0;
                }

                bool detectedMainActor = Scene.IsDetectedMainActor(this, 0, 0, addMinX, addMaxX);

                if ((detectedMainActor && readyToAttack) || 
                    (IsActionFinished && (Flags & 0x80) != 0))
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
                if ((Flags & 0x80) != 0)
                {
                    ActionId = IsFacingRight ? Action.Dizzy_Right : Action.Dizzy_Left;
                    ChangeAction();
                    StartInvulnerability();
                }
                else if ((Flags & 0x40) != 0)
                {
                    ActionId = IsFacingRight ? Action.ShieldedBreakShield_Right : Action.ShieldedBreakShield_Left;
                    ChangeAction();

                    if (HasShield)
                        Flags |= 0x90;
                    else
                        Flags |= 0x80;

                    HasShield = false;

                    if (HitPoints != 0)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodImp_Mix03);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagoTurn_Mix03);
                }
                else
                {
                    ActionId = IsFacingRight ? Action.ShieldedHitShield_Right : Action.ShieldedHitShield_Left;
                    ChangeAction();
                    Flags |= 0x40;

                    if (HitPoints != 0)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodImp_Mix03);
                }

                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (AnimatedObject.CurrentFrame > 2 && (Flags & 0x10) != 0) 
                {
                    if (IsFacingRight)
                        Position -= new Vector2(1, 0);
                    else
                        Position += new Vector2(1, 0);

                    if (AnimatedObject.CurrentFrame == 8)
                        Flags &= 0xef;
                }

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if ((Flags & 0x10) != 0)
                    Flags = (byte)(Flags & 0xef);
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