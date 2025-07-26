using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Boulder
{
    private bool FsmStep_CheckHitMainActor()
    {
        // Start, move camera to boulder
        if (Timer == 0)
        {
            ActionId = Action.Fall;
            MechModel.Speed = MechModel.Speed with { X = 0 };
            Scene.Camera.ProcessMessage(this, Message.Cam_MoveToTarget, Position + new Vector2(-60, 20));
            Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
            Timer++;
        }
        // Wait
        else if (Timer < 90)
        {
            Timer++;
        }
        // Continue, move camera back to main actor
        else if (Timer == 90)
        {
            Scene.Camera.ProcessMessage(this, Message.Cam_MoveToLinkedObject, false);
            Timer++;
        }

        // This is unused, cause this is when the actor is hidden which makes it not have an attack box yet
        if (Timer == 0xFF)
        {
            InteractableActor hitActor = Scene.IsHitActor(this);
            if (hitActor != null && hitActor != this)
            {
                hitActor.ReceiveDamage(hitActor.HitPoints);
                hitActor.ProcessMessage(this, Message.Actor_Hit, this);
            }
        }
        // Lower hitbox so they're easier to jump over
        else if (GameInfo.MapId == MapId.Bonus4)
        {
            Box attackBox = GetAttackBox();
            attackBox.Top += 16;
            attackBox.Bottom += 16;

            Box mainActorVulnerabilityBox = Scene.MainActor.GetVulnerabilityBox();

            if (attackBox.Intersects(mainActorVulnerabilityBox))
                Scene.MainActor.ProcessMessage(this, Message.Actor_Explode, this);
        }
        // Default
        else
        {
            if (Scene.IsHitMainActor(this))
                Scene.MainActor.ProcessMessage(this, Message.Actor_Explode, this);
        }

        return true;
    }

    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Rotation = 0;
                AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, 1, 1);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (ActionId == Action.Fall)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Rotation = 0;
                break;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:

                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (Speed.Y == 0)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BallImp1_BigFoot1_Mix02);

                    if (Scene.Camera.IsActorFramed(this))
                    {
                        Explosion impact = Scene.CreateProjectile<Explosion>(ActorType.Impact);
                        if (impact != null)
                            impact.Position = Position;
                    }

                    if (PendingShake)
                    {
                        Scene.Camera.ProcessMessage(this, Message.Cam_Shake, 96);
                        PendingShake = false;
                    }

                    Box detectionBox = GetDetectionBox();

                    PhysicalType type = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom));

                    if (type == PhysicalTypeValue.Solid)
                    {
                        type = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom) + Tile.Up);
                    }

                    if (type.Value is PhysicalTypeValue.SolidAngle30Right1 or PhysicalTypeValue.SolidAngle30Right2)
                    {
                        IsMovingRight = true;
                        MoveSpeed = MathHelpers.FromFixedPoint(0x3333);
                    }
                    else if (type.Value is PhysicalTypeValue.SolidAngle30Left1 or PhysicalTypeValue.SolidAngle30Left2)
                    {
                        IsMovingRight = false;
                        MoveSpeed = -MathHelpers.FromFixedPoint(0x3333);
                    }
                }
                else
                {
                    Box detectionBox = GetDetectionBox();
                    PhysicalType type = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom));

                    if (type.Value is PhysicalTypeValue.InstaKill or PhysicalTypeValue.MoltenLava)
                    {
                        bool activeBoulder = false;
                        foreach (BaseActor actor in new EnabledActorIterator(Scene))
                        {
                            if (actor != this && (ActorType)actor.Type == ActorType.Boulder && actor.State != Fsm_Wait)
                            {
                                activeBoulder = true;
                                break;
                            }
                        }

                        if (!activeBoulder && 
                            GameInfo.MapId != MapId.Bonus4 &&
                            !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__win3) && 
                            !LevelMusicManager.HasOverridenLevelMusic)
                        {
                            SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__canopy, 3);
                        }

                        ProcessMessage(this, Message.Destroy);
                    }
                }

                MechModel.Speed = MechModel.Speed with { X = MoveSpeed };

                if (Speed.Y == 0)
                {
                    State.MoveTo(Fsm_Roll);
                    return false;
                }
                break;

            case FsmAction.UnInit:

                break;
        }

        return true;
    }

    public bool Fsm_Roll(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (IsMovingRight)
                {
                    ActionId = Action.Roll_Right;
                    MoveSpeed = MathHelpers.FromFixedPoint(0x3333);
                }
                else
                {
                    ActionId = Action.Roll_Left;
                    MoveSpeed = -MathHelpers.FromFixedPoint(0x3333);
                }

                MechModel.Speed = new Vector2(MoveSpeed, 2);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, 1, 1);

                // TODO: The boulder moves slower when on screen, which causes issues in high resolution. Have it use distance check then instead?
                if (IsMovingRight)
                {
                    if (Speed.X > 1)
                        Rotation += Speed.X;
                    else if (Speed.X > MathHelpers.FromFixedPoint(0x800))
                        Rotation += 2;

                    if (Scene.Camera.IsActorFramed(this))
                    {
                        if (MoveSpeed < MathHelpers.FromFixedPoint(0x19000))
                            MoveSpeed += MathHelpers.FromFixedPoint(0x6ae);
                        else
                            MoveSpeed = MathHelpers.FromFixedPoint(0x19000);
                    }
                    else
                    {
                        if (MoveSpeed < 2)
                            MoveSpeed += MathHelpers.FromFixedPoint(0x3000);
                        else
                            MoveSpeed = 2;
                    }
                }
                else
                {
                    if (Speed.X < -1)
                        Rotation += Speed.X;
                    else if (Speed.X < -MathHelpers.FromFixedPoint(0x800))
                        Rotation -= 2;

                    if (Scene.Camera.IsActorFramed(this))
                    {
                        if (MoveSpeed > -MathHelpers.FromFixedPoint(0x19000))
                            MoveSpeed -= MathHelpers.FromFixedPoint(0x6ae);
                        else
                            MoveSpeed = -MathHelpers.FromFixedPoint(0x19000);
                    }
                    else
                    {
                        if (MoveSpeed > -2)
                            MoveSpeed -= MathHelpers.FromFixedPoint(0x3000);
                        else
                            MoveSpeed = -2;
                    }
                }

                // Change direction
                if (Speed.X == 0)
                {
                    if (IsMovingRight)
                    {
                        IsMovingRight = false;
                        MoveSpeed = -MathHelpers.FromFixedPoint(0x3333);
                    }
                    else
                    {
                        IsMovingRight = true;
                        MoveSpeed = MathHelpers.FromFixedPoint(0x3333);
                    }
                }

                MechModel.Speed = new Vector2(MoveSpeed, 2);

                if (Speed.Y >= 1)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}