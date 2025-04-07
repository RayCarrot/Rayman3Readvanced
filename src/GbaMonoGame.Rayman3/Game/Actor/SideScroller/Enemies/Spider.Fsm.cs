using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: Fix softlock when going back to start
public partial class Spider
{
    private bool FsmStep_CheckTurnAround()
    {
        bool turnAround = false;

        if ((!IsSpiderFacingLeft && Position.X > Scene.MainActor.Position.X) ||
            (IsSpiderFacingLeft && Position.X < Scene.MainActor.Position.X))
        {
            PhysicalType type = Scene.GetPhysicalType(Position + Tile.Left);
            if (type != PhysicalTypeValue.Spider_Stop_Right)
            {
                turnAround = true;

                if (type.Value is not (
                    PhysicalTypeValue.Enemy_Right or
                    PhysicalTypeValue.Spider_Stop_Left or
                    PhysicalTypeValue.Spider_Move_Up or
                    PhysicalTypeValue.Spider_Move_Down))
                {
                    float offsetX = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        offsetX += Tile.Size;

                        PhysicalType leftType = Scene.GetPhysicalType(Position + Tile.Left - new Vector2(0, offsetX));
                        if (leftType.Value is
                            PhysicalTypeValue.Enemy_Right or
                            PhysicalTypeValue.Spider_Stop_Left or
                            PhysicalTypeValue.Spider_Move_Up or
                            PhysicalTypeValue.Spider_Move_Down)
                        {
                            Position -= new Vector2(0, offsetX);
                            break;
                        }

                        PhysicalType rightType = Scene.GetPhysicalType(Position + Tile.Left + new Vector2(0, offsetX));
                        if (rightType.Value is
                            PhysicalTypeValue.Enemy_Right or
                            PhysicalTypeValue.Spider_Stop_Left or
                            PhysicalTypeValue.Spider_Move_Up or
                            PhysicalTypeValue.Spider_Move_Down)
                        {
                            Position += new Vector2(0, offsetX);
                            break;
                        }
                    }
                }
            }
        }
        
        if (turnAround)
        {
            State.MoveTo(Fsm_TurnAround);
            return false;
        }

        return true;
    }

    public bool Fsm_Spawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimatedObject.ObjPriority = 20;
                
                Timer = 0;
                IsNotAttacking = true;
                
                InititialPosition = Position;
                Position -= new Vector2(100, 0);
                
                InitialActionId = ActionId;
                ActionId = Action.Attack_Right;
                
                AnimatedObject.Pause(); // TODO: Somehow in the game this makes it not render the first frame (i.e. before spawned) - how/why?
                break;

            case FsmAction.Step:
                bool spawned = false;
                
                UpdateMusic();

                // Spawning
                if (SpawnTimer != 0xFF)
                {
                    SpawnTimer++;
                    if (SpawnTimer >= 16)
                        Timer++;
                }

                if (SpawnTimer == 60)
                {
                    // Return the camera to the main actor
                    Scene.Camera.ProcessMessage(this, Message.Cam_MoveToLinkedObject, false);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoPeur1_Mix03);
                    spawned = true;
                }

                if (Timer == 8 && ActionId == Action.Attack_Right)
                {
                    Timer = 0;
                    
                    // Check for end of animation and change to idle
                    if (AnimatedObject.CurrentFrame == 5)
                    {
                        ActionId = Action.Idle_Right;
                    }
                    else
                    {
                        // Manually advance the animation
                        AnimatedObject.CurrentFrame++;

                        // Move forward
                        Position += new Vector2(20, 0);
                    }
                }

                if (spawned)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (InitialActionId == Action.Idle_Right)
                {
                    InitialActionId = Action.Action0;
                    IsSpiderFacingLeft = false;
                    ClimbSpeedX = 7;
                }
                else if (InitialActionId == Action.Idle_Left)
                {
                    InitialActionId = Action.Action1;
                    IsSpiderFacingLeft = true;
                    ClimbSpeedX = -7;
                }

                ClimbSpeedY = 0;
                break;
        }

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Set the correct idle action
                if (ActionId != (!IsSpiderFacingLeft ? Action.Idle_Right : Action.Idle_Left))
                    ActionId = !IsSpiderFacingLeft ? Action.Idle_Right : Action.Idle_Left;
                break;

            case FsmAction.Step:
                bool outOfRange = false;

                UpdateMusic();
                UpdateSound();
                
                // Check if we're out of range from the main actor
                if ((!IsSpiderFacingLeft && Position.X + 37 <= Scene.MainActor.Position.X) ||
                    (IsSpiderFacingLeft && Position.X - 37 >= Scene.MainActor.Position.X))
                {
                    if (Position.Y + 72 > Scene.MainActor.Position.Y - 24 &&
                        Position.Y - 72 < Scene.MainActor.Position.Y - 24)
                    {
                        outOfRange = true;
                    }
                }

                // Attack if detected or within range
                if (Scene.IsDetectedMainActor(this) || !outOfRange)
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }
                
                // Move to main actor if out of range
                if (outOfRange)
                {
                    State.MoveTo(Fsm_Move);
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
                IsNotAttacking = false;
                Timer = 30;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SpidrAtk_Mix02);
                break;

            case FsmAction.Step:
                bool wrongDirection = false;
                bool outOfRange = false;

                UpdateMusic();
                UpdateSound();

                if (IsNotAttacking)
                {
                    if (IsActionFinished)
                    {
                        // If not rotating
                        if (ActionId < Action.RotateTo_Attack_Up)
                        {
                            if (!FsmStep_CheckTurnAround())
                                return false;

                            if (!IsSpiderFacingLeft)
                            {
                                if (Position.X + 37 <= Scene.MainActor.Position.X)
                                    outOfRange = true;
                                else if (Position.X - 37 >= Scene.MainActor.Position.X)
                                    wrongDirection = true;
                            }
                            else
                            {
                                if (Position.X - 37 >= Scene.MainActor.Position.X)
                                    outOfRange = true;
                                else if (Position.X + 37 <= Scene.MainActor.Position.X)
                                    wrongDirection = true;
                            }

                            // Reset attack if no buttons are pressed
                            if (!JoyPad.IsButtonPressed(GbaInput.A) &&
                                !JoyPad.IsButtonPressed(GbaInput.B) &&
                                !JoyPad.IsButtonPressed(GbaInput.Select) &&
                                !JoyPad.IsButtonPressed(GbaInput.Start) &&
                                !JoyPad.IsButtonPressed(GbaInput.Right) &&
                                !JoyPad.IsButtonPressed(GbaInput.Left) &&
                                !JoyPad.IsButtonPressed(GbaInput.Up) &&
                                !JoyPad.IsButtonPressed(GbaInput.Down) &&
                                !JoyPad.IsButtonPressed(GbaInput.R) &&
                                !JoyPad.IsButtonPressed(GbaInput.L))
                            {
                                IsNotAttacking = false;
                                Timer = 0;
                            }
                        }
                        // If rotating, then stop rotating
                        else
                        {
                            ActionId = ActionId switch
                            {
                                Action.RotateTo_Attack_Up => Action.Attack_Up,
                                Action.RotateTo_Attack_Down => Action.Attack_Down,
                                _ => Action.Attack_Right
                            };
                        }
                    }
                }
                else
                {
                    if (!FsmStep_CheckTurnAround())
                        return false;

                    // Wait 30 frames
                    Timer++;
                    if (Timer >= 30)
                    {
                        IsNotAttacking = true;

                        // Get distance to the main actor
                        float diffX = Scene.MainActor.Position.X - Position.X;
                        float diffY = Scene.MainActor.Position.Y - (Position.Y + 24);

                        // Attack vertically
                        if (Math.Abs(diffX) <= Math.Abs(diffY) &&
                            Position.X + 37 > Scene.MainActor.Position.X && 
                            Position.X - 37 < Scene.MainActor.Position.X)
                        {
                            if (diffY < 0)
                                ActionId = ActionId == Action.Attack_Up ? Action.Attack_Up : Action.RotateTo_Attack_Up;
                            else
                                ActionId = ActionId == Action.Attack_Down ? Action.Attack_Down : Action.RotateTo_Attack_Down;
                        }
                        // Attack horizontally
                        else if (diffX > 0)
                        {
                            ActionId = ActionId switch
                            {
                                Action.Attack_Up => Action.RotateFrom_Attack_Up,
                                Action.Attack_Down => Action.RotateFrom_Attack_Down,
                                _ => Action.Attack_Right
                            };
                        }
                    }
                }

                ManageMainActorCollision();

                if (!Scene.IsDetectedMainActor(this) && outOfRange)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }

                if (wrongDirection)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (ActionId == Action.Attack_Up)
                    ActionId = Action.RotateFrom_Attack_Up;
                else if (ActionId == Action.Attack_Down)
                    ActionId = Action.RotateFrom_Attack_Down;
                break;
        }

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimationTimer = 0;

                PhysicalType type = Scene.GetPhysicalType(Position);
                if (type.Value is 
                    PhysicalTypeValue.Enemy_Right or
                    PhysicalTypeValue.Spider_Stop_Left or
                    PhysicalTypeValue.Spider_Move_Up or
                    PhysicalTypeValue.Spider_Move_Down)
                {
                    ClimbSpeedX = 7;
                    ActionId = Action.Action0;
                    AnimatedObject.Pause();
                }
                else
                {
                    ClimbSpeedX = 0;
                }
                break;

            case FsmAction.Step:
                bool withinRange = false;
                bool canMove = true;

                UpdateMusic();
                UpdateSound();

                if (AnimationTimer == 0)
                {
                    if ((!IsSpiderFacingLeft && Position.X + 36 > Scene.MainActor.Position.X) ||
                        (IsSpiderFacingLeft && Position.X - 36 < Scene.MainActor.Position.X))
                    {
                        if (Position.Y + 72 > Scene.MainActor.Position.Y - 24 &&
                            Position.Y - 72 < Scene.MainActor.Position.Y - 24)
                        {
                            withinRange = true;
                        }
                        else
                        {
                            if (Position.Y < Scene.MainActor.Position.Y - 24)
                                Position += new Vector2(0, 2);
                            else
                                Position -= new Vector2(0, 2);

                            canMove = false;
                        }
                    }
                }

                if (canMove)
                {
                    if (!FsmStep_CheckTurnAround()) 
                        return false;
                }

                AnimationTimer++;
                
                if (AnimationTimer > 3)
                {
                    AnimationTimer = 0;
                    
                    if (AnimatedObject.CurrentFrame < 5)
                        AnimatedObject.CurrentFrame++;
                    else
                        AnimatedObject.CurrentFrame = 0;

                    if (ClimbSpeedX == 0)
                    {
                        ClimbSpeedX = !IsSpiderFacingLeft ? 7 : -7;
                        ActionId = !IsSpiderFacingLeft ? Action.Action0 : Action.Action1;
                        AnimatedObject.Pause();
                    }
                    else if (canMove)
                    {
                        Position += new Vector2(ClimbSpeedX, ClimbSpeedY);
                    }

                    ManageCollision();
                }

                ManageMainActorCollision();

                if (Scene.IsDetectedMainActor(this) || withinRange)
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }
                
                // Impossible condition
                if (withinRange)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                
                if (ShouldJump)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AnimatedObject.Resume();
                break;
        }

        return true;
    }

    public bool Fsm_TurnAround(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimationTimer = 0;
                ClimbSpeedX = 8;
                ActionId = Action.Action0;
                AnimatedObject.Pause();
                break;

            case FsmAction.Step:
                bool isFinished = false;

                UpdateMusic();
                UpdateSound();

                AnimationTimer++;
                if (AnimationTimer > 3)
                {
                    AnimationTimer = 0;

                    if (AnimatedObject.CurrentFrame < 5)
                        AnimatedObject.CurrentFrame++;
                    else
                        AnimatedObject.CurrentFrame = 0;

                    Position -= new Vector2(ClimbSpeedX, ClimbSpeedY);
                    ManageCollision();
                }

                if ((!IsSpiderFacingLeft && Position.X + 37 < Scene.MainActor.Position.X) ||
                    (IsSpiderFacingLeft && Position.X - 37 > Scene.MainActor.Position.X))
                {
                    isFinished = true;
                }

                ManageMainActorCollision();

                if (AnimationTimer == 1 && ClimbSpeedX == 0 && ClimbSpeedY == 0)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this) && IsActionFinished)
                {
                    State.MoveTo(Fsm_Attack);
                    return false;
                }

                if (isFinished)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ClimbSpeedX = -ClimbSpeedX;
                AnimatedObject.Resume();
                break;
        }

        return true;
    }

    public bool Fsm_Jump(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = -20;
                IsNotAttacking = true;
                ActionId = Action.Attack_Right;
                AnimatedObject.Pause();
                break;

            case FsmAction.Step:
                bool isFinished = false;
                
                UpdateMusic();
                
                Timer++;
                
                UpdateSound();
                
                if (Timer == 8)
                {
                    Timer = 0;

                    if (AnimatedObject.CurrentFrame == 5)
                    {
                        isFinished = true;
                    }
                    else
                    {
                        AnimatedObject.CurrentFrame++;
                        Position += new Vector2(18, 0);
                    }
                }

                ManageMainActorCollision();

                if (isFinished)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                InitialActionId = Action.Action0;
                IsSpiderFacingLeft = false;
                ClimbSpeedX = 7;
                ShouldJump = false;
                break;
        }

        return true;
    }
}