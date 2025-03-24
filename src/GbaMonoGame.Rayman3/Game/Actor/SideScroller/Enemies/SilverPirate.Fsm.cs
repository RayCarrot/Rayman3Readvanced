using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class SilverPirate
{
    private bool FsmStep_CheckDeath()
    {
        // Invulnerability lasts 2 seconds
        if (IsInvulnerable && GameTime.ElapsedFrames - InvulnerabilityTimer > 120)
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
                AttackTimer = GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (IsFacingRight && Scene.MainActor.Position.X < Position.X)
                    ActionId = Action.Idle_Left;
                else if (IsFacingLeft && Scene.MainActor.Position.X > Position.X)
                    ActionId = Action.Idle_Right;

                // Don't jump on fist thrown if all objects are kept active and it's not framed, otherwise
                // you keep hearing the sound each time you punch
                bool disableJumping = Scene.KeepAllObjectsActive && !AnimatedObject.IsFramed;

                if (!disableJumping &&
                    ((Rayman)Scene.MainActor).ActionId is 
                    Rayman.Action.EndChargeFist_Right or Rayman.Action.EndChargeFist_Left or 
                    Rayman.Action.EndChargeSecondFist_Right or Rayman.Action.EndChargeSecondFist_Left)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }

                if (GameTime.ElapsedFrames - AttackTimer > 60 && Scene.IsDetectedMainActor(this))
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

    public bool Fsm_Jump(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Jump_Right : Action.Jump_Left;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if (Scene.GetPhysicalType(Position).IsSolid)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraJump_BigFoot1_Mix02);
                break;
        }

        return true;
    }

    public bool Fsm_Attack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Rayman rayman = (Rayman)Scene.MainActor;
                HighShot = Position.Y >= Scene.MainActor.Position.Y && rayman.State != rayman.Fsm_Crouch && rayman.State != rayman.Fsm_Crawl;

                if (HighShot)
                    ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.ShootHigh_Right : Action.ShootHigh_Left;
                else
                    ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.ShootLow_Right : Action.ShootLow_Left;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraAtk1_Mix01__or__PiraHurt_Mix02);
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                if ((AnimatedObject.CurrentFrame == 7 && ActionId is Action.ShootHigh_Right or Action.ShootHigh_Left) ||
                    (AnimatedObject.CurrentFrame == 8 && ActionId is Action.ShootLow_Right or Action.ShootLow_Left))
                {
                    if (!AnimatedObject.IsDelayMode)
                        Shoot();
                }

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
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraHit1_Mix02__or__PiraHit3_Mix03);
                if (HitFromFront)
                    ActionId = IsFacingRight ? Action.HitBehind_Right : Action.HitBehind_Left;
                else
                    ActionId = IsFacingRight ? Action.Hit_Right : Action.Hit_Left;
                DoubleHitTimer = GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (!FsmStep_CheckDeath())
                    return false;

                // Allow 20 frames to be double hit and cause a hit knock-back
                if (Scene.GetPhysicalType(Position).IsSolid && GameTime.ElapsedFrames - DoubleHitTimer > 20)
                {
                    StartInvulnerability();
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

    public bool Fsm_HitKnockBack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                KnockBackYPosition = Position.Y;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraHit1_Mix02__or__PiraHit3_Mix03);
                ActionId = IsFacingRight ? Action.HitKnockBack1_Right : Action.HitKnockBack1_Left;
                StartInvulnerability();
                CheckAgainstMapCollision = false;
                break;

            case FsmAction.Step:
                LevelMusicManager.PlaySpecialMusicIfDetected(this);
                PhysicalType type = Scene.GetPhysicalType(Position);

                if (IsActionFinished)
                    ActionId = IsFacingRight ? Action.HitKnockBack2_Right : Action.HitKnockBack2_Left;

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
                    State.MoveTo(Fsm_Attack);
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
                if (HitFromFront)
                    ActionId = IsFacingRight ? Action.DyingBehind_Right : Action.DyingBehind_Left;
                else
                    ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;

                IsSolid = false;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraHit1_Mix02__or__PiraHit3_Mix03);
                LevelMusicManager.StopSpecialMusic();
                break;

            case FsmAction.Step:
                if (Scene.GetPhysicalType(Position).Value is
                        PhysicalTypeValue.InstaKill or
                        PhysicalTypeValue.Damage or
                        PhysicalTypeValue.Water or
                        PhysicalTypeValue.MoltenLava ||
                    Scene.GetPhysicalType(Position).IsSolid)
                {
                    MechModel.Reset();
                }

                if (!AnimatedObject.IsDelayMode && AnimatedObject.CurrentFrame == 5)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraDead_Mix05);

                if (IsActionFinished)
                {
                    SpawnRedLum(32);
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