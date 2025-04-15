using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Grolgoth
{
    private bool FsmStep_CheckHitMainActor()
    {
        if (Scene.IsHitMainActor(this))
        {
            // Why is there a check for a non-boss map?
            if (ActionId is Action.Ground_FallDown_Right or Action.Ground_FallDown_Left || GameInfo.MapId == MapId.IronMountains_M2)
            {
                Scene.MainActor.ProcessMessage(this, Message.Exploded);
            }
            else
            {
                Scene.MainActor.ProcessMessage(this, Message.Damaged);
                Scene.MainActor.ReceiveDamage(AttackPoints);
            }
        }

        // Force the camera to the top-left to avoid showing empty row of tiles at the bottom (why are they even there though?)
        if (GameInfo.MapId == MapId.BossFinal_M1)
            Scene.Camera.ProcessMessage(this, Message.Cam_MoveToTarget, Vector2.Zero);

        return true;
    }

    public bool Fsm_Invalid(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                State.MoveTo(Fsm_GroundDefault);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_GroundDefault(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = IsFacingRight ? Action.Ground_Idle_Right : Action.Ground_Idle_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;

                int rand = Random.GetNumber(101);

                // Deploy bomb
                if (IsActionFinished && Timer > 5 && AttackCount == 0)
                {
                    State.MoveTo(Fsm_GroundDeployBomb);
                    return false;
                }

                // Shoot energy shots
                if ((AttackCount != 0 && BossHealth < 3 && rand > 25) || 
                    (BossHealth > 2 && AttackCount > 1))
                {
                    State.MoveTo(Fsm_GroundShootEnergyShots);
                    return false;
                }

                // Shoot lasers
                if ((AttackCount != 0 && BossHealth < 3 && rand <= 25) ||
                    (AttackCount == 1 && BossHealth >= 3 && BossHealth != 5))
                {
                    State.MoveTo(Fsm_GroundShootLasers);
                    return false;
                }

                // Deploy bomb
                if (IsActionFinished && Timer > 5 && AttackCount == 1 && BossHealth == 5)
                {
                    State.MoveTo(Fsm_GroundDeployBomb);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_GroundShootEnergyShots(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (Random.GetNumber(11) > 5)
                    ActionId = IsFacingRight ? Action.Ground_BeginShootEnergyShotsHigh_Right : Action.Ground_BeginShootEnergyShotsHigh_Left;
                else
                    ActionId = IsFacingRight ? Action.Ground_BeginShootEnergyShotsLow_Right : Action.Ground_BeginShootEnergyShotsLow_Left;

                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                // Begin shooting
                if (ActionId is 
                    Action.Ground_BeginShootEnergyShotsHigh_Right or Action.Ground_BeginShootEnergyShotsHigh_Left or
                    Action.Ground_BeginShootEnergyShotsLow_Right or Action.Ground_BeginShootEnergyShotsLow_Left)
                {
                    Timer++;

                    if (Timer > 20)
                    {
                        if (ActionId is Action.Ground_BeginShootEnergyShotsHigh_Right or Action.Ground_BeginShootEnergyShotsHigh_Left)
                            ActionId = IsFacingRight ? Action.Ground_ShootEnergyShotsHigh_Right : Action.Ground_ShootEnergyShotsHigh_Left;
                        else if (ActionId is Action.Ground_BeginShootEnergyShotsLow_Right or Action.Ground_BeginShootEnergyShotsLow_Left)
                            ActionId = IsFacingRight ? Action.Ground_ShootEnergyShotsLow_Right : Action.Ground_ShootEnergyShotsLow_Left;

                        ChangeAction();
                        Timer = 0;
                    }
                }
                // Shoot
                else if ((ActionId is Action.Ground_ShootEnergyShotsHigh_Right or Action.Ground_ShootEnergyShotsHigh_Left && AnimatedObject.CurrentFrame == 8) || 
                         (ActionId is Action.Ground_ShootEnergyShotsLow_Right or Action.Ground_ShootEnergyShotsLow_Left && AnimatedObject.CurrentFrame == 2))
                {
                    if (Timer == 0)
                    {
                        Timer = 1;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossVO03_Mix01);
                        ShootFromGround();
                    }
                }

                if (IsActionFinished && ActionId is 
                        Action.Ground_ShootEnergyShotsHigh_Right or Action.Ground_ShootEnergyShotsHigh_Left or 
                        Action.Ground_ShootEnergyShotsLow_Right or Action.Ground_ShootEnergyShotsLow_Left)
                {
                    State.MoveTo(Fsm_GroundDefault);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_GroundShootLasers(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SavedAttackCount = AttackCount;
                AttackCount = 4;
                ActionId = IsFacingRight ? Action.Ground_BeginShootLasers_Right : Action.Ground_BeginShootLasers_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;

                if (ActionId is Action.Ground_ShootLasers_Right or Action.Ground_ShootLasers_Left && Timer > 10 && AttackCount != 0)
                {
                    ShootFromGround();
                    Timer = 0;
                }
                
                if (IsActionFinished)
                {
                    if (AttackCount == 0 && ActionId is Action.Ground_ShootLasers_Right or Action.Ground_ShootLasers_Left)
                    {
                        ActionId = IsFacingRight ? Action.Ground_EndShootLasers_Right : Action.Ground_EndShootLasers_Left;
                        ChangeAction();
                    }
                    else if (ActionId is Action.Ground_BeginShootLasers_Right or Action.Ground_BeginShootLasers_Left)
                    {
                        ActionId = IsFacingRight ? Action.Ground_ShootLasers_Right : Action.Ground_ShootLasers_Left;
                        Timer = 0;
                    }
                }

                if (IsActionFinished && ActionId is Action.Ground_EndShootLasers_Right or Action.Ground_EndShootLasers_Left)
                {
                    State.MoveTo(Fsm_GroundDefault);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SavedAttackCount--;
                AttackCount = SavedAttackCount;
                break;
        }

        return true;
    }

    public bool Fsm_GroundDeployBomb(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Ground_BeginDeployBomb_Right : Action.Ground_BeginDeployBomb_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                AttackCount = 1;

                if (ActionId is Action.Ground_BeginDeployBomb_Right or Action.Ground_BeginDeployBomb_Left && IsActionFinished)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachAtk2_Mix02);

                    if (BossHealth < 3)
                        DeployBigGroundBombs();
                    else
                        DeploySmallGroundBomb();

                    ActionId = IsFacingRight ? Action.Ground_DeployBomb_Right : Action.Ground_DeployBomb_Left;
                }
                break;

            case FsmAction.UnInit:
                AttackCount = BossHealth switch
                {
                    1 => 8,
                    2 => 7,
                    3 => 6,
                    4 => 5,
                    _ => 4
                };
                break;
        }

        return true;
    }

    public bool Fsm_GroundHit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                BossHealth--;
                ((FrameSideScroller)Frame.Current).UserInfo.BossHit();
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02);
                ActionId = IsFacingRight ? Action.Ground_Hit1_Right : Action.Ground_Hit1_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;

                if (IsActionFinished)
                {
                    if (ActionId is Action.Ground_Hit1_Right or Action.Ground_Hit1_Left)
                    {
                        ActionId = IsFacingRight ? Action.Ground_Hit2_Right : Action.Ground_Hit2_Left;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossVO03_Mix01);
                    }
                    else if (ActionId is Action.Ground_Hit2_Right or Action.Ground_Hit2_Left)
                    {
                        ActionId = IsFacingRight ? Action.Ground_Hit3_Right : Action.Ground_Hit3_Left;
                        Timer = 0;
                    }
                    else if (ActionId is Action.Ground_Hit3_Right or Action.Ground_Hit3_Left && Timer > 30)
                    {
                        ActionId = IsFacingRight ? Action.Ground_Hit4_Right : Action.Ground_Hit4_Left;
                        ChangeAction();
                    }
                }

                if (IsActionFinished && ActionId is Action.Ground_Hit4_Right or Action.Ground_Hit4_Left)
                {
                    State.MoveTo(Fsm_GroundFlyUp);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_GroundFlyUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Ground_PrepareFlyUp_Right : Action.Ground_PrepareFlyUp_Left;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (ActionId is Action.Ground_PrepareFlyUp_Right or Action.Ground_PrepareFlyUp_Left)
                {
                    if (IsActionFinished)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01);
                        ActionId = IsFacingRight ? Action.Ground_FlyUp_Right : Action.Ground_FlyUp_Left;
                    }
                }
                else if (ActionId is Action.Ground_FlyUp_Right or Action.Ground_FlyUp_Left)
                {
                    // Check if off-screen
                    if (!Scene.Camera.IsActorFramed(this))
                    {
                        ActionId = IsFacingRight ? Action.Ground_FallDown_Left : Action.Ground_FallDown_Right;
                        Position = Position with
                        {
                            X = Rom.Platform switch
                            {
                                Platform.GBA => IsFacingRight ? 200 : 40,
                                Platform.NGage => IsFacingRight ? 151 : 25,
                                _ => throw new UnsupportedPlatformException()
                            }
                        };
                        Timer = 0;
                    }
                }

                if (ActionId is Action.Ground_FallDown_Right or Action.Ground_FallDown_Left && BossHealth != 0)
                {
                    State.MoveTo(Fsm_GroundFallDown);
                    return false;
                }

                if (ActionId is Action.Ground_FallDown_Right or Action.Ground_FallDown_Left && BossHealth == 0)
                {
                    State.MoveTo(Fsm_GroundDying);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_GroundFallDown(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;

                switch (BossHealth)
                {
                    case 1:
                        AttackCount = 2;
                        DeployBigGroundBombs();
                        break;

                    case 2:
                        AttackCount = 1;
                        DeployBigGroundBombs();
                        break;

                    case 3:
                        AttackCount = Rom.Platform switch
                        {
                            Platform.GBA => 6,
                            Platform.NGage => 5,
                            _ => throw new UnsupportedPlatformException(),
                        };
                        DeployFallingBombs();
                        break;

                    case 4:
                        AttackCount = Rom.Platform switch
                        {
                            Platform.GBA => 4,
                            Platform.NGage => 3,
                            _ => throw new UnsupportedPlatformException(),
                        };
                        DeployFallingBombs();
                        break;

                    default:
                        AttackCount = 0;
                        break;
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                // Keep in the air if still attacking
                if (AttackCount != 0)
                {
                    Position -= new Vector2(0, 4);
                }
                // Land
                else if (InitialYPosition < Position.Y)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BigFoot1_Mix02);
                    Scene.Camera.ProcessMessage(this, Message.Cam_Shake, 96);
                    Position = Position with { Y = InitialYPosition };
                    ActionId = IsFacingRight ? Action.Ground_Land_Right : Action.Ground_Land_Left;
                    ChangeAction();
                }

                if (ActionId is Action.Ground_Land_Right or Action.Ground_Land_Left && IsActionFinished)
                {
                    State.MoveTo(Fsm_GroundDefault);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Propulse_Combust1_Mix02);
                AttackCount = BossHealth switch
                {
                    1 => 8,
                    2 => 7,
                    3 => 6,
                    4 => 5,
                    _ => 4
                };
                break;
        }

        return true;
    }

    public bool Fsm_GroundDying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                Scene.MainActor.ProcessMessage(this, Message.Main_LevelEnd);
                ProcessMessage(this, Message.Destroy);
                State.MoveTo(Fsm_GroundDefault);
                return false;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AirInit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                Scene.MainActor.ProcessMessage(this, Message.Main_Stop);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;

                if (Timer == 60)
                {
                    Vector2 camTarget = Position + new Vector2(-60, 20);
                    Position = Position with { Y = InitialYPosition - 250 };
                    Scene.Camera.ProcessMessage(this, Message.Cam_MoveToTarget, camTarget);
                }
                else if (Timer == 120)
                {
                    Position = Position with { Y = InitialYPosition };
                    ActionId = Action.Air_FallDown_Left;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachAtk1_Mix01);
                    ChangeAction();
                }

                if (IsActionFinished && ActionId == Action.Air_FallDown_Left)
                {
                    State.MoveTo(Fsm_AirShootMissile);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AirDefault(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;

                if (BossHealth == 5)
                    ActionId = IsFacingRight ? Action.Air_Idle_Right : Action.Air_Idle_Left;
                else
                    ActionId = IsFacingRight ? Action.Air_IdleDamaged_Right : Action.Air_IdleDamaged_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;
                if ((Timer > 300 && BossHealth > 1) ||
                    (Timer > 75 && BossHealth == 1))
                {
                    State.MoveTo(Fsm_AirShootMissile);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AirShootMissile(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = IsFacingRight ? Action.Air_ShootMissile_Right : Action.Air_ShootMissile_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (Timer == 0 && AnimatedObject.CurrentFrame == 4)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossVO03_Mix01);

                if (Timer == 0 && AnimatedObject.CurrentFrame == 5)
                {
                    ShootMissile();
                    Timer = 1;
                    
                    if (GameInfo.LastGreenLumAlive == 0)
                    {
                        Scene.Camera.ProcessMessage(this, Message.Cam_MoveToLinkedObject);
                        GameInfo.GreenLumTouchedByRayman(0, Vector2.Zero);
                    }
                }
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_AirDefault);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AttackCount = 8;
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_AirUnused(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Action59 : Action.Action26;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_AirDefault);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AirHit1(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = IsFacingRight ? Action.Air_Hit1_Right : Action.Air_Hit1_Left;
                
                BossHealth--;
                ((FrameSideScroller)Frame.Current).UserInfo.BossHit();

                Scene.Camera.LinkedObject = this;
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 80);
                Scene.MainActor.ProcessMessage(this, Message.Main_Stop);
                break;

            case FsmAction.Step:
                Timer++;

                if (Timer == 20)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02);

                Box detectionBox = GetDetectionBox();

                PhysicalType type = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.MaxY - 30));
                
                if (IsActionFinished && ActionId is Action.Air_Hit1_Right or Action.Air_Hit1_Left)
                    ActionId = IsFacingRight ? Action.Air_Hit2_Right : Action.Air_Hit2_Left;

                if (type == PhysicalTypeValue.InstaKill && BossHealth != 0)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02);
                    State.MoveTo(Fsm_AirHit2);
                    return false;
                }

                if (type == PhysicalTypeValue.InstaKill && BossHealth == 0)
                {
                    State.MoveTo(Fsm_AirDying);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BigSplsh_SplshGen_Mix04);
                Timer = 0;
                break;
        }

        return true;
    }

    public bool Fsm_AirHit2(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                Position = Position with
                {
                    Y = Rom.Platform switch
                    {
                        Platform.GBA => 42,
                        Platform.NGage => 90,
                        _ => throw new UnsupportedPlatformException()
                    }
                };
                ActionId = IsFacingRight ? Action.Air_Hit3_Right : Action.Air_Hit3_Left;
                break;

            case FsmAction.Step:
                Timer++;

                if (Timer == 90)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02);

                if (IsActionFinished)
                {
                    if (ActionId is Action.Air_Hit3_Right or Action.Air_Hit3_Left)
                    {
                        ActionId = IsFacingRight ? Action.Air_Hit4_Right : Action.Air_Hit4_Left;
                        Timer = 0;
                    }
                    else if (ActionId is Action.Air_Hit4_Right or Action.Air_Hit4_Left && Timer > 120)
                    {
                        ActionId = IsFacingRight ? Action.Air_BeginFlyUp_Right : Action.Air_BeginFlyUp_Left;
                        ChangeAction();
                    }
                }

                if (ActionId is Action.Air_BeginFlyUp_Right or Action.Air_BeginFlyUp_Left && IsActionFinished)
                {
                    State.MoveTo(Fsm_AirAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Laser4_Mix01);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01);
                break;
        }

        return true;
    }

    public bool Fsm_AirAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Air_FlyUp_Right : Action.Air_FlyUp_Left;
                AttackCount = 0xFF;
                break;

            case FsmAction.Step:
                if (ActionId is Action.Air_Idle_Right or Action.Air_Idle_Left && AttackCount == 0xff)
                {
                    if (BossHealth < 2)
                    {
                        if (Scene.MainActor.Position.Y > 75)
                        {
                            AttackCount = Rom.Platform switch
                            {
                                Platform.GBA => 10,
                                Platform.NGage => 6,
                                _ => throw new UnsupportedPlatformException()
                            };
                            DeployFallingBombs();
                        }
                    }
                    else
                    {
                        if (Scene.MainActor.Position.X is > 100 and < 380)
                        {
                            if (BossHealth == 4)
                            {
                                AttackCount = 3;
                                ShootFromAir();
                            }
                            else if (BossHealth == 3)
                            {
                                AttackCount = 3;
                                ShootFromAir();
                            }
                            else if (BossHealth == 2)
                            {
                                AttackCount = 6;
                                ShootFromAir();
                            }
                        }
                    }
                }

                if (Position.Y < InitialYPosition - 250 && ActionId is not (Action.Air_Idle_Right or Action.Air_Idle_Left))
                {
                    Scene.MainActor.ProcessMessage(this, Message.Main_ExitStopOrCutscene);
                    Scene.Camera.LinkedObject = Scene.MainActor;
                    Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 80);

                    ActionId = IsFacingRight ? Action.Air_Idle_Left : Action.Air_Idle_Right;
                }
                else if (AttackCount == 0 && ActionId is not (Action.Air_FallDown_Right or Action.Air_FallDown_Left))
                {
                    Position = Position with { Y = InitialYPosition };
                    ChangeAction();

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachAtk1_Mix01);

                    if ((BossHealth & 1) == 0)
                    {
                        ActionId = Action.Air_FallDown_Right;
                        Position = Position with { X = 50 };
                    }
                    else
                    {
                        ActionId = Action.Air_FallDown_Left;
                        Position = Position with { X = 423 };
                    }
                }

                if (ActionId is Action.Air_FallDown_Right or Action.Air_FallDown_Left && IsActionFinished)
                {
                    Unused = 0;
                    State.MoveTo(Fsm_AirShootMissile);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AirDying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Air_Dying1_Left;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__ScalDead_Mix02);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    if (ActionId != Action.Air_Dying2_Left)
                    {
                        ActionId = Action.Air_Dying2_Left;
                    }
                    else
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Main_LevelEnd);
                        ProcessMessage(this, Message.Destroy);
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