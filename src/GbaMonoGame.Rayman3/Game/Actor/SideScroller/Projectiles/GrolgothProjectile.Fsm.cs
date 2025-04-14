using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class GrolgothProjectile
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                UpdateVerticalOscillation();

                // Check for out of range
                if (ActionId != Action.Action12 &&
                    ActionId != Action.Action16 &&
                    ActionId != Action.Action17 &&
                    ((ScreenPosition.X > Scene.Resolution.X + 1 && Speed.X > 0) || (ScreenPosition.X < 0 && Speed.X < 0)))
                {
                    // Send message to the boss
                    Scene.GetGameObject(1).ProcessMessage(this, Message.Main_Damaged2);
                    IsDead = true;
                }
                // Check for hit main actor
                else if (Scene.IsHitMainActor(this))
                {
                    if (ActionId != Action.Action11 || Scale <= MathHelpers.FromFixedPoint(0xa000))
                    {
                        Scene.MainActor.ReceiveDamage(Rom.Platform switch
                        {
                            Platform.GBA => AttackPoints,
                            Platform.NGage => 1,
                            _ => throw new UnsupportedPlatformException()
                        });
                        Scene.MainActor.ProcessMessage(this, Message.Damaged);

                        if (ActionId is not (
                            Action.EnergyBall_Right or Action.EnergyBall_Left or
                            Action.Laser_Right or Action.Laser_Left))
                        {
                            // Send message to the boss
                            Scene.GetGameObject(1).ProcessMessage(this, Message.Main_Damaged2);
                        }

                        Explode();
                    }
                }
                else
                {
                    switch (ActionId)
                    {
                        case Action.SmallGroundBomb_Right:
                        case Action.SmallGroundBomb_Left:
                            if (IsVerticalOscillationMovingDown)
                            {
                                if (Position.Y > VerticalOscillationOffset - 2)
                                {
                                    Position -= new Vector2(0, 1.5f);
                                }
                                else if (Position.Y < VerticalOscillationOffset + 2)
                                {
                                    Position += new Vector2(0, 1.5f);
                                }
                                // TODO: This never gets hit. Bug? Fix?
                                else
                                {
                                    Position = Position with { Y = VerticalOscillationOffset };
                                    IsVerticalOscillationMovingDown = false;
                                }
                            }

                            UpdateBombSound();
                            ManageHitBomb(false);
                            break;

                        case Action.SmallGroundBombReverse_Right:
                        case Action.SmallGroundBombReverse_Left:
                            if (Scale != 1 && AnimatedObject.AffineMatrix != null)
                                AnimatedObject.AffineMatrix = new AffineMatrix(0, Scale, Scale);

                            if (Scene.IsHitActor(this) is { Type: (int)ActorType.Grolgoth } grolgoth)
                            {
                                grolgoth.ProcessMessage(this, Message.Damaged);
                                Explode();
                            } 
                            break;

                        case Action.BigGroundBomb_Right:
                        case Action.BigGroundBomb_Left:
                        case Action.Action11:
                            if (ActionId is Action.BigGroundBomb_Right or Action.BigGroundBomb_Left)
                                ManageHitBomb(true);

                            Timer++;

                            if (AnimatedObject.AffineMatrix != null)
                                AnimatedObject.AffineMatrix = new AffineMatrix(0, Scale, Scale);

                            if (Scale == 1)
                            {
                                AnimatedObject.AffineMatrix = new AffineMatrix(0, Scale, Scale);
                                AnimatedObject.IsDoubleAffine = true;
                            }

                            if (Scale > 0.625f)
                                Scale -= MathHelpers.FromFixedPoint(0x100);

                            if (Timer > 180 && ActionId == Action.Action11)
                                Explode();

                            UpdateBombSound();
                            break;

                        case Action.FallingBomb:
                            UpdateBombSound();

                            PhysicalType type = Scene.GetPhysicalType(Position);
                            if (type.IsSolid || type == PhysicalTypeValue.InstaKill)
                            {
                                // Send message to the boss
                                Scene.GetGameObject(1).ProcessMessage(this, Message.Main_Damaged2);
                                Explode();
                            }
                            break;

                        case Action.Action12:
                        case Action.Action16:
                        case Action.Action17:
                            // TODO: Implement
                            break;

                        case Action.Action13:
                        case Action.Action14:
                        case Action.Action15:
                            // TODO: Implement
                            break;
                    }
                }

                if (IsDead)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AnimatedObject.ScreenPos = AnimatedObject.ScreenPos with { X = 0 };
                Scale = 1;
                AnimatedObject.AffineMatrix = null;
                ProcessMessage(this, Message.Destroy);
                Timer = 0;
                Field_0x6c = 0;
                VerticalOscillationAmplitude = 0;
                IsVerticalOscillationMovingDown = true;
                VerticalOscillationOffset = 0;
                IsDead = false;
                AnimatedObject.ObjPriority = 10;
                break;
        }

        return true;
    }
}