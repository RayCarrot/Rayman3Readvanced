using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class RaymanBody
{
    private bool FsmStep_CheckCollision()
    {
        bool moveBackwards = false;

        // Check for actor collision
        InteractableActor hitActor = Scene.IsHitActor(this);

        if ((hitActor != null && hitActor != Rayman && hitActor != HitActor) || IsTouchingMap)
        {
            if (hitActor != null && hitActor != Rayman && hitActor != HitActor)
            {
                HitActor = hitActor;

                int damage = BodyPartType switch
                {
                    RaymanBodyPartType.Fist or RaymanBodyPartType.Foot => 2,
                    RaymanBodyPartType.SecondFist => 3,
                    _ => 5
                };

                hitActor.ReceiveDamage(damage);
                hitActor.ProcessMessage(this, Message.Actor_Hit, this);
                SpawnHitEffect();
            }

            if (ActionId != (Action)(BaseActionId + 3) && ActionId != (Action)(BaseActionId + 4))
            {
                if (IsTouchingMap)
                    SpawnHitEffect();

                if (BodyPartType == RaymanBodyPartType.Torso)
                {
                    if (hitActor is { Type: (int)ActorType.BreakableGround } && ((BreakableGround)hitActor).QuickFinishBodyAttack)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Rayman_QuickFinishBodyShotAttack);
                    }
                    else
                    {
                        ActionId = IsFacingRight ? Action.Torso_MoveBackwards_Right : Action.Torso_MoveBackwards_Left;
                        moveBackwards = true;
                    }
                }
                else
                {
                    ActionId = (Action)(BaseActionId + (IsFacingRight ? 4 : 3));
                    moveBackwards = true;
                }
            }
        }

        if (moveBackwards)
        {
            State.MoveTo(Fsm_MoveBackwards);
            return false;
        }

        return true;
    }

    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                if (BodyPartType == RaymanBodyPartType.HitEffect)
                {
                    State.MoveTo(Fsm_HitEffect);
                    return false;
                }
                
                if (ActionId != Action.Idle)
                {
                    State.MoveTo(Fsm_MoveForward);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveForward(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (BodyPartType is RaymanBodyPartType.SuperFist or RaymanBodyPartType.SecondSuperFist)
                    ChargePower = 17;
                else
                    ChargePower = Math.Clamp(ChargePower / 3, 4, 16);

                if (RSMultiplayer.IsActive)
                {
                    FrameMultiSideScroller frame = (FrameMultiSideScroller)Frame.Current;
                    GameObject invisibleActor = frame.InvisibleActorId == -1 ? null : Scene.GetGameObject(frame.InvisibleActorId);
                    AnimatedObject.BlendMode = invisibleActor != null && Rayman == invisibleActor ? BlendMode.AlphaBlend : BlendMode.None;
                    ChargePower += 2;
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckCollision())
                    return false;

                bool reverseDirection = false;

                ChargePower--;

                // Go from accelerating action to decelerating action
                if (ChargePower == 0)
                    ActionId = (Action)(BaseActionId + (IsFacingRight ? 5 : 6));

                // Turn around torso
                if (BodyPartType == RaymanBodyPartType.Torso)
                {
                    if (Speed.Y < 2)
                    {
                        ActionId = IsFacingRight ? Action.Torso_MoveBackwards_Right : Action.Torso_MoveBackwards_Left;
                        reverseDirection = true;
                    }
                }
                // Turn around fist or foot
                else
                {
                    if (IsFacingRight && Speed.X < 2)
                    {
                        ActionId = (Action)(BaseActionId + 4);
                        reverseDirection = true;
                    }
                    else if (IsFacingLeft && Speed.X > -2)
                    {
                        ActionId = (Action)(BaseActionId + 3);
                        reverseDirection = true;
                    }
                }

                if (reverseDirection)
                {
                    State.MoveTo(Fsm_MoveBackwards);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveBackwards(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                CheckAgainstMapCollision = false;
                break;
            
            case FsmAction.Step:
                if (!FsmStep_CheckCollision())
                    return false;

                float bodyPos1;
                float raymanPos1;
                float bodyPos2;
                float raymanPos2;
                float targetOffset1;

                if (BodyPartType == RaymanBodyPartType.Torso)
                {
                    bodyPos1 = Position.X;
                    raymanPos1 = Rayman.Position.X;
                    bodyPos2 = Position.Y;
                    raymanPos2 = Rayman.Position.Y;
                    targetOffset1 = 0;
                }
                else
                {
                    bodyPos1 = Position.Y;
                    raymanPos1 = Rayman.Position.Y;
                    bodyPos2 = Position.X;
                    raymanPos2 = Rayman.Position.X;

                    targetOffset1 = BodyPartType == RaymanBodyPartType.Foot ? 0 : 20;
                }
                
                float targetPos1 = raymanPos1 - targetOffset1;
                float speed;

                // NOTE: The game doesn't do a tolerance check, but we have to since we use floats
                if (Math.Abs(bodyPos1 - targetPos1) < 1)
                {
                    speed = 0;
                }
                else if (bodyPos1 < targetPos1)
                {
                    float dist = Math.Abs(raymanPos2 - bodyPos2);

                    if (dist < targetPos1 - bodyPos1)
                        speed = 7;
                    else if (3 < Rayman.Position.Y - 20 - bodyPos1)
                        speed = 4;
                    else
                        speed = 1;
                }
                else
                {
                    float dist = Math.Abs(raymanPos2 - bodyPos2);

                    if (dist < bodyPos1 - raymanPos1 - targetOffset1)
                        speed = -7;
                    else if (3 < bodyPos1 - raymanPos1 + targetOffset1)
                        speed = -4;
                    else
                        speed = -1;
                }

                if (BodyPartType == RaymanBodyPartType.Torso)
                    MechModel.Speed = MechModel.Speed with { X = speed };
                else
                    MechModel.Speed = MechModel.Speed with { Y = speed };

                float remainingDist;
                if (IsFacingRight && BodyPartType != RaymanBodyPartType.Torso)
                    remainingDist = raymanPos2 - bodyPos2;
                else
                    remainingDist = bodyPos2 - raymanPos2;

                if (remainingDist < 25)
                {
                    State.MoveTo(Fsm_Wait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (BodyPartType == RaymanBodyPartType.SuperFist)
                    BodyPartType = RaymanBodyPartType.Fist;
                else if (BodyPartType == RaymanBodyPartType.SecondSuperFist)
                    BodyPartType = RaymanBodyPartType.SecondFist;
                
                Rayman.ProcessMessage(this, Message.RaymanBody_FinishAttack, BodyPartType);

                HitActor = null;
                CheckAgainstMapCollision = true;

                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }

    public bool Fsm_HitEffect(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Wait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                HitActor = null;
                CheckAgainstMapCollision = true;
                ProcessMessage(this, Message.Destroy);
                AnimatedObject.ObjPriority = 32;
                break;
        }

        return true;
    }
}