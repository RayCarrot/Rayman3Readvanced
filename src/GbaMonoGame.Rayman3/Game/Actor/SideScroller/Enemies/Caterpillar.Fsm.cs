using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Caterpillar
{
    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SineWaveValue = 0;

                MechModel.Speed = Direction switch
                {
                    MoveDirection.Up => new Vector2(0, -1),
                    MoveDirection.Down => new Vector2(0, 1),
                    MoveDirection.Left => new Vector2(-1, 0),
                    MoveDirection.Right => new Vector2(1, 0),
                    _ => throw new Exception("Invalid direction")
                };
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

                SineWaveValue += 4;
                CheckMoveBounds();

                // Sine wave movement
                Position = Direction switch
                {
                    MoveDirection.Up or MoveDirection.Down => Position with { X = LastPosition.X + MathHelpers.Sin256(SineWaveValue) * 16 },
                    MoveDirection.Left or MoveDirection.Right => Position with { Y = LastPosition.Y + MathHelpers.Sin256(SineWaveValue) * 16 },
                    _ => throw new Exception("Invalid direction")
                };

                if (HitPoints == 0)
                {
                    State.MoveTo(_Fsm_Projectile);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this))
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

    public bool Fsm_Attack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                switch (Direction)
                {
                    case MoveDirection.Up:
                        LastPosition = LastPosition with { Y = Position.Y };
                        TargetPosition = Scene.MainActor.Position.Y;
                        MechModel.Speed = MechModel.Speed with { Y = -3 };
                        break;

                    case MoveDirection.Down:
                        LastPosition = LastPosition with { Y = Position.Y };
                        TargetPosition = Scene.MainActor.Position.Y;
                        MechModel.Speed = MechModel.Speed with { Y = 3 };
                        break;

                    case MoveDirection.Left:
                        LastPosition = LastPosition with { X = Position.X };
                        TargetPosition = Scene.MainActor.Position.X;
                        MechModel.Speed = MechModel.Speed with { X = -3 };
                        break;

                    case MoveDirection.Right:
                        LastPosition = LastPosition with { X = Position.X };
                        TargetPosition = Scene.MainActor.Position.X;
                        MechModel.Speed = MechModel.Speed with { X = 3 };
                        break;

                    default:
                        throw new Exception("Invalid direction");
                }
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

                bool reachedTarget = Direction switch
                {
                    MoveDirection.Up => Position.Y <= TargetPosition,
                    MoveDirection.Down => Position.Y >= TargetPosition,
                    MoveDirection.Left => Position.X <= TargetPosition,
                    MoveDirection.Right => Position.X >= TargetPosition,
                    _ => throw new Exception("Invalid direction")
                };

                if (HitPoints == 0)
                {
                    State.MoveTo(_Fsm_Projectile);
                    return false;
                }

                if (reachedTarget)
                {
                    State.MoveTo(_Fsm_AttackReturn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_AttackReturn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Reverse the direction
                MechModel.Speed = Direction switch
                {
                    MoveDirection.Up => MechModel.Speed with { Y = 3 },
                    MoveDirection.Down => MechModel.Speed with { Y = -3 },
                    MoveDirection.Left => MechModel.Speed with { X = 3 },
                    MoveDirection.Right => MechModel.Speed with { X = -3 },
                    _ => throw new Exception("Invalid direction")
                };
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

                bool reachedTarget = Direction switch
                {
                    MoveDirection.Up => Position.Y >= LastPosition.Y,
                    MoveDirection.Down => Position.Y <= LastPosition.Y,
                    MoveDirection.Left => Position.X >= LastPosition.X,
                    MoveDirection.Right => Position.X <= LastPosition.X,
                    _ => throw new Exception("Invalid direction")
                };

                if (HitPoints == 0)
                {
                    State.MoveTo(_Fsm_Projectile);
                    return false;
                }

                if (reachedTarget)
                {
                    State.MoveTo(_Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Projectile(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimatedObject.DisableLinks = true;
                AnimatedObject.CurrentAnimation = AnimatedObject.BaseAnimation;
                
                ActionId = IsFacingRight ? Action.Falling_Right : Action.Falling_Left;
                
                CheckAgainstMapCollision = true;
                IsTouchingMap = false;
                IsInvulnerable = true;
                
                HitPoints++;
                
                Timer = 0;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MumuHit_SkullHit_Mix02);
                break;

            case FsmAction.Step:
                Timer++;

                // Land on the ground
                if (IsTouchingMap && ActionId is not (Action.KnockedDown_Left or Action.KnockedDown_Right))
                    ActionId = IsFacingRight ? Action.KnockedDown_Left : Action.KnockedDown_Right;

                // Picked up by Rayman
                if (Scene.IsDetectedMainActor(this) &&
                    ((Rayman)Scene.MainActor).AttachedObject == null &&
                    ActionId is Action.KnockedDown_Left or Action.KnockedDown_Right)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_PickUpObject, this);
                }

                if (Timer >= 240)
                {
                    State.MoveTo(_Fsm_ProjectileReturn);
                    return false;
                }

                if (ActionId is Action.KnockedDown_Left or Action.KnockedDown_Right &&
                    Scene.IsDetectedMainActor(this) &&
                    ((Rayman)Scene.MainActor).AttachedObject == this)
                {
                    State.MoveTo(_Fsm_PickedUp);
                    return false;
                }

                if (Rom.Platform == Platform.NGage &&
                    Scene.GetPhysicalType(new Vector2(Position.X, GetDetectionBox().Bottom)) == PhysicalTypeValue.MoltenLava)
                {
                    State.MoveTo(_Fsm_Dying);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                CheckAgainstMapCollision = false;
                SineWaveValue = 0;
                break;
        }

        return true;
    }

    public bool Fsm_ProjectileReturn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Direction switch
                {
                    MoveDirection.Up => Action.Move_Up,
                    MoveDirection.Down => Action.Move_Down,
                    MoveDirection.Left => Action.Move_Left,
                    MoveDirection.Right => Action.Move_Right,
                    _ => throw new Exception("Invalid direction")
                };

                Vector2 dist = LastPosition - Position;
                float distLength = dist.Length();
                if (distLength != 0)
                {
                    ReturnSpeed = dist / distLength * 2;
                    MechModel.Speed = ReturnSpeed;
                }

                AnimatedObject.DisableLinks = false;
                AnimatedObject.SetPosition(Position);

                IsInvulnerable = false;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MumuWake_LumHit_Mix03);
                break;

            case FsmAction.Step:
                bool returned = false;
                if (ReturnSpeed.X > 0)
                {
                    if (Position.X + ReturnSpeed.X > LastPosition.X)
                    {
                        ReturnSpeed = Vector2.Zero;
                        Position = LastPosition;
                        returned = true;
                    }
                }
                else if (ReturnSpeed.X < 0)
                {
                    if (Position.X + ReturnSpeed.X < LastPosition.X)
                    {
                        ReturnSpeed = Vector2.Zero;
                        Position = LastPosition;
                        returned = true;
                    }
                }

                if (HitPoints == 0)
                {
                    State.MoveTo(_Fsm_Projectile);
                    return false;
                }

                if (returned)
                {
                    State.MoveTo(_Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_PickedUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.PickedUp;
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrownUp(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.ThrownUp;
                break;

            case FsmAction.Step:
                bool hitOtherObj = false;

                if (Scene.IsHitActor(this) is { } hitObj)
                {
                    hitObj.ReceiveDamage(AttackPoints);
                    hitOtherObj = true;
                }

                // Caught by Rayman if falling back down
                if (Scene.IsDetectedMainActor(this) &&
                    ((Rayman)Scene.MainActor).AttachedObject == null &&
                    Speed.Y > 0)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_CatchObject, this);
                }

                if (hitOtherObj || Scene.GetPhysicalType(new Vector2(Position.X, GetDetectionBox().Bottom)).IsSolid)
                {
                    State.MoveTo(_Fsm_Dying);
                    return false;
                }

                if (Scene.IsDetectedMainActor(this) &&
                    ((Rayman)Scene.MainActor).AttachedObject == this &&
                    Speed.Y > 0)
                {
                    State.MoveTo(_Fsm_PickedUp);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrownForward(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Scene.MainActor.IsFacingRight ? Action.ThrownForward_Right : Action.ThrownForward_Left;
                break;

            case FsmAction.Step:
                bool hitOtherObj = false;

                if (Scene.IsHitActor(this) is { } hitObj)
                {
                    hitObj.ReceiveDamage(AttackPoints);
                    hitOtherObj = true;
                }

                PhysicalType type = Scene.GetPhysicalType(new Vector2(Position.X, GetDetectionBox().Bottom));

                // NOTE: Checking for the InstaKill type appears to be a mistake in the original code (it checking for <= 32 instead of < 32)
                if (hitOtherObj || type.IsSolid || type.Value is PhysicalTypeValue.InstaKill or PhysicalTypeValue.MoltenLava)
                {
                    State.MoveTo(_Fsm_Dying);
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
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MumuDead_Mix04);
                ActionId = Action.Dying;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                    ProcessMessage(this, Message.Destroy);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}