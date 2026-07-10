using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Jano : MovableActor
{
    public Jano(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        Ammo = AMMO_PHASE_1;
        AlphaBlend = ALPHA_BLEND_MAX;
        FirstTimeMovingAway = true;
        
        SkullPlatforms = new JanoSkullPlatform[3];
        
        IsOnLeftSide = false;
        TargetPosition = Vector2.Zero;
        OffsetY = 0;

        Position = Position with { Y = OffsetY + BASE_OFFSET_Y };

        // Special intro state for time attack to skip the grimace sequence
        if (Rayman3.TimeAttack.IsActive)
            State.SetTo(_Fsm_TimeAttackIntro);
        else if (Rayman3.GameInfo.LastGreenLumAlive == 0)
            State.SetTo(_Fsm_Intro);
        else
            State.SetTo(_Fsm_Default);

        AnimatedObject.BlendMode = BlendMode.AlphaBlend;
    }

    public JanoSkullPlatform[] SkullPlatforms { get; }
    public Vector2 TargetPosition { get; set; }
    public bool HasFinishedCurrentCycle { get; set; }
    public int OffsetY { get; set; }
    public byte Ammo { get; set; }
    public float AlphaBlend { get; set; }
    public bool IsOnLeftSide { get; set; }
    public bool FirstTimeMovingAway { get; set; }
    public ushort Timer { get; set; }

    private bool IsBeingAttacked()
    {
        Box viewBox = GetViewBox();
        Rayman rayman = (Rayman)Scene.MainActor;

        for (int i = 0; i < Rayman.BODY_PARTS_FISTS_COUNT; i++)
        {
            if (rayman.ActiveBodyParts[Rayman.BODY_PARTS_FISTS_INDEX + i] != null)
            {
                Box fistDetectionBox = rayman.ActiveBodyParts[Rayman.BODY_PARTS_FISTS_INDEX + i].GetDetectionBox();

                if (viewBox.Intersects(fistDetectionBox))
                    return true;
            }
        }

        return false;
    }

    private void Shoot()
    {
        Ammo--;

        JanoShot shot = Scene.CreateProjectile<JanoShot>(ActorType.JanoShot);

        if (shot != null)
        {
            Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__JanoShot_Mix01, this);

            int dir;
            if (ActionId == Action.Attack_Right)
            {
                shot.ActionId = JanoShot.Action.Move_Right;
                dir = 1;
            }
            else
            {
                shot.ActionId = JanoShot.Action.Move_Left;
                dir = -1;
            }

            shot.Position = new Vector2(Position.X + dir * SHOT_PROJECTILE_OFFSET_X, Position.Y + SHOT_PROJECTILE_OFFSET_Y);

            // From above
            if (Position.Y == OffsetY + HIGH_OFFSET_Y)
                shot.MechModel.Speed = new Vector2(dir * SHOT_PROJECTILE_HIGH_SPEED_X, SHOT_PROJECTILE_HIGH_SPEED_Y);
            // From below
            else if (Position.Y == OffsetY + LOW_OFFSET_Y)
                shot.MechModel.Speed = new Vector2(dir * SHOT_PROJECTILE_LOW_SPEED_X, SHOT_PROJECTILE_LOW_SPEED_Y);
            // Default
            else
                shot.MechModel.Speed = new Vector2(dir * SHOT_PROJECTILE_BASE_SPEED_X, SHOT_PROJECTILE_BASE_SPEED_Y);
        }
    }

    private void ShootMultiple()
    {
        Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__JanoShot_Mix01, this);

        for (int i = 0; i < AMMO_SHOOT_MULTIPLE; i++)
        {
            JanoShot shot = Scene.CreateProjectile<JanoShot>(ActorType.JanoShot);

            if (shot != null)
            {
                shot.ActionId = JanoShot.Action.Move_Down;
                shot.Position = new Vector2(
                    x: Scene.MainActor.Position.X + i * 25 + -100, 
                    y: i * -25 + (Random.GetNumber(26) + 25) * -1 - (i > 5 ? 30 : 0));
            }
        }
    }

    private void CreateSkullPlatform(int index)
    {
        JanoSkullPlatform skullPlatform = Scene.CreateProjectile<JanoSkullPlatform>(ActorType.JanoSkullPlatform);

        if (skullPlatform != null)
        {
            SkullPlatforms[index] = skullPlatform;
            skullPlatform.SkullPlatformIndex = index;
            skullPlatform.Position = Position + new Vector2(SKULL_PLATFORM_PROJECTILE_OFFSET_X, SKULL_PLATFORM_PROJECTILE_OFFSET_Y);

            switch (CheckCurrentPhase())
            {
                case 1:
                    skullPlatform.ActionId = JanoSkullPlatform.Action.Move_Left;
                    skullPlatform.TargetY = OffsetY + 235;
                    break;

                case 2:
                    skullPlatform.ActionId = JanoSkullPlatform.Action.Move_DownLeft;
                    skullPlatform.TargetY = OffsetY + 235;
                    break;

                case 3:
                    skullPlatform.ActionId = JanoSkullPlatform.Action.SolidMove_Spawn;
                    skullPlatform.Position = skullPlatform.Position with { X = Position.X - 96 };
                    skullPlatform.TargetY = OffsetY + 250;
                    break;

                case 4:
                    skullPlatform.ActionId = JanoSkullPlatform.Action.SolidMove_Spawn;
                    skullPlatform.Position = skullPlatform.Position with { X = 1600 };
                    break;
            }

            skullPlatform.ChangeAction();
            skullPlatform.Speed = skullPlatform.Speed with { X = SKULL_PLATFORM_SPEED_X };
        }
    }

    private bool IsReadyToTurnBackAround()
    {
        int screenOffsetX;

        if (Scene.MainActor.Position.X > 1400)
            screenOffsetX = 40;
        else if (Scene.MainActor.LinkedMovementActor == null)
            screenOffsetX = 30;
        else
            screenOffsetX = -30;

        return (ScreenPosition.X >= screenOffsetX + Scene.Resolution.X && !IsOnLeftSide) ||
               (ScreenPosition.X >= screenOffsetX + -30 && IsOnLeftSide);
    }

    public int CheckCurrentPhase()
    {
        if (Scene.MainActor.Position.X < PHASE_2_POSITION_X)
        {
            OffsetY = PHASE_1_OFFSET_Y;
            return 1;
        }
        else if (Scene.MainActor.Position.X is > PHASE_2_POSITION_X and < PHASE_3_POSITION_X)
        {
            OffsetY = PHASE_2_OFFSET_Y;
            return 2;
        }
        else if (Scene.MainActor.Position.X is > PHASE_3_POSITION_X and < PHASE_4_POSITION_X)
        {
            OffsetY = PHASE_3_OFFSET_Y;
            return 3;
        }
        else
        {
            OffsetY = PHASE_4_OFFSET_Y;
            return 4;
        }
    }

    private void RefillAmmo()
    {
        int phase = CheckCurrentPhase();

        if (phase == 1)
            Ammo = AMMO_PHASE_1;
        else if (phase == 2)
            Ammo = AMMO_PHASE_2;
        else if (phase == 3)
            Ammo = AMMO_PHASE_3;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // This is one of the few actors which actually returns true in here

        if (base.ProcessMessageImpl(sender, message, param))
            return true;

        switch (message)
        {
            case Message.Actor_Hit:
                // Empty
                return true;

            case Message.Actor_Explode:
                ShootMultiple();
                return true;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawLarge(animationPlayer, forceDraw);
    }
}