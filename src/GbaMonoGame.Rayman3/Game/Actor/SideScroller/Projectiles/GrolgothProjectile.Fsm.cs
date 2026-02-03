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
                if (ActionId != Action.MissileDefault &&
                    ActionId != Action.MissileBeep &&
                    ActionId != Action.MissileLockedIn &&
                    ((ScreenPosition.X > Scene.Resolution.X + 1 && Speed.X > 0) || (ScreenPosition.X < 0 && Speed.X < 0)))
                {
                    // Send message to the boss
                    Scene.GetGameObject(1).ProcessMessage(this, Message.Actor_End);
                    IsDead = true;
                }
                // Check for hit main actor
                else if (Scene.IsHitMainActor(this))
                {
                    if (ActionId != Action.BigExplodingBomb || Scale <= MathHelpers.FromFixedPoint(0xa000))
                    {
                        Scene.MainActor.ReceiveDamage(Rom.Platform switch
                        {
                            Platform.GBA => AttackPoints,
                            Platform.NGage => 1,
                            _ => throw new UnsupportedPlatformException()
                        });
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);

                        if (ActionId is not (
                            Action.EnergyBall_Right or Action.EnergyBall_Left or
                            Action.Laser_Right or Action.Laser_Left))
                        {
                            // Send message to the boss
                            Scene.GetGameObject(1).ProcessMessage(this, Message.Actor_End);
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
                                // There's a bug where the final else condition never gets hit, making the bomb keep vertically shaking
                                if (Engine.ActiveConfig.Tweaks.FixBugs)
                                {
                                    if (Position.Y > VerticalOscillationOffset + 2)
                                    {
                                        Position -= new Vector2(0, 1.5f);
                                    }
                                    else if (Position.Y < VerticalOscillationOffset - 2)
                                    {
                                        Position += new Vector2(0, 1.5f);
                                    }
                                    else
                                    {
                                        Position = Position with { Y = VerticalOscillationOffset };
                                        IsVerticalOscillationMovingDown = false;
                                    }
                                }
                                else
                                {
                                    if (Position.Y > VerticalOscillationOffset - 2)
                                    {
                                        Position -= new Vector2(0, 1.5f);
                                    }
                                    else if (Position.Y < VerticalOscillationOffset + 2)
                                    {
                                        Position += new Vector2(0, 1.5f);
                                    }
                                    else
                                    {
                                        Position = Position with { Y = VerticalOscillationOffset };
                                        IsVerticalOscillationMovingDown = false;
                                    }
                                }
                            }

                            UpdateBombSound();
                            ManageHitBomb(false);
                            break;

                        case Action.SmallGroundBombReverse_Right:
                        case Action.SmallGroundBombReverse_Left:
                            if (Scale != 1)
                                AnimatedObject.AffineMatrix = new AffineMatrix(0, Scale, Scale);

                            if (Scene.IsHitActor(this) is { Type: (int)ActorType.Grolgoth } hitActor)
                            {
                                hitActor.ProcessMessage(this, Message.Actor_Hurt);
                                Explode();
                            } 
                            break;

                        case Action.BigGroundBomb_Right:
                        case Action.BigGroundBomb_Left:
                        case Action.BigExplodingBomb:
                            if (ActionId is Action.BigGroundBomb_Right or Action.BigGroundBomb_Left)
                                ManageHitBomb(true);

                            Timer++;

                            AnimatedObject.AffineMatrix = new AffineMatrix(0, Scale, Scale);

                            if (Scale == 1)
                            {
                                AnimatedObject.SetFlagUseRotationScaling(true);
                                AnimatedObject.IsDoubleAffine = true;
                            }

                            if (Scale > 0.625f)
                                Scale -= MathHelpers.FromFixedPoint(0x100);

                            if (Timer > 180 && ActionId == Action.BigExplodingBomb)
                                Explode();

                            UpdateBombSound();
                            break;

                        case Action.FallingBomb:
                            UpdateBombSound();

                            PhysicalType type = Scene.GetPhysicalType(Position);
                            if (type.IsSolid || type == PhysicalTypeValue.InstaKill)
                            {
                                // Send message to the boss
                                Scene.GetGameObject(1).ProcessMessage(this, Message.Actor_End);
                                Explode();
                            }
                            break;

                        case Action.MissileDefault:
                        case Action.MissileBeep:
                        case Action.MissileLockedIn:
                            UpdateMissile();

                            Grolgoth grolgoth = Scene.GetGameObject<Grolgoth>(1);
                            if (grolgoth.ActionId is
                                Grolgoth.Action.Action26 or Grolgoth.Action.Action59 or
                                Grolgoth.Action.Air_Hit1_Left or Grolgoth.Action.Air_Hit1_Right)
                            {
                                Explode();
                            }

                            if (Scene.IsHitActor(this) is { Type: (int)ActorType.Grolgoth } hitGrolgoth &&
                                MissileTimer > 240 && MissileTimer != -1)
                            {
                                hitGrolgoth.ProcessMessage(this, Message.Actor_Hurt);
                                Explode();
                            }
                            else if (Scene.MainActor.HitPoints == 0)
                            {
                                Explode();
                            }
                            break;

                        case Action.MissileSmoke1:
                        case Action.MissileSmoke2:
                        case Action.MissileSmoke3:
                            UpdateMissileSmoke();
                            break;
                    }
                }

                if (IsDead)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AnimatedObject.ScreenPos = AnimatedObject.ScreenPos with { X = 0 };
                Scale = 1;
                MissileTimer = -1;
                AnimatedObject.AffineMatrix = new AffineMatrix(0, Scale, Scale);
                AnimatedObject.SetFlagUseRotationScaling(false);
                ProcessMessage(this, Message.Destroy);
                Timer = 0;
                Rotation = Angle256.Zero;
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