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
        Field_67 = 0;

        if (GameInfo.MapId == MapId.BossFinal_M1)
        {
            Position = Rom.Platform switch
            {
                Platform.GBA => Position with { X = 200 },
                Platform.NGage => Position with { X = 151 },
                _ => throw new UnsupportedPlatformException(),
            };
            ActionId = Action.Ground_Fall_Left;
            AttackCount = 8;
            BossHealth = 5;
            Timer = 0;
            State.SetTo(FUN_1001a1d4);
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
            State.SetTo(FUN_10019370);
        }
    }

    public float InitialYPosition { get; set; }
    public ushort Timer { get; set; }
    public byte AttackCount { get; set; }
    public byte Field_67 { get; set; } // TODO: Name
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
                if (ActionId is Action.Action41 or Action.Action42)
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
                if (ActionId is Action.Action41 or Action.Action42)
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
        // TODO: Implement
    }

    private void DeployBigGroundBombs()
    {
        // TODO: Implement
    }

    private void CreateUnusedAttack()
    {
        // TODO: Implement
    }

    private void CreateMissile()
    {
        // TODO: Implement
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

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Damaged:
                if (State == Fsm_GroundDeployBomb) 
                    State.MoveTo(Fsm_GroundHit);
                else if (State == FUN_1001a660 || State == FUN_1001a7a4)
                    State.MoveTo(FUN_1001aa10);
                return false;

            case Message.Main_Damaged2:
                if (State == Fsm_GroundDeployBomb) 
                    State.MoveTo(Fsm_GroundDefault);
                else if ((State == FUN_1001a1d4 || State == FUN_1001aec8) && AttackCount != 0)
                    AttackCount--;
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // TODO: Implement
        base.Draw(animationPlayer, forceDraw);
    }
}