using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: Name functions
public partial class Grolgoth
{
    private bool FsmStep_CheckHitMainActor()
    {
        if (Scene.IsHitMainActor(this))
        {
            // Why is there a check for a non-boss map?
            if (ActionId is Action.Ground_Fall_Right or Action.Ground_Fall_Left || GameInfo.MapId == MapId.IronMountains_M2)
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

    // FUN_0804e8b4
    public bool FUN_10019370(FsmAction action)
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

                if ((AttackCount != 0 && BossHealth < 3 && rand <= 25) ||
                    (AttackCount == 1 && BossHealth >= 3 && BossHealth != 5))
                {
                    State.MoveTo(FUN_10019938);
                    return false;
                }

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

    // TODO: Implement
    // FUN_0804ef8c
    public bool FUN_10019938(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

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
                ActionId = IsFacingRight ? Action.Action10 : Action.Action11;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                Timer++;
                // TODO: Implement
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // TODO: Implement
    public bool FUN_10019f48(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // FUN_0804fbc0
    public bool FUN_1001a1d4(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;

                switch (BossHealth)
                {
                    case 1:
                        AttackCount = 2;
                        DeployFallingBombs();
                        break;

                    case 2:
                        AttackCount = 1;
                        DeployFallingBombs();
                        break;

                    case 3:
                        AttackCount = Rom.Platform switch
                        {
                            Platform.GBA => 6,
                            Platform.NGage => 5,
                            _ => throw new UnsupportedPlatformException(),
                        };
                        DeployBigGroundBombs();
                        break;

                    case 4:
                        AttackCount = Rom.Platform switch
                        {
                            Platform.GBA => 4,
                            Platform.NGage => 3,
                            _ => throw new UnsupportedPlatformException(),
                        };
                        DeployBigGroundBombs();
                        break;

                    default:
                        AttackCount = 0;
                        break;
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (AttackCount != 0)
                {
                    Position -= new Vector2(0, 4);
                }
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

    // TODO: Implement
    // ?
    public bool FUN_1001a418(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001a4ac(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001a660(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001a7a4(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001a940(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001aa10(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001acc4(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001aec8(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    // TODO: Implement
    // ?
    public bool FUN_1001b1c0(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:

                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }
}