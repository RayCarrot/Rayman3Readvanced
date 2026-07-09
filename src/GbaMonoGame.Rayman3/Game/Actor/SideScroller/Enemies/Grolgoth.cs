using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Grolgoth : MovableActor
{
    public Grolgoth(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        InitialYPosition = Position.Y;
        SavedAttackCount = 0;

        if (Rayman3.GameInfo.MapId == MapId.BossFinal_M1)
        {
            Position = Position with { X = GROUND_FALL_DOWN_RIGHT_POSITION_X };
            ActionId = Action.Ground_FallDown_Left;
            AttackCount = GROUND_INITIAL_ATTACK_COUNT;
            BossHealth = GROUND_INITIAL_HEALTH;
            Timer = 0;
            State.SetTo(_Fsm_GroundFallDown);
            Position = Position with { Y = GROUND_INITIAL_POSITION_Y };
        }
        else if (Rayman3.GameInfo.MapId == MapId.BossFinal_M2)
        {
            if (Rayman3.GameInfo.LastGreenLumAlive == 0 && !Rayman3.TimeAttack.IsActive)
            {
                State.SetTo(_Fsm_AirInit);
                Timer = 0;
            }
            else
            {
                State.SetTo(_Fsm_AirDefault);
                Timer = AIR_SHOOT_MISSILE_DEFAULT_TIME;
                ActionId = Action.Air_Idle_Left;
            }

            BossHealth = AIR_INITIAL_HEALTH;
            AttackCount = AIR_INITIAL_ATTACK_COUNT;
            Unused = 0;
        }
        else
        {
            // Not sure why this is here? The Grolgoth is randomly added to a lot of non-boss levels.
            State.SetTo(_Fsm_Invalid);
        }
    }

    public float InitialYPosition { get; set; }
    public ushort Timer { get; set; }
    public byte AttackCount { get; set; }
    public byte SavedAttackCount { get; set; }
    public byte BossHealth { get; set; }
    public byte Unused { get; set; } // Unused

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
                    projectile.Position = new Vector2(Position.X + GROUND_LASER_PROJECTILE_OFFSET_X, InitialYPosition + (Random.GetNumber(3) + 8) * 8);
                    projectile.ActionId = GrolgothProjectile.Action.Laser_Right;
                    projectile.MechModel.Speed = new Vector2(3 + MathHelpers.FromFixedPoint(Random.GetNumber(0x20001)), 0);

                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Laser3_Mix03, this);

                    projectile.ChangeAction();
                }
                // Energy ball
                else
                {
                    if (ActionId is Action.Ground_ShootEnergyShotsHigh_Right or Action.Ground_ShootEnergyShotsHigh_Left)
                        projectile.Position = new Vector2(Position.X + GROUND_ENERGY_SHOT_HIGH_PROJECTILE_OFFSET_X, InitialYPosition + GROUND_ENERGY_SHOT_HIGH_PROJECTILE_OFFSET_Y);
                    else
                        projectile.Position = new Vector2(Position.X + GROUND_ENERGY_SHOT_LOW_PROJECTILE_OFFSET_X, InitialYPosition + GROUND_ENERGY_SHOT_LOW_PROJECTILE_OFFSET_Y);

                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__Laser4_Mix01, this);
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01, this);

                    projectile.ActionId = GrolgothProjectile.Action.EnergyBall_Right;
                    projectile.MechModel.Speed = new Vector2(GROUND_ENERGY_SHOT_SPEED_X, 0);

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
                    projectile.Position = new Vector2(Position.X - GROUND_LASER_PROJECTILE_OFFSET_X, InitialYPosition + (Random.GetNumber(3) + 8) * 8);
                    projectile.ActionId = GrolgothProjectile.Action.Laser_Left;
                    projectile.MechModel.Speed = new Vector2(-(3 + MathHelpers.FromFixedPoint(Random.GetNumber(0x20001))), 0);

                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Laser3_Mix03, this);

                    projectile.ChangeAction();
                }
                // Energy ball
                else
                {
                    if (ActionId is Action.Ground_ShootEnergyShotsHigh_Right or Action.Ground_ShootEnergyShotsHigh_Left)
                        projectile.Position = new Vector2(Position.X - GROUND_ENERGY_SHOT_HIGH_PROJECTILE_OFFSET_X, InitialYPosition + GROUND_ENERGY_SHOT_HIGH_PROJECTILE_OFFSET_Y);
                    else
                        projectile.Position = new Vector2(Position.X - GROUND_ENERGY_SHOT_LOW_PROJECTILE_OFFSET_X, InitialYPosition + GROUND_ENERGY_SHOT_LOW_PROJECTILE_OFFSET_Y);

                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__Laser4_Mix01, this);
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Laser4_Mix01, this);

                    projectile.ActionId = GrolgothProjectile.Action.EnergyBall_Left;
                    projectile.MechModel.Speed = new Vector2(-GROUND_ENERGY_SHOT_SPEED_X, 0);

                    projectile.VerticalOscillationAmplitude = (byte)(2 + Random.GetNumber(5));
                    projectile.VerticalOscillationSpeed = MathHelpers.FromFixedPoint(0x6000 + Random.GetNumber(0x4001));

                    projectile.ChangeAction();
                }
            }
        }
    }

    private void ShootFromAir()
    {
        for (int i = 0; i < AttackCount; i++)
        {
            GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
            if (projectile != null)
            {
                if (!Engine.Sem.IsSongPlaying(Rayman3SoundEvent.Play__Fire02_Laser4_Mix01))
                    Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__Fire02_Laser4_Mix01, this);

                // From the right
                if ((AttackCount == AIR_SHOOT_DOUBLE_SIDE_ATTACK_COUNT && i < AIR_SHOOT_ONE_SIDE_ATTACK_COUNT) || BossHealth == 3)
                {
                    projectile.Position = projectile.Position with { X = AIR_SHOT_PROJECTILE_RIGHT_POSITION_X };
                    projectile.MechModel.Speed = new Vector2(-(1 + MathHelpers.FromFixedPoint(Random.GetNumber(0x28001))), 0);
                    projectile.ActionId = GrolgothProjectile.Action.EnergyBall_Left;
                }
                // From the left
                else if ((AttackCount == AIR_SHOOT_DOUBLE_SIDE_ATTACK_COUNT && i >= AIR_SHOOT_ONE_SIDE_ATTACK_COUNT) || BossHealth == 4)
                {
                    projectile.Position = projectile.Position with { X = AIR_SHOT_PROJECTILE_LEFT_POSITION_X };
                    projectile.MechModel.Speed = new Vector2(1 + MathHelpers.FromFixedPoint(Random.GetNumber(0x28001)), 0);
                    projectile.ActionId = GrolgothProjectile.Action.EnergyBall_Right;
                }

                projectile.ChangeAction();

                if ((i + 1) % 3 == 0)
                {
                    if (i < AIR_SHOOT_ONE_SIDE_ATTACK_COUNT)
                        projectile.Position = projectile.Position with { Y = AIR_SHOT_PROJECTILE_1_POSITION_Y };
                    else
                        projectile.Position = projectile.Position with { Y = AIR_SHOT_PROJECTILE_4_POSITION_Y };
                }
                else if (((i + 1) & 1) == 0)
                {
                    if (i < AIR_SHOOT_ONE_SIDE_ATTACK_COUNT)
                        projectile.Position = projectile.Position with { Y = AIR_SHOT_PROJECTILE_2_POSITION_Y };
                    else
                        projectile.Position = projectile.Position with { Y = AIR_SHOT_PROJECTILE_5_POSITION_Y };
                }
                else
                {
                    if (i < AIR_SHOOT_ONE_SIDE_ATTACK_COUNT)
                        projectile.Position = projectile.Position with { Y = AIR_SHOT_PROJECTILE_3_POSITION_Y };
                    else
                        projectile.Position = projectile.Position with { Y = AIR_SHOT_PROJECTILE_6_POSITION_Y };
                }
            }
        }
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

        if (Rayman3.GameInfo.MapId == MapId.BossFinal_M1 && AttackCount == GROUND_HEALTH_3_FALLING_BOMBS_ATTACK_COUNT)
            availableBombPositions[Random.GetNumber(AttackCount)] = false;

        for (int i = 0; i < AttackCount; i++)
        {
            GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
            if (projectile != null)
            {
                projectile.ActionId = GrolgothProjectile.Action.FallingBomb;
                projectile.ChangeAction();
                
                if (Rayman3.GameInfo.MapId == MapId.BossFinal_M1)
                {
                    float yPos = -(10 + Random.GetNumber(51));
                    
                    if (AttackCount == GROUND_HEALTH_4_FALLING_BOMBS_ATTACK_COUNT)
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
                    projectile.Position = new Vector2(50 + i * 35, 20 - (50 + Random.GetNumber(51)));
                    projectile.MechModel.Speed = new Vector2(0, MathHelpers.FromFixedPoint(0x5000 + Random.GetNumber(-0x3fff)));
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
                projectile.Position = new Vector2(Position.X + GROUND_SMALL_BOMB_PROJECTILE_OFFSET_X, InitialYPosition + GROUND_SMALL_BOMB_PROJECTILE_OFFSET_Y);
                projectile.ActionId = GrolgothProjectile.Action.SmallGroundBomb_Right;
            }
            else
            {
                projectile.Position = new Vector2(Position.X - GROUND_SMALL_BOMB_PROJECTILE_OFFSET_X, InitialYPosition + GROUND_SMALL_BOMB_PROJECTILE_OFFSET_Y);
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
                if (ActionId is Action.Ground_BeginDeployBomb_Right or Action.Ground_BeginDeployBomb_Left)
                {
                    if (IsFacingRight)
                    {
                        projectile.Position = new Vector2(Position.X + GROUND_BIG_BOMB_PROJECTILE_OFFSET_X, GROUND_BIG_BOMB_PROJECTILE_POSITION_Y);
                        projectile.ActionId = GrolgothProjectile.Action.BigGroundBomb_Right;
                    }
                    else
                    {
                        projectile.Position = new Vector2(Position.X - GROUND_BIG_BOMB_PROJECTILE_OFFSET_X, GROUND_BIG_BOMB_PROJECTILE_POSITION_Y);
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
                        projectile.Position = new Vector2(0, GROUND_BIG_BOMB_PROJECTILE_POSITION_Y);
                        projectile.ActionId = GrolgothProjectile.Action.BigGroundBomb_Right;
                    }
                    else
                    {
                        projectile.Position = new Vector2(Scene.Resolution.X, GROUND_BIG_BOMB_PROJECTILE_POSITION_Y);
                        projectile.ActionId = GrolgothProjectile.Action.BigGroundBomb_Left;
                    }

                    projectile.ChangeAction();
                }
            }
        }
    }

    private void ShootMissile()
    {
        GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
        if (projectile != null)
        {
            float yPos = InitialYPosition + AIR_MISSILE_PROJECTILE_OFFSET_Y;

            if (IsFacingRight)
                projectile.Position = new Vector2(Position.X + AIR_MISSILE_PROJECTILE_OFFSET_X, yPos);
            else
                projectile.Position = new Vector2(Position.X - AIR_MISSILE_PROJECTILE_OFFSET_X, yPos);

            projectile.ActionId = GrolgothProjectile.Action.MissileDefault;
            projectile.ChangeAction();

            Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__MachAtk2_Mix02, this);
        }
    }

    // Unused
    private void DeployBigExplodingBomb()
    {
        GrolgothProjectile projectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
        if (projectile != null)
        {
            int playfieldWidth = Scene.Playfield.PhysicalLayer.PixelWidth;
            int playfieldHeight = Scene.Playfield.PhysicalLayer.PixelHeight;

            Vector2 posDiff;
            do
            {
                projectile.Position = new Vector2(
                    x: 100 + Random.GetNumber(playfieldWidth - 200 + 1),
                    y: 25 + Random.GetNumber(playfieldHeight - 50 + 1));

                posDiff = Scene.MainActor.Position - projectile.Position;
            } while (Math.Abs(posDiff.X) < 30 && Math.Abs(posDiff.Y) < 30);

            projectile.ActionId = GrolgothProjectile.Action.BigExplodingBomb;
            projectile.ChangeAction();
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            // Hit by projectile
            case Message.Actor_Hurt:
                if (State == _Fsm_GroundDeployBomb) 
                    State.MoveTo(_Fsm_GroundHit);
                else if (State == _Fsm_AirDefault || State == _Fsm_AirShootMissile)
                    State.MoveTo(_Fsm_AirHit1);
                return false;

            // Projectile attack finished
            case Message.Actor_End:
                if (State == _Fsm_GroundDeployBomb)
                {
                    // NOTE: The original game does not have this check, but we need it since otherwise there can be a timing issue in
                    //       widescreen where the projectiles stay alive long enough for the Grolgoth to just enter the state to deploy
                    //       a bomb, meaning that sending the message here incorrectly cancels the bomb deployment.
                    if (((GrolgothProjectile)sender).ActionId is not (
                        GrolgothProjectile.Action.EnergyBall_Right or GrolgothProjectile.Action.EnergyBall_Left or 
                        GrolgothProjectile.Action.Laser_Right or GrolgothProjectile.Action.Laser_Left))
                        State.MoveTo(_Fsm_GroundDefault);
                }
                else if ((State == _Fsm_GroundFallDown || State == _Fsm_AirAttack) && AttackCount != 0)
                {
                    AttackCount--;
                }
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // The animation wraps to the bottom for the first 2 frames, but not after! This is noticeable in both the GBA and N-Gage versions.
        if (AnimatedObject.CurrentAnimation == 34)
        {
            if (Engine.Settings.Active.Tweaks.FixBugs && AnimatedObject.CurrentFrame < 2)
                AnimatedObject.SetAnimationWrap(34, new Box(0, 0, 0, 67));
            else
                AnimatedObject.SetAnimationWrap(34, Box.Empty);
        }

        DrawLarge(animationPlayer, forceDraw);
    }
}