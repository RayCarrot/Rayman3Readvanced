using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Jano : MovableActor
{
    public Jano(int instanceId, Scene2D scene, ActorResource actorResource) 
        : base(instanceId, scene, actorResource)
    {
        Ammo = 2;
        AlphaBlend = 0x10;
        FirstTimeMovingAway = true;
        
        SkullPlatforms = new JanoSkullPlatform[3];
        
        IsOnLeftSide = false;
        TargetPosition = Vector2.Zero;
        OffsetY = 0;

        Position = Position with { Y = OffsetY + 150 };

        if (GameInfo.LastGreenLumAlive == 0)
            State.SetTo(Fsm_Intro);
        else
            State.SetTo(Fsm_Default);

        AnimatedObject.RenderOptions.BlendMode = BlendMode.AlphaBlend;
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

        for (int i = 0; i < 2; i++)
        {
            if (rayman.ActiveBodyParts[0] != null)
            {
                Box fistDetectionBox = rayman.ActiveBodyParts[0].GetDetectionBox();

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
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__JanoShot_Mix01);

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

            shot.Position = new Vector2(Position.X + dir * 64, Position.Y + 32);

            if (Position.Y == OffsetY + 110)
                shot.MechModel.Speed = new Vector2(dir * 2.5f, 0.625f);
            else if (Position.Y == OffsetY + 200)
                shot.MechModel.Speed = new Vector2(dir * 2, -0.75f);
            else
                shot.MechModel.Speed = new Vector2(dir * 2, 0);
        }
    }

    private void ShootMultiple()
    {
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__JanoShot_Mix01);

        for (int i = 0; i < 10; i++)
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
            skullPlatform.Position = Position + new Vector2(-32, 60);

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
            skullPlatform.Speed = skullPlatform.Speed with { X = -1 };
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
        if (Scene.MainActor.Position.X < 800)
        {
            OffsetY = 0;
            return 1;
        }
        else if (Scene.MainActor.Position.X is > 800 and < 1320)
        {
            OffsetY = -32;
            return 2;
        }
        else if (Scene.MainActor.Position.X is > 1320 and < 1750)
        {
            OffsetY = -64;
            return 3;
        }
        else
        {
            OffsetY = -64;
            return 4;
        }
    }

    private void RefillAmmo()
    {
        int phase = CheckCurrentPhase();

        if (phase == 1)
            Ammo = 2;
        else if (phase == 2)
            Ammo = 3;
        else if (phase == 3)
            Ammo = 4;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // This is the only actor which actually returns true in here

        if (base.ProcessMessageImpl(sender, message, param))
            return true;

        switch (message)
        {
            case Message.Hit:
                // Empty
                return true;

            case Message.Exploded:
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