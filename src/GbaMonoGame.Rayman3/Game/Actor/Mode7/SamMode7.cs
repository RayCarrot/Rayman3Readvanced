using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class SamMode7 : Mode7Actor
{
    public SamMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        ShouldDraw = false;
        AnimatedObject.BgPriority = 0;
        RenderHeight = 16;
        Direction = Angle256.Quarter * 3; // 3/4
        DrawTetherSparkles = true;
        WaterSplashObj = null;
        TetherSparkles = null;

        State.SetTo(Fsm_Init);
    }

    private readonly float[] DrawOffsetsTable =
    [
        0x20, 0x40, 0x60, 0x80, 0xA0, 0xC0, 0xE0, 0x100,
        0xE0, 0xC0, 0xA0, 0x80, 0x60, 0x40, 0x20, 0x0,
        -0x20, -0x40, -0x60, -0x80, -0xA0, -0xC0, -0xE0, -0x100,
        -0xE0, -0xC0, -0xA0, -0x80, -0x60, -0x40, -0x20, 0x0,
    ];

    public bool ShouldDraw { get; set; }
    public bool DrawTetherSparkles { get; set; }
    public AnimatedObject[] TetherSparkles { get; set; }

    public byte Timer { get; set; }
    public Angle256 TargetDirection { get; set; }
    public WaterSplashMode7 WaterSplashObj { get; set; }

    private void SetMode7DirectionalAction()
    {
        SetMode7DirectionalAction(0, 5);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Main_Damaged2:
                DrawTetherSparkles = false;
                return true;

            case Message.ReloadAnimation:
                // Don't need to do anything. The original game reloads the tether animations here.
                return false;

            default:
                return false;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        TetherSparkles ??= new AnimatedObject[8];

        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.WaterskiSparkleAnimations);

        for (int i = 0; i < TetherSparkles.Length; i++)
        {
            TetherSparkles[i] = new AnimatedObject(resource, false)
            {
                CurrentAnimation = 0,
                CurrentFrame = i switch
                {
                    0 => 1,
                    1 => 4,
                    2 => 2,
                    3 => 3,
                    4 => 1,
                    5 => 4,
                    6 => 2,
                    7 => 3,
                    _ => throw new ArgumentOutOfRangeException()
                },
                BgPriority = 0,
                ObjPriority = 32,
                RenderContext = Scene.RenderContext,
            };
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (!ShouldDraw)
            return;

        // Change position for drawing to give the effect of the actor wiggling side to side
        Vector2 dir = Direction.ToDirectionalVector();
        float drawOffset = DrawOffsetsTable[Timer % 32];
        Vector2 drawPos = Position + drawOffset * new Vector2(dir.Y, dir.X) / 256;
        Vector2 actualPos = Position;
        Position = drawPos;

        if (Scene.Camera.IsActorFramed(this) || forceDraw)
        {
            AnimatedObject.IsFramed = true;

            if (DrawTetherSparkles)
            {
                foreach (AnimatedObject tetherSparkle in TetherSparkles)
                    tetherSparkle.IsFramed = true;

                Mode7Actor mainActor = (Mode7Actor)Scene.MainActor;

                for (int i = 0; i < TetherSparkles.Length; i++)
                {
                    Vector2 pos = ((i + 1) * ((Position - mainActor.Position) / 8) + mainActor.Position);
                    float zPos = 24 + mainActor.ZPos / 8 * (8 - i);
                    ((CameraActorMode7)Scene.Camera).IsAnimatedObjectFramed(TetherSparkles[i], pos, zPos);
                }

                foreach (AnimatedObject tetherSparkle in TetherSparkles)
                    animationPlayer.Play(tetherSparkle);
            }

            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.ComputeNextFrame();

            foreach (AnimatedObject tetherSparkle in TetherSparkles)
            {
                tetherSparkle.IsFramed = false;
                tetherSparkle.ComputeNextFrame();
            }
        }

        // Restore actual position
        Position = actualPos;
    }
}