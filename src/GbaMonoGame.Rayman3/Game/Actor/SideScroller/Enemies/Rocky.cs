using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// TODO: Camera shake doesn't work when in high res
public sealed partial class Rocky : MovableActor
{
    public Rocky(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Init);
        
        BossHealth = 3;
        Position = Position with { X = 210 };
        Timer = 150;
        InitialYPosition = Position.Y;
        BlueLum = null;
        AttackCount = 5;

        AnimatedObject.BgPriority = 0;
        AnimatedObject.ObjPriority = 17;

        // The top of the fists wraps to the bottom. This is noticeable in both the GBA and N-Gage versions.
        if (Engine.ActiveConfig.Tweaks.FixBugs)
        {
            AnimatedObject.SetAnimationWrap(1, new Box(0, 0, 0, 115));
            AnimatedObject.SetAnimationWrap(5, new Box(0, 0, 0, 126));
            AnimatedObject.SetAnimationWrap(7, new Box(0, 0, 0, 122));
        }
    }

    private const int FlamePositionsCount = 6;

    public float[] FlameXPositions { get; } = [40, 72, 108, 136, 168, 210];

    public int BossHealth { get; set; }
    public int AttackCount { get; set; }
    public float InitialYPosition { get; set; }
    public Lums BlueLum { get; set; }
    public ushort Timer { get; set; }

    private void SpawnFlames()
    {
        bool[] availableFlamePositions = new bool[FlamePositionsCount];
        
        // Default all flame positions to available
        Array.Fill(availableFlamePositions, true);

        int nextIndex;
        if (IsFacingLeft)
        {
            availableFlamePositions[FlamePositionsCount - 1] = false;
            nextIndex = FlamePositionsCount - 1;
        }
        else
        {
            availableFlamePositions[0] = false;
            nextIndex = 0;
        }

        int flamesCount = BossHealth switch
        {
            3 => 1,
            2 => 2,
            _ => 3
        };

        RockyFlame[] flames = new RockyFlame[flamesCount];
        for (int i = 0; i < flames.Length; i++)
        {
            flames[i] = Scene.CreateProjectile<RockyFlame>(ActorType.RockyFlame);
            flames[i].AnimatedObject.IsSoundEnabled = i == 0;
        }

        float yPos = Rom.Platform switch
        {
            Platform.GBA => 160,
            Platform.NGage => 136,
            _ => throw new UnsupportedPlatformException()
        };

        // Set the position of the first flame based on Rayman's X position
        for (int i = 0; i < FlamePositionsCount; i++)
        {
            if (Scene.MainActor.Position.X < FlameXPositions[i] || i == FlamePositionsCount - 1)
            {
                flames[0].Position = new Vector2(FlameXPositions[i] - 8, yPos);
                availableFlamePositions[i] = false;
                break;
            }
        }

        // Set the position of the remaining flames
        for (int i = 1; i < flames.Length; i++)
        {
            // Get next available index
            bool isAvailable = availableFlamePositions[nextIndex];
            while (!isAvailable)
            {
                nextIndex = Random.GetNumber(6);
                isAvailable = availableFlamePositions[nextIndex];
            }

            flames[i].Position = new Vector2(FlameXPositions[nextIndex] - 8, yPos);
            availableFlamePositions[nextIndex] = false;
        }

        AttackCount--;
    }

    private void SpawnBlueLum()
    {
        if (BlueLum is not { IsEnabled: true })
        {
            if (BlueLum == null)
            {
                BlueLum = Scene.CreateProjectile<Lums>(ActorType.Lums);
                BlueLum.AnimatedObject.BasePaletteIndex = 1;
                BlueLum.AnimatedObject.CurrentAnimation = 0;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Appear_SocleFX1_Mix01);
            }
            else
            {
                BlueLum.ProcessMessage(this, Message.Resurrect);

                // It's probably an oversight to not have the sound play here
                if (Engine.ActiveConfig.Tweaks.FixBugs)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Appear_SocleFX1_Mix01);
            }

            BlueLum.ActionId = Lums.Action.BlueLum;
            BlueLum.Position = new Vector2(IsFacingLeft ? 80 : 160, 80);
        }

        AttackCount = BossHealth == 3 ? 5 : 8;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Actor_Hit:
                Vector2 hitPos = ((GameObject)param).Position;
                if (State == Fsm_Default && hitPos.Y < Position.Y - 30)
                    State.MoveTo(Fsm_Hit);
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