using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Jano
{
    private bool FsmStep_CheckHit()
    {
        // NOTE: In the original game this changes the alpha one step every second frame and
        //       uses direct equality checks rather than less than or greater than
        if (IsBeingAttacked())
        {
            if (AlphaBlend > 4)
            {
                AlphaBlend -= 0.5f;

                if (AlphaBlend <= 4 && !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__JanoRire_Mix01))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__JanoRire_Mix01);
            }
        }
        else
        {
            if (AlphaBlend < 0x10)
                AlphaBlend += 0.5f;
        }

        AnimatedObject.GbaAlpha = AlphaBlend;

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
                if (Scene.MainActor.Position.X > 375 && Timer == 0)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);

                    if (Scene.MainActor.IsFacingLeft)
                        Scene.MainActor.AnimatedObject.FlipX = false;

                    Timer = 1;
                }
                // Move on screen
                else if (Timer == 1)
                {
                    if (Position.X <= 480)
                        Timer = 2;
                    else
                        Position -= new Vector2(2, 0);
                }
                // Wait
                else if (Timer is >= 2 and < 30)
                {
                    Timer++;
                }
                // Grimace
                else if (Timer == 30)
                {
                    ((Rayman)Scene.MainActor).ActionId = Rayman.Action.Idle_Grimace_Right;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Grimace1_Mix04);
                    Timer = 31;
                    ActionId = Action.Grimace_Left;
                }
                // Wait
                else if (Timer > 30)
                {
                    Timer++;
                }

                if (IsActionFinished && ActionId is Action.Grimace_Right or Action.Grimace_Left && Timer > 60)
                {
                    State.MoveTo(Fsm_MoveAway);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Create a checkpoint to avoid showing the intro cutscene each time
                GameInfo.GreenLumTouchedByRayman(0, new Vector2(378, 208));
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

                // Move back
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X > 170)
                {
                    Position -= new Vector2(2, 0);
                    return true;
                }
                else if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X > 170)
                {
                    Position += new Vector2(2, 0);
                    return true;
                }

                // Move away if Rayman gets too close
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < 50)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < 120 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < 50)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < 120 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                // Attack if there is remaining ammo
                if (Timer > 60 && Ammo != 0)
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                // If no ammo then create skull platform
                if (Timer > 60)
                {
                    RefillAmmo();
                    State.MoveTo(Fsm_CreateSkullPlatform);
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
                if (distX < 50 && Position.X < 1850)
                {
                    if (IsOnLeftSide)
                        Position -= new Vector2(3, 0);
                    else
                        Position += new Vector2(3, 0);
                }

                if (IsActionFinished && ActionId is Action.Grimace_Right or Action.Grimace_Left)
                {
                    State.MoveTo(Fsm_MoveAway);
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
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Janogrrr_Mix03);

                if (ActionId is not (Action.Move_Right or Action.Move_Left))
                    ActionId = IsOnLeftSide ? Action.TurnAroundSlow_Right : Action.TurnAroundSlow_Left;
                
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                Timer++;

                float distX = Math.Abs(Position.X - Scene.MainActor.Position.X);
                if (distX < 50 && Position.X < 1850)
                {
                    if (IsOnLeftSide)
                        Position -= new Vector2(3, 0);
                    else
                        Position += new Vector2(3, 0);
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
                                Position = Position with { Y = OffsetY + 110 };
                            else
                                Position = Position with { Y = OffsetY + 150 };
                        }
                        else if (phase is 2 or 3)
                        {
                            if (Timer < 250)
                                Position = Position with { Y = OffsetY + 150 };
                            else if (Timer < 500)
                                Position = Position with { Y = OffsetY + 110 };
                            else
                                Position = Position with { Y = OffsetY + 200 };
                        }
                    }

                    ActionId = IsOnLeftSide ? Action.TurnAroundFast_Right : Action.TurnAroundFast_Left;
                    ChangeAction();
                }

                if (Position.X > 2050)
                {
                    State.MoveTo(Fsm_CheckComplete);
                    return false;
                }

                if (IsActionFinished && 
                    ActionId is Action.TurnAroundFast_Right or Action.TurnAroundFast_Left && 
                    Scene.MainActor.Position.X > 1400 &&
                    Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_CheckComplete);
                    return false;
                }

                if (IsActionFinished && 
                    ActionId is Action.TurnAroundFast_Right or Action.TurnAroundFast_Left)
                {
                    State.MoveTo(Fsm_MoveBack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Grimace1_Mix04);
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
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X > 170)
                    Position -= new Vector2(2, 0);
                else if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X > 170)
                    Position += new Vector2(2, 0);

                if (Timer > 45 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_Default);
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
                        if (Position.Y > OffsetY + 152)
                        {
                            Position -= new Vector2(0, 2);
                        }
                        else if (Position.Y < OffsetY + 148)
                        {
                            Position += new Vector2(0, 2);
                        }
                        else
                        {
                            Position = Position with { Y = OffsetY + 150 };
                            reachedTarget = true;
                        }
                        break;

                    case 2:
                        if (Position.Y > OffsetY + 110)
                        {
                            Position -= new Vector2(0, 2);
                        }
                        else if (Position.Y >= 20 && Position.Y < OffsetY + 110)
                        {
                            Position += new Vector2(0, 2);
                        }
                        else
                        {
                            Position = Position with { Y = OffsetY + 110 };
                            reachedTarget = true;
                        }
                        break;

                    case 3:
                        if (Position.Y < OffsetY + 150)
                        {
                            Position += new Vector2(0, 2);
                        }
                        else
                        {
                            Position = Position with { Y = OffsetY + 150 };
                            reachedTarget = true;
                        }
                        break;
                }

                int skullPlatformIndex = Array.IndexOf(SkullPlatforms, null);

                if (Position.X - Scene.MainActor.Position.X < 150)
                    Position += new Vector2(4, 0);

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
                    else if (Timer > 20 && 
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
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (CheckCurrentPhase() == 3 && IsActionFinished && ActionId == Action.CreateSkullPlatform4)
                {
                    State.MoveTo(Fsm_StandAside);
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
                    if (TargetPosition.Y + 4 < Position.Y)
                    {
                        Position -= new Vector2(0, 4);
                    }
                    else if (Position.Y < TargetPosition.Y - 4)
                    {
                        Position += new Vector2(0, 4);
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
                        int offset = rand switch
                        {
                            < 376 => 110,
                            _ => 150
                        };
                        TargetPosition = TargetPosition with { Y = OffsetY + offset };
                    }
                    else if (phase is 2 or 3)
                    {
                        int offset = rand switch
                        {
                            < 250 => 150,
                            < 500 => 110,
                            _ => 200
                        };
                        TargetPosition = TargetPosition with { Y = OffsetY + offset };
                    }

                    Timer = 1;
                }

                if (IsActionFinished && Ammo != 0 && ActionId is Action.Attack_Right or Action.Attack_Left)
                    ActionId = ActionId == Action.Attack_Right ? Action.Idle_Right : Action.Idle_Left;

                // Move away if Rayman gets too close
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < 50)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < 120 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < 50)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < 120 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                // Finished shooting, not phase 3
                if (IsActionFinished && Ammo == 0 && CheckCurrentPhase() != 3)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                // Finished shooting, phase 3
                if (IsActionFinished && Ammo == 0 && CheckCurrentPhase() == 3)
                {
                    State.MoveTo(Fsm_SwitchSide);
                    return false;
                }

                // Why does the game check these again??
                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < 50)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (!IsOnLeftSide && Position.X - Scene.MainActor.Position.X < 120 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < 50)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                if (IsOnLeftSide && Scene.MainActor.Position.X - Position.X < 120 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_BeginMoveAway);
                    return false;
                }

                // Attack again if reached target and waited half a second
                if (Single.IsNaN(TargetPosition.Y) && Timer > 30)
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

                if (Position.Y > 20)
                    Position -= new Vector2(0, 4);
                else
                    Position = Position with { Y = 20 };

                if (!IsOnLeftSide)
                {
                    if (Scene.MainActor.Position.X - Position.X > Scene.Resolution.X)
                    {
                        IsOnLeftSide = true;
                        isFinished = true;

                        Timer = (ushort)Random.GetNumber(751);
                        Position = Timer switch
                        {
                            < 250 => Position with { Y = OffsetY + 150 },
                            < 500 => Position with { Y = OffsetY + 110 },
                            _ => Position with { Y = OffsetY + 200 }
                        };
                    }
                }
                else
                {
                    if (Position.X - Scene.MainActor.Position.X > 170)
                    {
                        IsOnLeftSide = false;
                        isFinished = true;
                    }
                }

                if (isFinished && IsOnLeftSide)
                {
                    RefillAmmo();
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (isFinished && !IsOnLeftSide)
                {
                    RefillAmmo();
                    State.MoveTo(Fsm_CreateSkullPlatform);
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

                bool hasActiveSkullPlatforms = SkullPlatforms.Any(x => x != null);

                if (Scene.MainActor.LinkedMovementActor != null)
                {
                    State.MoveTo(Fsm_MoveAway);
                    return false;
                }

                if (!hasActiveSkullPlatforms)
                {
                    State.MoveTo(Fsm_Default);
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

                if (Position.X < 1900)
                    Position += new Vector2(2, 0);

                Rayman rayman = (Rayman)Scene.MainActor;
                if (CheckCurrentPhase() == 4 && 
                    Position.X > 1750 && 
                    Scene.MainActor.Speed.Y == 0 &&
                    rayman.State != rayman.Fsm_HangOnEdge)
                {
                    State.MoveTo(Fsm_Complete);
                    return false;
                }

                if (Scene.MainActor.Position.X < 1400 && Scene.MainActor.Speed.Y == 0)
                {
                    State.MoveTo(Fsm_Default);
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
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__ScalDead_Mix02);
                Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                Timer = 0;
                Position = Position with { Y = OffsetY + 150 };
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHit())
                    return false;

                Timer++;

                if (Timer > 90)
                {
                    State.MoveTo(Fsm_Default);
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