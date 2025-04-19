using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class JanoSkullPlatform
{
    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                bool isHit = false;
                bool isOffScreen = false;
                bool isDespawned = false;
                bool isStoppedMoving = false;

                Box detectionBox = GetDetectionBox();

                Rayman rayman = (Rayman)Scene.MainActor;

                for (int i = 0; i < 2; i++)
                {
                    RaymanBody activeFist = rayman.ActiveBodyParts[i];

                    if (activeFist != null && activeFist.GetDetectionBox().Intersects(detectionBox))
                    {
                        activeFist.ProcessMessage(this, Message.RaymanBody_FinishedAttack);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkullHit_Mix02);
                        isHit = true;
                        break;
                    }
                }

                if (Scene.IsHitMainActor(this))
                {
                    Speed = Speed with { X = 0 };
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    isStoppedMoving = true;
                }

                if (ScreenPosition.X < -40)
                {
                    isOffScreen = true;
                }
                else if (Speed.X == 0 && ActionId is not (Action.SolidMove_Spawn or Action.Despawn or Action.SolidMove_SpawnLeft))
                {
                    isStoppedMoving = true;
                }
                else if (ActionId == Action.SolidMove_SpawnLeft)
                {
                    Timer++;

                    if (Timer > 120)
                        ActionId = Action.Despawn;
                }
                else if (IsActionFinished)
                {
                    if (ActionId == Action.SolidMove_Spawn)
                        ActionId = Action.SolidMove_SpawnLeft;
                    else
                        isDespawned = ActionId == Action.Despawn;
                }

                if (isDespawned)
                {
                    Jano.SkullPlatforms[SkullPlatformIndex] = null;
                    CheckAgainstObjectCollision = true;
                    CheckAgainstMapCollision = true;
                    Timer = 0;
                    ScreenPosition = Vector2.Zero;
                    ProcessMessage(this, Message.Destroy);
                    State.MoveTo(Fsm_TimeOut);
                    return false;
                }
                
                if (isHit)
                {
                    State.MoveTo(Fsm_Stationary);
                    return false;
                }
                
                if (isOffScreen)
                {
                    State.MoveTo(Fsm_DespawnDown);
                    return false;
                }
                
                if (isStoppedMoving)
                {
                    State.MoveTo(Fsm_FallDown);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Stationary(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (ActionId is Action.SolidMove_SpawnLeft or Action.SolidMove_Spawn)
                    ActionId = Action.SolidMove_Stationary;
                else
                    ActionId = Action.Shake;

                int phase = Jano.CheckCurrentPhase();
                if (phase < 3)
                    Timer = 0;
                else
                    Timer = 420;
                break;

            case FsmAction.Step:
                bool isCollided = false;
                bool isOnSolidMove = false;

                Timer++;

                if (Timer > 720)
                {
                    if (ActionId != Action.Shake && ActionId != Action.SolidMove_Stationary)
                    {
                        ActionId = Action.Shake;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkulShak_Mix01);
                    }
                }
                // Move down to target
                else if (Position.Y < TargetY)
                {
                    MechModel.Speed = MechModel.Speed with { Y = 2 };

                    if (IsTouchingMap)
                        isCollided = true;
                }
                else
                {
                    MechModel.Speed = MechModel.Speed with { Y = 0 };

                    if (ActionId != Action.SolidMove_Stationary)
                        ActionId = Action.Stationary;
                }

                Box detectionBox = GetDetectionBox();

                Rayman rayman = (Rayman)Scene.MainActor;
                if (Timer > 15)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        RaymanBody activeFist = rayman.ActiveBodyParts[i];

                        if (activeFist != null)
                        {
                            Box fistBox = activeFist.GetDetectionBox();
                            
                            // Extend by 5 in all directions
                            fistBox.MinX -= 5;
                            fistBox.MinY -= 5;
                            fistBox.MaxX += 5;
                            fistBox.MaxY += 5;

                            if (fistBox.Intersects(detectionBox))
                            {
                                activeFist.ProcessMessage(this, Message.RaymanBody_FinishedAttack);
                                break;
                            }
                        }
                    }
                }

                // Check for collision with other skull platforms
                for (int i = 0; i < Jano.SkullPlatforms.Length; i++)
                {
                    JanoSkullPlatform skullPlatform = Jano.SkullPlatforms[i];

                    if (skullPlatform != null && SkullPlatformIndex != i)
                    {
                        Box skullBox = skullPlatform.GetDetectionBox();

                        // Extend
                        skullBox.MinX -= 15;
                        skullBox.MinY -= 10;
                        skullBox.MaxX += 15;
                        skullBox.MaxY += 10;

                        if (skullBox.Intersects(detectionBox))
                        {
                            skullPlatform.ProcessMessage(this, Message.HitActorOfSameType);
                            ActionId = Action.Collided;
                            isCollided = true;
                            break;
                        }
                    }
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Main_LinkMovement, this);

                    if (ActionId == Action.SolidMove_Stationary)
                    {
                        Jano.ProcessMessage(this, Message.Exploded);
                        isOnSolidMove = true;
                    }
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                    }
                }

                if (Timer > 780)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                    State.MoveTo(Fsm_DespawnDown);
                    return false;
                }

                if (isOnSolidMove)
                {
                    State.MoveTo(Fsm_SolidMove);
                    return false;
                }

                if (isCollided)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                    State.MoveTo(Fsm_FallDown);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SolidMove(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.SolidMove_Right;
                break;

            case FsmAction.Step:
                bool isStopped = false;
                
                Timer++;

                if (Speed.X == 0)
                {
                    ActionId = Action.Collided;
                    isStopped = true;
                }

                if (Position.Y < TargetY)
                    MechModel.Speed = MechModel.Speed with { Y = 1 };
                else
                    MechModel.Speed = MechModel.Speed with { Y = 0 };

                if (Speed.X == 0)
                    MechModel.Speed = MechModel.Speed with { X = 0 };

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    mainActor.ProcessMessage(this, Message.Main_LinkMovement, this);
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                        mainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                }

                if (isStopped)
                {
                    State.MoveTo(Fsm_FallDown);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_TimeOut(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.SolidMove_Spawn;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
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

    public bool Fsm_DespawnDown(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.DespawnDown;

                CheckAgainstObjectCollision = false;
                CheckAgainstMapCollision = false;

                Timer = 0;

                Scene.MainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                break;

            case FsmAction.Step:
                bool isFinished = false;

                Timer++;

                if (IsActionFinished && ActionId == Action.Despawn)
                    isFinished = true;

                if (ActionId != Action.Despawn &&
                    (ScreenPosition.Y > Scene.Resolution.Y + 1 || ScreenPosition.X < 0 || Timer > 180 || Position.Y > 280))
                {
                    ActionId = Action.Despawn;
                }

                if (isFinished)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Jano.SkullPlatforms[SkullPlatformIndex] = null;
                CheckAgainstObjectCollision = true;
                CheckAgainstMapCollision = true;
                Timer = 0;
                ScreenPosition = Vector2.Zero;
                ActionId = Action.Despawned;
                ChangeAction();
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }

    public bool Fsm_FallDown(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__SpherImp_Mix02))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SpherImp_Mix02);
                
                SpawnHitEffect();

                if (ActionId is not (Action.FallDown or Action.Collided))
                    ActionId = Action.FallDown;

                CheckAgainstObjectCollision = false;
                CheckAgainstMapCollision = false;

                Timer = 0;

                Scene.MainActor.ProcessMessage(this, Message.Main_UnlinkMovement, this);
                break;

            case FsmAction.Step:
                bool isFinished = false;

                Timer++;

                if (IsActionFinished && ActionId == Action.Despawn)
                    isFinished = true;

                if (ActionId != Action.Despawn && 
                    (ScreenPosition.Y > Scene.Resolution.Y + 1 || ScreenPosition.X < 0 || Timer > 180 || Position.Y > 280))
                {
                    ActionId = Action.Despawn;
                }

                if (isFinished)
                {
                    State.MoveTo(Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Jano.SkullPlatforms[SkullPlatformIndex] = null;
                CheckAgainstObjectCollision = true;
                CheckAgainstMapCollision = true;
                Timer = 0;
                ScreenPosition = Vector2.Zero;
                ActionId = Action.Despawned;
                ChangeAction();
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}