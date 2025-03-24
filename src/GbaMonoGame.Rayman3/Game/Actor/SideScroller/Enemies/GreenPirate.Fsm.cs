using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class GreenPirate
{
    private bool FsmStep_CheckDeath()
    {
        if (IsInvulnerable && GameTime.ElapsedFrames - InvulnerabilityTimer > 135)
            IsInvulnerable = false;

        // Damage main actor
        if (Scene.IsHitMainActor(this))
        {
            Scene.MainActor.ReceiveDamage(AttackPoints);
            Scene.MainActor.ProcessMessage(this, Message.Damaged, this);
        }

        // Killed
        if (HitPoints == 0)
        {
            State.MoveTo(Fsm_Dying);
            return false;
        }

        // Taken damage
        if (HitPoints < PrevHitPoints && State != Fsm_Hit)
        {
            PrevHitPoints = HitPoints;
            State.MoveTo(Fsm_Hit);
            return false;
        }

        // Taken damage multiple times in quick succession
        if (HitPoints < PrevHitPoints)
        {
            PrevHitPoints = HitPoints;
            State.MoveTo(Fsm_HitKnockBack);
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

                // If all objects are kept active we want to wait with having the pirate fall until it's framed
                if (Scene.KeepAllObjectsActive && !AnimatedObject.IsFramed)
                {
                    Position = Resource.Pos.ToVector2();
                }
                else
                {
                    if (GetPhysicalGroundType().IsSolid && ActionId is Action.Fall_Right or Action.Fall_Left)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraJump_BigFoot1_Mix02);
                        ActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    }
                }

                // Wait for landing to finish
                if (IsActionFinished && ActionId is Action.Land_Right or Action.Land_Left)
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

                if (IdleDetectionTimer > 50)
                {
                    State.MoveTo(Fsm_Attack);
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
                // First high, then low
                if (!IsSecondAttack)
                    ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.ShootHigh_Right : Action.ShootHigh_Left;
                else
                    ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.ShootLow_Right : Action.ShootLow_Left;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraAtk1_Mix01__or__PiraHurt_Mix02);
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (!AnimatedObject.IsDelayMode)
                {
                    if ((AnimatedObject.CurrentFrame == 7 && ActionId is Action.ShootHigh_Right or Action.ShootHigh_Left) ||
                        (AnimatedObject.CurrentFrame == 8 && ActionId is Action.ShootLow_Right or Action.ShootLow_Left))
                    {
                        Shoot(!IsSecondAttack);
                    }
                }

                if (IsActionFinished && !IsSecondAttack)
                {
                    IsSecondAttack = true;
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                if (IsActionFinished)
                {
                    IsSecondAttack = false;
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
                if (HitFromFront)
                    ActionId = IsFacingRight ? Action.HitBehind_Right : Action.HitBehind_Left;
                else
                    ActionId = IsFacingRight ? Action.Hit_Right : Action.Hit_Left;
                
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraHit1_Mix02__or__PiraHit3_Mix03);
                IdleDetectionTimer = 0;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                IdleDetectionTimer++;

                if (IdleDetectionTimer > 30)
                {
                    StartInvulnerability();
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

    public bool Fsm_HitKnockBack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                KnockBackYPosition = Position.Y;
                ActionId = IsFacingRight ? Action.HitKnockBack_Right : Action.HitKnockBack_Left;
                StartInvulnerability();
                CheckAgainstMapCollision = false;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraHit1_Mix02__or__PiraHit3_Mix03);
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);
                PhysicalType type = Scene.GetPhysicalType(Position);

                if (IsActionFinished)
                    ActionId = IsFacingRight ? Action.Hit_Right : Action.Hit_Left;

                if (type.Value is
                        PhysicalTypeValue.InstaKill or
                        PhysicalTypeValue.Damage or
                        PhysicalTypeValue.Water or
                        PhysicalTypeValue.MoltenLava ||
                    (type.IsSolid && KnockBackYPosition + 16 < Position.Y))
                {
                    State.MoveTo(Fsm_Dying);
                    return false;
                }

                if (type.IsSolid)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                CheckAgainstMapCollision = true;
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
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraHit1_Mix02__or__PiraHit3_Mix03);
                IsSolid = false;
                LevelMusicManager.StopSpecialMusic();
                break;

            case FsmAction.Step:
                if (!AnimatedObject.IsDelayMode && AnimatedObject.CurrentFrame == 5)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraDead_Mix05);

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