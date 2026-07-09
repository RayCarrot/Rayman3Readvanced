using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public partial class Grolgoth
{
    private bool FsmStep_CheckHitMainActor()
    {
        if (Scene.IsHitMainActor(this))
        {
            // Why is there a check for a non-boss map?
            if (ActionId is Action.Ground_FallDown_Right or Action.Ground_FallDown_Left || Rayman3.GameInfo.MapId == MapId.IronMountains_M2)
            {
                if (!Engine.Settings.Active.Difficulty.NoInstaKills)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Explode);
                }
                else
                {
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                    Scene.MainActor.ReceiveDamage(2);
                }
            }
            else
            {
                Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                Scene.MainActor.ReceiveDamage(AttackPoints);
            }
        }

        // Force the camera to the top-left to avoid showing empty row of tiles at the bottom (why are they even there though?)
        if (Rayman3.GameInfo.MapId == MapId.BossFinal_M1)
            Scene.Camera.ProcessMessage(this, Message.Cam_MoveToTarget, GROUND_CAMERA_TARGET);

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
                State.MoveTo(_Fsm_GroundDefault);
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

                // Wait for time attack countdown
                if (Rayman3.TimeAttack.IsActive && Rayman3.TimeAttack.Mode != TimeAttackMode.Play)
                    return true;

                Timer++;

                int rand = Random.GetNumber(101);

                // If no more pending attacks...
                if (AttackCount == 0)
                {
                    // Deploy bomb if the action finished and waited at least 5 frames
                    if (IsActionFinished && Timer > GROUND_DEPLOY_BOMBS_TIME)
                    {
                        State.MoveTo(_Fsm_GroundDeployBomb);
                        return false;
                    }
                }
                // If there are pending attacks...
                else
                {
                    // Shoot energy shots if:
                    // - Boss health is 1-2 and 75% random chance
                    // - Boss health is 3-5 and it's not the last attack
                    if ((BossHealth <= 2 && rand > 25) ||
                        (BossHealth >= 3 && AttackCount > 1))
                    {
                        State.MoveTo(_Fsm_GroundShootEnergyShots);
                        return false;
                    }

                    // Shoot lasers if:
                    // - Boss health is 1-2 and 25% chance
                    // - Boss health is 3-4 and it's the last attack
                    if ((BossHealth <= 2 && rand <= 25) ||
                        (BossHealth >= 3 && BossHealth != 5 && AttackCount == 1))
                    {
                        State.MoveTo(_Fsm_GroundShootLasers);
                        return false;
                    }

                    // Deploy bomb if the action finished, waited at least 5 frames and:
                    // - Boss health is 5 and it's the last attack
                    if (IsActionFinished && BossHealth == 5 && Timer > GROUND_DEPLOY_BOMBS_TIME && AttackCount == 1)
                    {
                        State.MoveTo(_Fsm_GroundDeployBomb);
                        return false;
                    }
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

                    if (Timer > GROUND_SHOOT_ENERGY_SHOTS_TIME)
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
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BossVO03_Mix01, this);
                        ShootFromGround();
                    }
                }

                if (IsActionFinished && ActionId is 
                        Action.Ground_ShootEnergyShotsHigh_Right or Action.Ground_ShootEnergyShotsHigh_Left or 
                        Action.Ground_ShootEnergyShotsLow_Right or Action.Ground_ShootEnergyShotsLow_Left)
                {
                    State.MoveTo(_Fsm_GroundDefault);
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
                AttackCount = GROUND_SHOOT_LASERS_ATTACK_COUNT;
                ActionId = IsFacingRight ? Action.Ground_BeginShootLasers_Right : Action.Ground_BeginShootLasers_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;

                if (ActionId is Action.Ground_ShootLasers_Right or Action.Ground_ShootLasers_Left && Timer > GROUND_SHOOT_LASERS_TIME && AttackCount != 0)
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
                    State.MoveTo(_Fsm_GroundDefault);
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

                AttackCount = GROUND_DEPLOY_BOMBS_ATTACK_COUNT;

                if (ActionId is Action.Ground_BeginDeployBomb_Right or Action.Ground_BeginDeployBomb_Left && IsActionFinished)
                {
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MachAtk2_Mix02, this);

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
                    1 => GROUND_HEALTH_1_ATTACK_COUNT,
                    2 => GROUND_HEALTH_2_ATTACK_COUNT,
                    3 => GROUND_HEALTH_3_ATTACK_COUNT,
                    4 => GROUND_HEALTH_4_ATTACK_COUNT,
                    _ => GROUND_HEALTH_5_ATTACK_COUNT
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
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02, this);
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
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BossVO03_Mix01, this);
                    }
                    else if (ActionId is Action.Ground_Hit2_Right or Action.Ground_Hit2_Left)
                    {
                        ActionId = IsFacingRight ? Action.Ground_Hit3_Right : Action.Ground_Hit3_Left;
                        Timer = 0;
                    }
                    else if (ActionId is Action.Ground_Hit3_Right or Action.Ground_Hit3_Left && Timer > GROUND_HIT_TIME)
                    {
                        ActionId = IsFacingRight ? Action.Ground_Hit4_Right : Action.Ground_Hit4_Left;
                        ChangeAction();
                    }
                }

                if (IsActionFinished && ActionId is Action.Ground_Hit4_Right or Action.Ground_Hit4_Left)
                {
                    State.MoveTo(_Fsm_GroundFlyUp);
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
                        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01, this);
                        ActionId = IsFacingRight ? Action.Ground_FlyUp_Right : Action.Ground_FlyUp_Left;
                    }
                }
                else if (ActionId is Action.Ground_FlyUp_Right or Action.Ground_FlyUp_Left)
                {
                    // Check if off-screen
                    if (!Scene.Camera.IsActorFramed(this))
                    {
                        ActionId = IsFacingRight ? Action.Ground_FallDown_Left : Action.Ground_FallDown_Right;
                        Position = Position with { X = IsFacingRight ? GROUND_FALL_DOWN_RIGHT_POSITION_X : GROUND_FALL_DOWN_LEFT_POSITION_X };
                        Timer = 0;
                    }
                }

                if (ActionId is Action.Ground_FallDown_Right or Action.Ground_FallDown_Left && BossHealth != 0)
                {
                    State.MoveTo(_Fsm_GroundFallDown);
                    return false;
                }

                if (ActionId is Action.Ground_FallDown_Right or Action.Ground_FallDown_Left && BossHealth == 0)
                {
                    State.MoveTo(_Fsm_GroundDying);
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
                        AttackCount = GROUND_HEALTH_1_BIG_GROUND_BOMBS_ATTACK_COUNT;
                        DeployBigGroundBombs();
                        break;

                    case 2:
                        AttackCount = GROUND_HEALTH_2_BIG_GROUND_BOMBS_ATTACK_COUNT;
                        DeployBigGroundBombs();
                        break;

                    case 3:
                        AttackCount = GROUND_HEALTH_3_FALLING_BOMBS_ATTACK_COUNT;
                        DeployFallingBombs();
                        break;

                    case 4:
                        AttackCount = GROUND_HEALTH_4_FALLING_BOMBS_ATTACK_COUNT;
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
                    Position -= new Vector2(0, GROUND_FALL_SPEED);
                }
                // Land
                else if (InitialYPosition < Position.Y)
                {
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BigFoot1_Mix02, this);
                    Scene.Camera.ProcessMessage(this, Message.Cam_Shake, GROUND_LAND_CAMERA_SHAKE_LENGTH);
                    Position = Position with { Y = InitialYPosition };
                    ActionId = IsFacingRight ? Action.Ground_Land_Right : Action.Ground_Land_Left;
                    ChangeAction();
                }

                if (ActionId is Action.Ground_Land_Right or Action.Ground_Land_Left && IsActionFinished)
                {
                    State.MoveTo(_Fsm_GroundDefault);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__Propulse_Combust1_Mix02, this);
                AttackCount = BossHealth switch
                {
                    1 => GROUND_HEALTH_1_ATTACK_COUNT,
                    2 => GROUND_HEALTH_2_ATTACK_COUNT,
                    3 => GROUND_HEALTH_3_ATTACK_COUNT,
                    4 => GROUND_HEALTH_4_ATTACK_COUNT,
                    _ => GROUND_HEALTH_5_ATTACK_COUNT
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
                Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                ProcessMessage(this, Message.Destroy);
                State.MoveTo(_Fsm_GroundDefault);
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
                Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;

                if (Timer == AIR_INTRO_CAMERA_MOVE_TIME)
                {
                    Vector2 camTarget = Position + new Vector2(AIR_INTRO_CAMERA_MOVE_OFFSET_X, AIR_INTRO_CAMERA_MOVE_OFFSET_Y);
                    Position = Position with { Y = InitialYPosition + AIR_OFF_SCREEN_OFFSET_Y };
                    Scene.Camera.ProcessMessage(this, Message.Cam_MoveToTarget, camTarget);
                }
                else if (Timer == AIR_INTRO_FALL_DOWN_TIME)
                {
                    Position = Position with { Y = InitialYPosition };
                    ActionId = Action.Air_FallDown_Left;
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MachAtk1_Mix01, this);
                    ChangeAction();
                }

                if (IsActionFinished && ActionId == Action.Air_FallDown_Left)
                {
                    State.MoveTo(_Fsm_AirShootMissile);
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

                if (BossHealth == AIR_INITIAL_HEALTH)
                    ActionId = IsFacingRight ? Action.Air_Idle_Right : Action.Air_Idle_Left;
                else
                    ActionId = IsFacingRight ? Action.Air_IdleDamaged_Right : Action.Air_IdleDamaged_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                // Wait for time attack countdown
                if (Rayman3.TimeAttack.IsActive && Rayman3.TimeAttack.Mode != TimeAttackMode.Play)
                    return true;

                Timer++;
                if ((Timer > AIR_SHOOT_MISSILE_DEFAULT_TIME && BossHealth > 1) ||
                    (Timer > AIR_SHOOT_MISSILE_HEALTH_1_TIME && BossHealth == 1))
                {
                    State.MoveTo(_Fsm_AirShootMissile);
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
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BossVO03_Mix01, this);

                if (Timer == 0 && AnimatedObject.CurrentFrame == 5)
                {
                    ShootMissile();
                    Timer = 1;
                    
                    if (Rayman3.GameInfo.LastGreenLumAlive == 0)
                    {
                        Scene.Camera.ProcessMessage(this, Message.Cam_MoveToLinkedObject);
                        Rayman3.GameInfo.GreenLumTouchedByRayman(0, Vector2.Zero);
                    }
                }

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_AirDefault);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AttackCount = AIR_INITIAL_ATTACK_COUNT; // Unused
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
                    State.MoveTo(_Fsm_AirDefault);
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
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, AIR_HIT_CAMERA_TARGET_Y);
                Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
                break;

            case FsmAction.Step:
                Timer++;

                if (Timer == AIR_HIT_SOUND_TIME)
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02, this);

                Box detectionBox = GetDetectionBox();

                PhysicalType type = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom + AIR_HIT_DETECTION_BOX_BOTTOM_OFFSET));
                
                if (IsActionFinished && ActionId is Action.Air_Hit1_Right or Action.Air_Hit1_Left)
                    ActionId = IsFacingRight ? Action.Air_Hit2_Right : Action.Air_Hit2_Left;

                if (type == PhysicalTypeValue.InstaKill && BossHealth != 0)
                {
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02, this);
                    State.MoveTo(_Fsm_AirHit2);
                    return false;
                }

                if (type == PhysicalTypeValue.InstaKill && BossHealth == 0)
                {
                    State.MoveTo(_Fsm_AirDying);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BigSplsh_SplshGen_Mix04, this);
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
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BossHurt_Mix02, this);

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
                    State.MoveTo(_Fsm_AirAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__Laser4_Mix01, this);
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01, this);
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
                AttackCount = AIR_INVALID_ATTACK_COUNT;
                break;

            case FsmAction.Step:
                if (ActionId is Action.Air_Idle_Right or Action.Air_Idle_Left && AttackCount == AIR_INVALID_ATTACK_COUNT)
                {
                    if (BossHealth < 2)
                    {
                        if (Scene.MainActor.Position.Y > AIR_DEPLOY_FALLING_BOMBS_MIN_POSITION_Y)
                        {
                            AttackCount = AIR_FALLING_BOMBS_ATTACK_COUNT;
                            DeployFallingBombs();
                        }
                    }
                    else
                    {
                        if (Scene.MainActor.Position.X is > AIR_SHOOT_MIN_POSITION_X and < AIR_SHOOT_MAX_POSITION_X)
                        {
                            if (BossHealth is 3 or 4)
                            {
                                AttackCount = AIR_SHOOT_ONE_SIDE_ATTACK_COUNT;
                                ShootFromAir();
                            }
                            else if (BossHealth == 2)
                            {
                                AttackCount = AIR_SHOOT_DOUBLE_SIDE_ATTACK_COUNT;
                                ShootFromAir();
                            }
                        }
                    }
                }

                if (Position.Y < InitialYPosition - AIR_OFF_SCREEN_OFFSET_Y && ActionId is not (Action.Air_Idle_Right or Action.Air_Idle_Left))
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
                    Scene.Camera.LinkedObject = Scene.MainActor;
                    Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, AIR_HIT_CAMERA_TARGET_Y);

                    ActionId = IsFacingRight ? Action.Air_Idle_Left : Action.Air_Idle_Right;
                }
                else if (AttackCount == 0 && ActionId is not (Action.Air_FallDown_Right or Action.Air_FallDown_Left))
                {
                    Position = Position with { Y = InitialYPosition };
                    ChangeAction();

                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MachAtk1_Mix01, this);

                    if ((BossHealth & 1) == 0)
                    {
                        ActionId = Action.Air_FallDown_Right;
                        Position = Position with { X = AIR_FALL_DOWN_LEFT_POSITION_X };
                    }
                    else
                    {
                        ActionId = Action.Air_FallDown_Left;
                        Position = Position with { X = AIR_FALL_DOWN_RIGHT_POSITION_X };
                    }
                }

                if (ActionId is Action.Air_FallDown_Right or Action.Air_FallDown_Left && IsActionFinished)
                {
                    Unused = 0;
                    State.MoveTo(_Fsm_AirShootMissile);
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
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__ScalDead_Mix02, this);
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
                        if (!Rayman3.TimeAttack.IsActive)
                            Rayman3.Achievements.Unlock(AchievementId.DefeatBossFinal);
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
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