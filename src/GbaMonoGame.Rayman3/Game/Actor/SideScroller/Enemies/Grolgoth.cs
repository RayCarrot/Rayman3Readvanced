using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Grolgoth : MovableActor
{
    public Grolgoth(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        InitialYPosition = Position.Y;
        SavedAttackCount = 0;

        if (GameInfo.MapId == MapId.BossFinal_M1)
        {
            Position = Rom.Platform switch
            {
                Platform.GBA => Position with { X = 200 },
                Platform.NGage => Position with { X = 151 },
                _ => throw new UnsupportedPlatformException(),
            };
            ActionId = Action.Ground_FallDown_Left;
            AttackCount = 8;
            BossHealth = 5;
            Timer = 0;
            State.SetTo(Fsm_GroundFallDown);
            Position = Position with { Y = -30 };
        }
        else if (GameInfo.MapId == MapId.BossFinal_M2)
        {
            if (GameInfo.LastGreenLumAlive == 0)
            {
                State.SetTo(FUN_1001a4ac);
                Timer = 0;
            }
            else
            {
                State.SetTo(FUN_1001a660);
                Timer = 300;
                ActionId = Action.Action22;
            }

            BossHealth = 5;
            AttackCount = 8;
            Field_69 = 0;
        }
        else
        {
            // Not sure why this is here? The Grolgoth is randomly added to a lot of non-boss levels.
            State.SetTo(Fsm_Invalid);
        }
    }

    public float InitialYPosition { get; set; }
    public ushort Timer { get; set; }
    public byte AttackCount { get; set; }
    public byte SavedAttackCount { get; set; }
    public byte BossHealth { get; set; }
    public byte Field_69 { get; set; } // TODO: Name

    private void ShootFromGround()
    {
        AttackCount--;

        GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
        if (projectile != null)
        {
            if (IsFacingRight)
            {
                // Laser
                if (ActionId is Action.Ground_ShootLasers_Right or Action.Ground_ShootLasers_Left)
                {
                    projectile.Position = new Vector2(Position.X + 16, InitialYPosition + (Random.GetNumber(3) + 8) * 8);
                    projectile.ActionId = GrolgothProjectile.Action.Laser_Right;
                    projectile.MechModel.Speed = new Vector2(3 + MathHelpers.FromFixedPoint(Random.GetNumber(0x20001)), 0);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser3_Mix03);

                    projectile.ChangeAction();
                }
                // Energy ball
                else
                {
                    if (ActionId is Action.Ground_ShootEnergyShotsHigh_Right or Action.Ground_ShootEnergyShotsHigh_Left)
                        projectile.Position = new Vector2(Position.X + 64, InitialYPosition + 48);
                    else
                        projectile.Position = new Vector2(Position.X + 16, InitialYPosition + 72);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Laser4_Mix01);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01);

                    projectile.ActionId = GrolgothProjectile.Action.EnergyBall_Right;
                    projectile.MechModel.Speed = new Vector2(2.5f, 0);

                    projectile.VerticalOscillationAmplitude = (byte)(2 + Random.GetNumber(5));
                    projectile.VerticalOscillationSpeed = MathHelpers.FromFixedPoint(0x6000 + Random.GetNumber(0x4001));

                    projectile.ChangeAction();
                }
            }
            else
            {
                // Laser
                if (ActionId is Action.Ground_ShootLasers_Right or Action.Ground_ShootLasers_Left)
                {
                    projectile.Position = new Vector2(Position.X - 16, InitialYPosition + (Random.GetNumber(3) + 8) * 8);
                    projectile.ActionId = GrolgothProjectile.Action.Laser_Left;
                    projectile.MechModel.Speed = new Vector2(-(3 + MathHelpers.FromFixedPoint(Random.GetNumber(0x20001))), 0);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser3_Mix03);

                    projectile.ChangeAction();
                }
                // Energy ball
                else
                {
                    if (ActionId is Action.Ground_ShootEnergyShotsHigh_Right or Action.Ground_ShootEnergyShotsHigh_Left)
                        projectile.Position = new Vector2(Position.X - 64, InitialYPosition + 48);
                    else
                        projectile.Position = new Vector2(Position.X - 16, InitialYPosition + 72);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Laser4_Mix01);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01);

                    projectile.ActionId = GrolgothProjectile.Action.EnergyBall_Left;
                    projectile.MechModel.Speed = new Vector2(-2.5f, 0);

                    projectile.VerticalOscillationAmplitude = (byte)(2 + Random.GetNumber(5));
                    projectile.VerticalOscillationSpeed = MathHelpers.FromFixedPoint(0x6000 + Random.GetNumber(0x4001));

                    projectile.ChangeAction();
                }
            }
        }
    }

    private void ShootFromAir()
    {
        // TODO: Implement
    }

    private void DeployFallingBombs()
    {
        float[] bombXPositions = Rom.Platform switch
        {
            Platform.GBA => [25, 70, 115, 160, 185, 230],
            Platform.NGage => [15, 30, 55, 80, 105, 155],
            _ => throw new UnsupportedPlatformException(),
        };

        bool[] availableBombPositions = new bool[6];
        Array.Fill(availableBombPositions, true);

        if (GameInfo.MapId == MapId.BossFinal_M1 && AttackCount == Rom.Platform switch 
            {
                Platform.GBA => 6,
                Platform.NGage => 5,
                _ => throw new UnsupportedPlatformException(),
            })
        {
            availableBombPositions[Random.GetNumber(AttackCount)] = false;
        }

        for (int i = 0; i < AttackCount; i++)
        {
            GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
            if (projectile != null)
            {
                projectile.ActionId = GrolgothProjectile.Action.FallingBomb;
                projectile.ChangeAction();
                
                if (GameInfo.MapId == MapId.BossFinal_M1)
                {
                    float yPos = -(10 + Random.GetNumber(51));
                    
                    if (AttackCount == Rom.Platform switch
                        {
                            Platform.GBA => 4,
                            Platform.NGage => 3,
                            _ => throw new UnsupportedPlatformException(),
                        })
                    {
                        int index;
                        do
                        {
                            index = Random.GetNumber(Rom.Platform switch
                            {
                                Platform.GBA => 6,
                                Platform.NGage => 5,
                                _ => throw new UnsupportedPlatformException(),
                            });
                        } while (!availableBombPositions[index]);

                        projectile.Position = new Vector2(bombXPositions[index], yPos);
                        availableBombPositions[index] = false;

                        projectile.MechModel.Speed = new Vector2(0, MathHelpers.FromFixedPoint(0x5000 + Random.GetNumber(-0x3fff)));
                    }
                    else
                    {
                        projectile.Position = new Vector2(bombXPositions[i], yPos);

                        if (availableBombPositions[i])
                            projectile.MechModel.Speed = new Vector2(0, MathHelpers.FromFixedPoint(0x7000 + Random.GetNumber(0x7001)));
                        else
                            projectile.MechModel.Speed = new Vector2(0, MathHelpers.FromFixedPoint(0x5000));
                    }
                }
                else
                {
                    // TODO: Implement
                    //pFVar5 = GameObject::GetPosition((GameObject*)projectile);
                    //iVar3 = Random::GetNumber(0x33);
                    //pFVar5->y = (iVar3 + 0x32) * -0x10000 + 0x140000;
                    //pFVar5 = GameObject::GetPosition((GameObject*)projectile);
                    //pFVar5->x = 0x320000;
                    //pFVar5 = GameObject::GetPosition((GameObject*)projectile);
                    //pFVar5->x = pFVar5->x + uVar7 * 0x230000;
                    //pMVar4 = Actor::GetMechModel(projectile);
                    //iVar3 = Random::GetNumber(-0x3fff);
                    //iVar3 = iVar3 + 0x5000;
                    //(pMVar4->speed).y = iVar3;
                } 
                
            }
        }
    }

    private void DeploySmallGroundBomb()
    {
        GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
        if (projectile != null)
        {
            if (BossHealth == 5)
            {
                projectile.IsVerticalOscillationMovingDown = true;
                projectile.VerticalOscillationOffset = InitialYPosition + 72;
            }
            else if (BossHealth == 4)
            {
                projectile.IsVerticalOscillationMovingDown = true;
                projectile.VerticalOscillationOffset = InitialYPosition + 32;
            }
            else if (BossHealth == 3)
            {
                projectile.IsVerticalOscillationMovingDown = true;
                projectile.VerticalOscillationOffset = InitialYPosition;
            }

            if (IsFacingRight)
            {
                projectile.Position = new Vector2(Position.X + 40, InitialYPosition + 48);
                projectile.ActionId = GrolgothProjectile.Action.SmallGroundBomb_Right;
            }
            else
            {
                projectile.Position = new Vector2(Position.X - 40, InitialYPosition + 48);
                projectile.ActionId = GrolgothProjectile.Action.SmallGroundBomb_Left;
            }

            projectile.ChangeAction();
        }
    }

    private void DeployBigGroundBombs()
    {
        for (int i = 0; i < AttackCount; i++)
        {
            GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
            if (projectile != null)
            {
                float yPos = Rom.Platform switch
                {
                    Platform.GBA => 112,
                    Platform.NGage => 160,
                    _ => throw new UnsupportedPlatformException()
                };

                if (ActionId is Action.Ground_BeginDeployBomb_Right or Action.Ground_BeginDeployBomb_Left)
                {
                    if (IsFacingRight)
                    {
                        projectile.Position = new Vector2(Position.X + 48, yPos);
                        projectile.ActionId = GrolgothProjectile.Action.BigGroundBomb_Right;
                    }
                    else
                    {
                        projectile.Position = new Vector2(Position.X - 48, yPos);
                        projectile.ActionId = GrolgothProjectile.Action.BigGroundBomb_Left;
                    }

                    projectile.ChangeAction();

                    if (BossHealth == 1)
                        projectile.MechModel.Speed = projectile.MechModel.Speed with { X = -1 };
                }
                else
                {
                    if (IsFacingRight && (i != 0 || AttackCount != 2))
                    {
                        projectile.Position = new Vector2(0, yPos);
                        projectile.ActionId = GrolgothProjectile.Action.BigGroundBomb_Right;
                    }
                    else
                    {
                        projectile.Position = new Vector2(Scene.Resolution.X, yPos);
                        projectile.ActionId = GrolgothProjectile.Action.BigGroundBomb_Left;
                    }

                    projectile.ChangeAction();
                }
            }
        }
    }

    private void CreateMissile()
    {
        // TODO: Implement
    }

    private void CreateUnusedAttack()
    {
        // TODO: Implement
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            // Hit by projectile
            case Message.Damaged:
                if (State == Fsm_GroundDeployBomb) 
                    State.MoveTo(Fsm_GroundHit);
                else if (State == FUN_1001a660 || State == FUN_1001a7a4)
                    State.MoveTo(FUN_1001aa10);
                return false;

            // Projectile attack finished
            case Message.Main_Damaged2:
                if (State == Fsm_GroundDeployBomb) 
                    State.MoveTo(Fsm_GroundDefault);
                else if ((State == Fsm_GroundFallDown || State == FUN_1001aec8) && AttackCount != 0)
                    AttackCount--;
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawLarge(animationPlayer, forceDraw);
    }
}