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
                    for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                        CheckPlayerCollision(Scene.GetGameObject<MovableActor>(id));
                }
                else
                {
                    CheckPlayerCollision(Scene.MainActor);
                }

                // Get the current physical type
                Mode7PhysicalTypeDefine physicalType = Scene.GetPhysicalType(Position).Mode7Define;

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

                // TODO: This rotation code is only correct from one direction. Fix by using camera angle?
                if (ActionId is Action.Move_Right or Action.Move_Up)
                    Rotation -= 3;
                else if (ActionId is Action.Move_Left or Action.Move_Down)
                    Rotation += 3;
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
                BounceSpeed = -8;
                break;

            case FsmAction.Step:
                BounceSpeed += 0.25f;
                ZPos -= BounceSpeed;

                // Check player collision when in the air
                if (ZPos <= 15 && BounceSpeed > 0)
                {
                    if (RSMultiplayer.IsActive)
                    {
                        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                        {
                            MissileMode7 player = Scene.GetGameObject<MissileMode7>(id);
                            
                            Box actionBox = GetActionBox();
                            Box playerDetectionBox = player.GetDetectionBox();

                            if (actionBox.Intersects(playerDetectionBox) && !player.IsInvulnerable)
                            {
                                player.ReceiveDamage(1);
                                player.Scale = new Vector2(0.5f, 2);
                                player.CustomScaleTimer = 100;
                            }
                        }
                    }
                    else
                    {
                        if (Scene.IsDetectedMainActor(this) && !Scene.MainActor.IsInvulnerable)
                        {
                            Scene.MainActor.ReceiveDamage(1);

                            MissileMode7 mainActor = (MissileMode7)Scene.MainActor;
                            mainActor.Scale = new Vector2(0.5f, 2);
                            mainActor.CustomScaleTimer = 100;
                        }
                    }
                }

                if (ZPos < 0)
                {
                    ZPos = 0;
                    State.MoveTo(Fsm_BounceHitGround);
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
                    for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                        CheckPlayerCollision(Scene.GetGameObject<MovableActor>(id));
                }
                else
                {
                    CheckPlayerCollision(Scene.MainActor);
                }

                if (IsSquashing)
                {
                    Scale += new Vector2(-0.03125f, 0.25f);

                    if (Scale.Y >= 2.5f)
                        IsSquashing = false;
                }
                else
                {
                    Scale += new Vector2(0.03125f, -0.25f);
                }

                if (Scale.Y <= 1 && !IsSquashing)
                {
                    Scale = Scale with { Y = 1 };
                    State.MoveTo(Fsm_Bounce);
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