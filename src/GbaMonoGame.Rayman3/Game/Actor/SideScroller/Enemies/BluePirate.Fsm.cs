using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class BluePirate
{
    private bool FsmStep_CheckDeath()
    {
        if (IsInvulnerable && GameTime.ElapsedFrames - InvulnerabilityTimer > 120)
            IsInvulnerable = false;

        // Damage main actor
        if (Scene.IsHitMainActor(this))
        {
            Scene.MainActor.ReceiveDamage(AttackPoints);
            Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt, this);
        }

        // Killed
        if (HitPoints == 0)
        {
            State.MoveTo(_Fsm_Dying);
            return false;
        }

        // Taken damage
        if (HitPoints < PrevHitPoints)
        {
            PrevHitPoints = HitPoints;
            State.MoveTo(_Fsm_Hit);
            return false;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (GetPhysicalGroundType().IsSolid && ActionId is Action.Fall_Right or Action.Fall_Left)
                {
                    // Custom to prevent fall sounds from playing on level load when playing with all objects loaded
                    if (Scene.KeepAllObjectsActive && !AnimatedObject.IsFramed)
                        QueueFallSound = true;
                    else
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraJump_BigFoot1_Mix02);

                    ActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                }

                // Wait for landing to finish
                if (IsActionFinished && ActionId is Action.Land_Right or Action.Land_Left)
                {
                    State.MoveTo(_Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                IdleDetectionTimer = 0;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (IsFacingRight && Scene.MainActor.Position.X < Position.X)
                    ActionId = Action.Idle_Left;
                else if (IsFacingLeft && Scene.MainActor.Position.X > Position.X)
                    ActionId = Action.Idle_Right;

                // If all objects are kept active we don't want the pirate to keep attacking from off-screen
                // since it'll play sounds and use up the projectiles, meaning other enemies can't use them
                if (Scene.KeepAllObjectsActive && !AnimatedObject.IsFramed)
                    IdleDetectionTimer = 0;

                IdleDetectionTimer++;

                if (IdleDetectionTimer > 30)
                {
                    State.MoveTo(_Fsm_AttackExtend);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AttackExtend(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraAtk1_Mix01__or__PiraHurt_Mix02);
                ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.AttackExtend_Right : Action.AttackExtend_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (AnimatedObject.CurrentFrame >= 10)
                {
                    Box chainAttackBox = GetChainAttackBox(AnimatedObject.CurrentFrame switch
                    {
                        9 => 56, // Impossible condition
                        10 => 78,
                        11 => 100,
                        12 => 124,
                        _ => throw new Exception("Invalid animation frame"),
                    });
                    Box mainActorDetectionBox = Scene.MainActor.GetDetectionBox();
                    
                    if (mainActorDetectionBox.Intersects(chainAttackBox))
                        Scene.MainActor.ReceiveDamage(AttackPoints);
                }

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_AttackWait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AttackWait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.AttackWait_Right : Action.AttackWait_Left;
                IdleDetectionTimer = 0;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                IdleDetectionTimer++;

                Box chainAttackBox = GetChainAttackBox(140);
                Box mainActorDetectionBox = Scene.MainActor.GetDetectionBox();

                if (mainActorDetectionBox.Intersects(chainAttackBox))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

                if (IdleDetectionTimer > 10)
                {
                    State.MoveTo(_Fsm_AttackRetract);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AttackRetract(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.AttackRetract_Right : Action.AttackRetract_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (!IsActionFinished && AnimatedObject.CurrentFrame <= 4)
                {
                    Box chainAttackBox = GetChainAttackBox(AnimatedObject.CurrentFrame switch
                    {
                        0 => 140,
                        1 => 124,
                        2 => 100,
                        3 => 78,
                        4 => 56,
                        _ => throw new Exception("Invalid animation frame"),
                    });
                    Box mainActorDetectionBox = Scene.MainActor.GetDetectionBox();

                    if (mainActorDetectionBox.Intersects(chainAttackBox))
                        Scene.MainActor.ReceiveDamage(AttackPoints);
                }

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Idle);
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
                if (HitFromFront)
                    ActionId = IsFacingRight ? Action.HitBehind_Right : Action.HitBehind_Left;
                else
                    ActionId = IsFacingRight ? Action.Hit_Right : Action.Hit_Left;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraHit1_Mix02__or__PiraHit3_Mix03);
                IdleDetectionTimer = 0;
                StartInvulnerability();
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                IdleDetectionTimer++;

                if (IdleDetectionTimer > 30)
                {
                    State.MoveTo(_Fsm_AttackExtend);
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
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraDead_Mix05);
                IsSolid = false;
                LevelMusicManager.StopSpecialMusic();
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    SpawnRedLum(0);
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