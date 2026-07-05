using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class BoulderMode7
{
    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                // Handle collision
                if (RSMultiplayer.IsActive)
                {
                    for (int id = 0; id < Engine.Multiplayer.PlayersCount; id++)
                        CheckPlayerCollision(Scene.GetGameObject<MovableActor>(id));
                }
                else
                {
                    CheckPlayerCollision(Scene.MainActor);
                }

                // Get the current physical type
                MissileMode7PhysicalTypeDefine physicalType = MissileMode7PhysicalTypeDefine.FromPhysicalType(Scene.GetPhysicalType(Position));

                // Change direction
                if (physicalType.Damage)
                {
                    if (ActionId == Action.Move_Up)
                        ActionId = Action.Move_Down;
                    else if (ActionId == Action.Move_Down)
                        ActionId = Action.Move_Up;
                    else if (ActionId == Action.Move_Left)
                        ActionId = Action.Move_Right;
                    else if (ActionId == Action.Move_Right)
                        ActionId = Action.Move_Left;
                }

                // The original code doesn't account for the camera view, so the rotation will sometimes be reversed. Optionally fix this.
                if (Engine.Settings.Active.Tweaks.FixBugs)
                {
                    if (ActionId == Action.Move_Right)
                    {
                        if (CamAngle > Angle256.Half)
                            Rotation += ROTATION_SPEED;
                        else
                            Rotation -= ROTATION_SPEED;
                    }
                    else if (ActionId == Action.Move_Left)
                    {
                        if (CamAngle > Angle256.Half)
                            Rotation -= ROTATION_SPEED;
                        else
                            Rotation += ROTATION_SPEED;
                    }
                    else if (ActionId == Action.Move_Up)
                    {
                        if (CamAngle > Angle256.Quarter && CamAngle < Angle256.Quarter * 3)
                            Rotation += ROTATION_SPEED;
                        else
                            Rotation -= ROTATION_SPEED;
                    }
                    else if (ActionId == Action.Move_Down)
                    {
                        if (CamAngle > Angle256.Quarter && CamAngle < Angle256.Quarter * 3)
                            Rotation -= ROTATION_SPEED;
                        else
                            Rotation += ROTATION_SPEED;
                    }
                }
                else
                {
                    if (ActionId is Action.Move_Right or Action.Move_Up)
                        Rotation -= ROTATION_SPEED;
                    else if (ActionId is Action.Move_Left or Action.Move_Down)
                        Rotation += ROTATION_SPEED;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Bounce(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                BounceSpeed = -BOUNCE_SPEED;
                break;

            case FsmAction.Step:
                BounceSpeed += BOUNCE_ACCELERATION;
                ZPos -= BounceSpeed;

                // Check player collision when in the air
                if (ZPos < 16 && BounceSpeed > 0)
                {
                    if (RSMultiplayer.IsActive)
                    {
                        for (int id = 0; id < Engine.Multiplayer.PlayersCount; id++)
                        {
                            MissileMode7 player = Scene.GetGameObject<MissileMode7>(id);
                            
                            Box actionBox = GetActionBox();
                            Box playerDetectionBox = player.GetDetectionBox();

                            if (actionBox.Intersects(playerDetectionBox) && !player.IsInvulnerable)
                            {
                                player.ReceiveDamage(1);
                                player.Scale = PLAYER_SQUASH_SCALE;
                                player.CustomScaleTimer = PLAYER_SQUASH_TIME;
                            }
                        }
                    }
                    else
                    {
                        if (Scene.IsDetectedMainActor(this) && !Scene.MainActor.IsInvulnerable)
                        {
                            Scene.MainActor.ReceiveDamage(1);

                            MissileMode7 mainActor = (MissileMode7)Scene.MainActor;
                            mainActor.Scale = PLAYER_SQUASH_SCALE;
                            mainActor.CustomScaleTimer = PLAYER_SQUASH_TIME;
                        }
                    }
                }

                if (ZPos < 0)
                {
                    ZPos = 0;
                    State.MoveTo(_Fsm_BounceHitGround);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_BounceHitGround(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Scale = Scale with { Y = 1 };
                IsSquashing = true;
                break;

            case FsmAction.Step:
                // Handle collision
                if (RSMultiplayer.IsActive)
                {
                    for (int id = 0; id < Engine.Multiplayer.PlayersCount; id++)
                        CheckPlayerCollision(Scene.GetGameObject<MovableActor>(id));
                }
                else
                {
                    CheckPlayerCollision(Scene.MainActor);
                }

                if (IsSquashing)
                {
                    Scale += SQUASH_DELTA;

                    if (Scale.Y >= SQUASH_TARGET_Y)
                        IsSquashing = false;
                }
                else
                {
                    Scale -= SQUASH_DELTA;
                }

                if (Scale.Y <= 1 && !IsSquashing)
                {
                    Scale = Scale with { Y = 1 };
                    State.MoveTo(_Fsm_Bounce);
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