using System;
using BinarySerializer.Ubisoft.GbaEngine;
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

        // The top of the flame wraps to the bottom. Fix this by setting a max y. The y positions should only be negative for this object.
        AnimatedObject.WrapMaxY = 100;
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
        // TODO: Implement
    }

    // TODO: Implement
    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        return base.ProcessMessageImpl(sender, message, param);
    }

    // TODO: Implement
    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        base.Draw(animationPlayer, forceDraw);
    }
}