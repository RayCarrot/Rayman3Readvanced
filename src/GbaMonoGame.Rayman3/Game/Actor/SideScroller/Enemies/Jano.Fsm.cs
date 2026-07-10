using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public partial class Jano
{
    private bool FsmStep_CheckHit()
    {
        // NOTE: In the original game this changes the alpha one step every second frame and
        //       uses direct equality checks rather than less than or greater than
        if (IsBeingAttacked())
        {
            if (AlphaBlend > ALPHA_BLEND_MIN)
            {
                AlphaBlend -= ALPHA_BLEND_DELTA;

                if (AlphaBlend <= ALPHA_BLEND_MIN && !Engine.Sem.IsSongPlaying(Rayman3SoundEvent.Play__JanoRire_Mix01))
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__JanoRire_Mix01, this);
            }
        }
        else
        {
            if (AlphaBlend < ALPHA_BLEND_MAX)
                AlphaBlend += ALPHA_BLEND_DELTA;
        }

        AnimatedObject.Alpha = AlphaCoefficient.FromGbaValue(AlphaBlend);

        return true;
    }

    public bool Fsm_TimeAttackIntro(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = Action.Idle_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                // Wait for Rayman to reach the position
                if (Scene.MainActor.Position.X > INTRO_START_POSITION_X)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Intro(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = Action.Idle_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                // Begin intro sequence when Rayman reaches its position
                if (Scene.MainActor.Position.X > INTRO_START_POSITION_X && Timer == 0)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);

                    if (Scene.MainActor.IsFacingLeft)
                        Scene.MainActor.AnimatedObject.FlipX = false;

                    Timer = 1;
                }
                // Move on screen
                else if (Timer == 1)
                {
                    if (Position.X <= INTRO_TARGET_X)
                        Timer = 2;
                    else
                        Position += new Vector2(INTRO_SPEED_X, 0);
                }
                // Wait
                else if (Timer is >= 2 and < INTRO_GRIMACE_TIME)
                {
                    Timer++;
                }
                // Grimace
                else if (Timer == INTRO_GRIMACE_TIME)
                {
                    ((Rayman)Scene.MainActor).ActionId = Rayman.Action.Idle_Grimace_Right;
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Grimace1_Mix04, this);
                    Timer = INTRO_GRIMACE_TIME + 1;
                    ActionId = Action.Grimace_Left;
                }
                // Wait
                else if (Timer > INTRO_GRIMACE_TIME)
                {
                    Timer++;
                }

                if (IsActionFinished && ActionId is Action.Grimace_Right or Action.Grimace_Left && Timer > INTRO_END_TIME)
                {
                    State.MoveTo(_Fsm_MoveAway);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Create a checkpoint to avoid showing the intro cutscene each time
                Rayman3.GameInfo.GreenLumTouchedByRayman(0, new Vector2(378, 208));
                break;
        }

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsOnLeftSide ? Action.Idle_Right : Action.Idle_Left;
                Timer = 0;
                TargetPosition = TargetPosition with { X = Scene.Playfield.Camera.Position.X + (Scene.Resolution.X - 30) };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                Timer++;

                // Move closer
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X > MAX_RAYMAN_DISTANCE_X)
                {
                    Position -= new Vector2(MOVE_CLOSER_SPEED_X, 0);
                    return true;
                }
                else if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X > MAX_RAYMAN_DISTANCE_X)
                {
                    Position += new Vector2(MOVE_CLOSER_SPEED_X, 0);
                    return true;
                }

                // Move away if Rayman gets too close
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < MIN_RAYMAN_DISTANCE_X)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < MIN_NOT_JUMPING_RAYMAN_DISTANCE_X && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < MIN_RAYMAN_DISTANCE_X)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < MIN_NOT_JUMPING_RAYMAN_DISTANCE_X && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                // Attack if there is remaining ammo
                if (Timer > ATTACK_TIME && Ammo != 0)
                {
                    State.MoveTo(_Fsm_Attack);
                    return false;
                }

                // If no ammo then create skull platform
                if (Timer > ATTACK_TIME)
                {
                    RefillAmmo();
                    State.MoveTo(_Fsm_CreateSkullPlatform);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_BeginMoveAway(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsOnLeftSide ? Action.Grimace_Right : Action.Grimace_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                float distX = Math.Abs(Position.X - Scene.MainActor.Position.X);
                if (distX < MIN_RAYMAN_DISTANCE_X && Position.X < MOVE_AWAY_MAX_POSITION_X)
                {
                    if (IsOnLeftSide)
                        Position -= new Vector2(MOVE_AWAY_SPEED_X, 0);
                    else
                        Position += new Vector2(MOVE_AWAY_SPEED_X, 0);
                }

                if (IsActionFinished && ActionId is Action.Grimace_Right or Action.Grimace_Left)
                {
                    State.MoveTo(_Fsm_MoveAway);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveAway(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (FirstTimeMovingAway)
                    FirstTimeMovingAway = false;
                else
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Janogrrr_Mix03, this);

                if (ActionId is not (Action.Move_Right or Action.Move_Left))
                    ActionId = IsOnLeftSide ? Action.TurnAroundSlow_Right : Action.TurnAroundSlow_Left;
                
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                Timer++;

                float distX = Math.Abs(Position.X - Scene.MainActor.Position.X);
                if (distX < MIN_RAYMAN_DISTANCE_X && Position.X < MOVE_AWAY_MAX_POSITION_X)
                {
                    if (IsOnLeftSide)
                        Position -= new Vector2(MOVE_AWAY_SPEED_X, 0);
                    else
                        Position += new Vector2(MOVE_AWAY_SPEED_X, 0);
                }

                // Finish initial turning around
                if (ActionId is Action.TurnAroundSlow_Right or Action.TurnAroundSlow_Left && IsActionFinished)
                {
                    ActionId = IsOnLeftSide ? Action.Move_Left : Action.Move_Right;
                    Timer = 0;
                }

                if (IsReadyToTurnBackAround() && GameTime.ElapsedFrames - Timer > 10 && ActionId is Action.Move_Right or Action.Move_Left)
                {
                    // Update y position if Rayman is not on a skull platform
                    if (Scene.MainActor.LinkedMovementActor == null)
                    {
                        Timer = (ushort)Random.GetNumber(751);

                        int phase = CheckCurrentPhase();

                        if (phase == 1)
                        {
                            if (Timer < 376)
                                Position = Position with { Y = OffsetY + HIGH_OFFSET_Y };
                            else
                                Position = Position with { Y = OffsetY + BASE_OFFSET_Y };
                        }
                        else if (phase is 2 or 3)
                        {
                            if (Timer < 250)
                                Position = Position with { Y = OffsetY + BASE_OFFSET_Y };
                            else if (Timer < 500)
                                Position = Position with { Y = OffsetY + HIGH_OFFSET_Y };
                            else
                                Position = Position with { Y = OffsetY + LOW_OFFSET_Y };
                        }
                    }

                    ActionId = IsOnLeftSide ? Action.TurnAroundFast_Right : Action.TurnAroundFast_Left;
                    ChangeAction();
                }

                if (Position.X > CHECK_END_MIN_POSITION_X)
                {
                    State.MoveTo(_Fsm_CheckComplete);
                    return false;
                }

                if (IsActionFinished && 
                    ActionId is Action.TurnAroundFast_Right or Action.TurnAroundFast_Left && 
                    Scene.MainActor.Position.X > CHECK_END_RAYMAN_MIN_POSITION_X &&
                    Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_CheckComplete);
                    return false;
                }

                if (IsActionFinished && 
                    ActionId is Action.TurnAroundFast_Right or Action.TurnAroundFast_Left)
                {
                    State.MoveTo(_Fsm_MoveBack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__Grimace1_Mix04, this);
                break;
        }

        return true;
    }

    public bool Fsm_MoveBack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsOnLeftSide ? Action.Idle_Right : Action.Idle_Left;
                Timer = 1;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                Timer++;

                // Move back
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X > MOVE_CLOSER_MIN_POSITION_X)
                    Position -= new Vector2(MOVE_CLOSER_SPEED_X, 0);
                else if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X > MOVE_CLOSER_MIN_POSITION_X)
                    Position += new Vector2(MOVE_CLOSER_SPEED_X, 0);

                if (Timer > MOVE_CLOSER_TIME && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_CreateSkullPlatform(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.CreateSkullPlatform1;
                Timer = 0;
                HasFinishedCurrentCycle = false;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                Timer++;

                bool reachedTarget = false;
                switch (CheckCurrentPhase())
                {
                    case 1:
                        if (Position.Y > OffsetY + BASE_OFFSET_Y + CREATE_SKULL_PLATFORM_SPEED_Y)
                        {
                            Position -= new Vector2(0, CREATE_SKULL_PLATFORM_SPEED_Y);
                        }
                        else if (Position.Y < OffsetY + BASE_OFFSET_Y - CREATE_SKULL_PLATFORM_SPEED_Y)
                        {
                            Position += new Vector2(0, CREATE_SKULL_PLATFORM_SPEED_Y);
                        }
                        else
                        {
                            Position = Position with { Y = OffsetY + BASE_OFFSET_Y };
                            reachedTarget = true;
                        }
                        break;

                    case 2:
                        if (Position.Y > OffsetY + HIGH_OFFSET_Y)
                        {
                            Position -= new Vector2(0, CREATE_SKULL_PLATFORM_SPEED_Y);
                        }
                        else if (Position.Y >= 20 && Position.Y < OffsetY + HIGH_OFFSET_Y)
                        {
                            Position += new Vector2(0, CREATE_SKULL_PLATFORM_SPEED_Y);
                        }
                        else
                        {
                            Position = Position with { Y = OffsetY + HIGH_OFFSET_Y };
                            reachedTarget = true;
                        }
                        break;

                    case 3:
                        if (Position.Y < OffsetY + BASE_OFFSET_Y)
                        {
                            Position += new Vector2(0, CREATE_SKULL_PLATFORM_SPEED_Y);
                        }
                        else
                        {
                            Position = Position with { Y = OffsetY + BASE_OFFSET_Y };
                            reachedTarget = true;
                        }
                        break;
                }

                int skullPlatformIndex = Array.IndexOf(SkullPlatforms, null);

                if (Position.X - Scene.MainActor.Position.X < CREATE_SKULL_PLATFORM_MIN_DISTANCE_X)
                    Position += new Vector2(CREATE_SKULL_PLATFORM_SPEED_X, 0);

                if (ActionId == Action.CreateSkullPlatform4 && AnimatedObject.CurrentFrame == 7 && !HasFinishedCurrentCycle) 
                {
                    CreateSkullPlatform(skullPlatformIndex);
                    HasFinishedCurrentCycle = true;
                }

                if (IsActionFinished)
                {
                    if (ActionId == Action.CreateSkullPlatform1)
                    {
                        ActionId = Action.CreateSkullPlatform2;
                    }
                    else if (Timer > CREATE_SKULL_PLATFORM_TIME && 
                             reachedTarget && 
                             ActionId == Action.CreateSkullPlatform2 && 
                             skullPlatformIndex != 3)
                    {
                        ActionId = Action.CreateSkullPlatform3;
                        ChangeAction();
                    }
                    else if (ActionId == Action.CreateSkullPlatform3)
                    {
                        ActionId = Action.CreateSkullPlatform4;
                        ChangeAction();
                    }
                }

                if (CheckCurrentPhase() != 3 && IsActionFinished && ActionId == Action.CreateSkullPlatform4)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }

                if (CheckCurrentPhase() == 3 && IsActionFinished && ActionId == Action.CreateSkullPlatform4)
                {
                    State.MoveTo(_Fsm_StandAside);
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
                ActionId = IsOnLeftSide ? Action.Attack_Right : Action.Attack_Left;
                HasFinishedCurrentCycle = false;
                Timer = 0;
                TargetPosition = TargetPosition with { Y = 0 };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                if (Timer != 0)
                    Timer++;

                // Move to target pos
                if (ActionId is Action.Idle_Right or Action.Idle_Left && !Single.IsNaN(TargetPosition.Y))
                {
                    if (TargetPosition.Y + ATTACK_SPEED_X < Position.Y)
                    {
                        Position -= new Vector2(0, ATTACK_SPEED_X);
                    }
                    else if (Position.Y < TargetPosition.Y - ATTACK_SPEED_X)
                    {
                        Position += new Vector2(0, ATTACK_SPEED_X);
                    }
                    else
                    {
                        Position = Position with { Y = TargetPosition.Y };
                        TargetPosition = TargetPosition with { Y = Single.NaN };
                    }
                }

                // Shoot
                if (AnimatedObject.CurrentFrame == 6 && !HasFinishedCurrentCycle)
                {
                    Shoot();
                    HasFinishedCurrentCycle = true;

                    // Set next target pos
                    int rand = Random.GetNumber(751);
                    int phase = CheckCurrentPhase();
                    if (phase == 1)
                    {
                        float offset = rand switch
                        {
                            < 376 => HIGH_OFFSET_Y,
                            _ => BASE_OFFSET_Y
                        };
                        TargetPosition = TargetPosition with { Y = OffsetY + offset };
                    }
                    else if (phase is 2 or 3)
                    {
                        float offset = rand switch
                        {
                            < 250 => BASE_OFFSET_Y,
                            < 500 => HIGH_OFFSET_Y,
                            _ => LOW_OFFSET_Y
                        };
                        TargetPosition = TargetPosition with { Y = OffsetY + offset };
                    }

                    Timer = 1;
                }

                if (IsActionFinished && Ammo != 0 && ActionId is Action.Attack_Right or Action.Attack_Left)
                    ActionId = ActionId == Action.Attack_Right ? Action.Idle_Right : Action.Idle_Left;

                // Move away if Rayman gets too close
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < MIN_RAYMAN_DISTANCE_X)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < MIN_NOT_JUMPING_RAYMAN_DISTANCE_X && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < MIN_RAYMAN_DISTANCE_X)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < MIN_NOT_JUMPING_RAYMAN_DISTANCE_X && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                // Finished shooting, not phase 3
                if (IsActionFinished && Ammo == 0 && CheckCurrentPhase() != 3)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }

                // Finished shooting, phase 3
                if (IsActionFinished && Ammo == 0 && CheckCurrentPhase() == 3)
                {
                    State.MoveTo(_Fsm_SwitchSide);
                    return false;
                }

                // Why does the game check these again??
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < MIN_RAYMAN_DISTANCE_X)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < MIN_NOT_JUMPING_RAYMAN_DISTANCE_X && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < MIN_RAYMAN_DISTANCE_X)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < MIN_NOT_JUMPING_RAYMAN_DISTANCE_X && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_BeginMoveAway);
                    return false;
                }

                // Attack again if reached target and waited half a second
                if (Single.IsNaN(TargetPosition.Y) && Timer > ATTACK_AGAIN_TIME)
                {
                    State.MoveTo(_Fsm_Attack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SwitchSide(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsOnLeftSide ? Action.Move_Right : Action.Move_Left;
                break;

            case FsmAction.Step:
                bool isFinished = false;

                if (!FsmStep_CheckHit())
                    return false;

                if (Position.Y > SWITCH_SIDE_TARGET_Y)
                    Position -= new Vector2(0, SWITCH_SIDE_SPEED_Y);
                else
                    Position = Position with { Y = SWITCH_SIDE_TARGET_Y };

                if (!IsOnLeftSide)
                {
                    if (Scene.MainActor.Position.X - Position.X > Scene.Resolution.X)
                    {
                        IsOnLeftSide = true;
                        isFinished = true;

                        Timer = (ushort)Random.GetNumber(751);
                        Position = Timer switch
                        {
                            < 250 => Position with { Y = OffsetY + BASE_OFFSET_Y },
                            < 500 => Position with { Y = OffsetY + HIGH_OFFSET_Y },
                            _ => Position with { Y = OffsetY + LOW_OFFSET_Y }
                        };
                    }
                }
                else
                {
                    if (Position.X - Scene.MainActor.Position.X > MAX_RAYMAN_DISTANCE_X)
                    {
                        IsOnLeftSide = false;
                        isFinished = true;
                    }
                }

                if (isFinished && IsOnLeftSide)
                {
                    RefillAmmo();
                    State.MoveTo(_Fsm_Default);
                    return false;
                }

                if (isFinished && !IsOnLeftSide)
                {
                    RefillAmmo();
                    State.MoveTo(_Fsm_CreateSkullPlatform);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_StandAside(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.TurnAroundSlow_Left;
                break;

            case FsmAction.Step:
                if (ActionId is Action.TurnAroundSlow_Right or Action.TurnAroundSlow_Left && IsActionFinished)
                    ActionId = Action.Move_Right;

                if (ActionId is Action.Move_Right or Action.Move_Left && ScreenPosition.X > Scene.Resolution.X + 40)
                    ActionId = Action.Idle_Left;

                bool hasActiveSkullPlatforms = false;
                foreach (JanoSkullPlatform x in SkullPlatforms)
                {
                    if (x != null)
                    {
                        hasActiveSkullPlatforms = true;
                        break;
                    }
                }

                if (Scene.MainActor.LinkedMovementActor != null)
                {
                    State.MoveTo(_Fsm_MoveAway);
                    return false;
                }

                if (!hasActiveSkullPlatforms)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_CheckComplete(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle_Left;
                Timer = 1;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                if (Position.X < CHECK_END_TARGET_X)
                    Position += new Vector2(CHECK_END_SPEED_X, 0);

                Rayman rayman = (Rayman)Scene.MainActor;
                if (CheckCurrentPhase() == 4 && 
                    Position.X > PHASE_4_POSITION_X && 
                    Scene.MainActor.Speed.Y == 0 &&
                    rayman.State != rayman._Fsm_HangOnEdge)
                {
                    State.MoveTo(_Fsm_Complete);
                    return false;
                }

                if (Scene.MainActor.Position.X < CHECK_END_RAYMAN_MIN_POSITION_X && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Complete(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Complete;
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__ScalDead_Mix02, this);
                if (!Rayman3.TimeAttack.IsActive)
                    Rayman3.Achievements.Unlock(AchievementId.DefeatBossBadDreams);
                Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                Timer = 0;
                Position = Position with { Y = OffsetY + BASE_OFFSET_Y };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                Timer++;

                if (Timer > COMPLETE_TIME)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}