using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class BlueLumBar : Bar
{
    public BlueLumBar(Scene2D scene) : base(scene)
    {
        GameInfo.ResetBlueLumsTime();
    }

    private const int BarXPosition = 10;
    private const int FillXPosition = 11;

    public AnimatedObject Empty { get; set; } // Empty animation - unused leftover?
    public AnimatedObject Outline { get; set; }
    public AnimatedObject Fill1 { get; set; }
    public AnimatedObject Fill2 { get; set; }
    public AnimatedObject Fill3 { get; set; }
    public AnimatedObject ScaledFill { get; set; }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.BlueLumBarAnimations);

        Empty = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(BarXPosition, -10),
            VerticalAnchor = VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 2,
            RenderContext = Scene.HudRenderContext,
        };

        Outline = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 2,
            ScreenPos = new Vector2(BarXPosition, -10),
            VerticalAnchor = VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        Fill1 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 1,
            ScreenPos = new Vector2(FillXPosition + 16 * 0, -9),
            VerticalAnchor = VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 1,
            RenderContext = Scene.HudRenderContext,
        };

        Fill2 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 1,
            ScreenPos = new Vector2(FillXPosition + 16 * 1, -9),
            VerticalAnchor = VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 1,
            RenderContext = Scene.HudRenderContext,
        };

        Fill3 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 1,
            ScreenPos = new Vector2(FillXPosition + 16 * 2, -9),
            VerticalAnchor = VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 1,
            RenderContext = Scene.HudRenderContext,
        };

        ScaledFill = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 1,
            ScreenPos = new Vector2(FillXPosition + 16 * 3, -9),
            VerticalAnchor = VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 1,
            IsDoubleAffine = true,
            AffineMatrix = new AffineMatrix(0, 2, 1),
            RenderContext = Scene.HudRenderContext,
        };
    }

    public override void Set() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        bool drawScaledFill = true;

        if (Mode == BarMode.StayHidden || GameInfo.IsBlueLumsTimeOver())
            return;

        // The timer has a range of 0-416. Dividing it by 8 gives us 0-52.
        float scaledTimerValue = GameInfo.BlueLumsTimer / 8f;
        
        // Round down to nearest factor of 16
        float scaledFillXPos = scaledTimerValue;
        if (scaledFillXPos % 16 != 0)
            scaledFillXPos -= scaledFillXPos % 16;

        scaledFillXPos += FillXPosition;

        switch ((int)(scaledTimerValue % 16))
        {
            case 0:
                drawScaledFill = false;
                break;

            case 1:
                ScaledFill.AffineMatrix = new AffineMatrix(0, 16, 1);
                scaledFillXPos -= 8;
                break;

            case 2:
                ScaledFill.AffineMatrix = new AffineMatrix(0, 8, 1);
                scaledFillXPos -= 7;
                break;

            case 3:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x55555), 1);
                scaledFillXPos -= 7;
                break;

            case 4:
                ScaledFill.AffineMatrix = new AffineMatrix(0, 4, 1);
                scaledFillXPos -= 6;
                break;

            case 5:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x33333), 1);
                scaledFillXPos -= 6;
                break;

            case 6:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x2aaaa), 1);
                scaledFillXPos -= 6;
                break;

            case 7:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x24924), 1);
                scaledFillXPos -= 5;
                break;

            case 8:
                ScaledFill.AffineMatrix = new AffineMatrix(0, 2, 1);
                scaledFillXPos -= 4;
                break;

            case 9:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x1c71b), 1);
                scaledFillXPos -= 4;
                break;

            case 10:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x19999), 1);
                scaledFillXPos -= 4;
                break;

            case 11:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x1745c), 1);
                scaledFillXPos -= 3;
                break;

            case 12:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x15555), 1);
                scaledFillXPos -= 3;
                break;

            case 13:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x13b13), 1);
                scaledFillXPos -= 2;
                break;

            case 14:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x124ff), 1);
                scaledFillXPos -= 2;
                break;

            case 15:
                ScaledFill.AffineMatrix = new AffineMatrix(0, MathHelpers.FromFixedPoint(0x11111), 1);
                scaledFillXPos -= 1;
                break;
        }

        ScaledFill.ScreenPos = ScaledFill.ScreenPos with { X = scaledFillXPos };

        animationPlayer.PlayFront(Empty);
        animationPlayer.PlayFront(Outline);

        if (scaledTimerValue >= 16 * 3)
            animationPlayer.PlayFront(Fill3);

        if (scaledTimerValue >= 16 * 2)
            animationPlayer.PlayFront(Fill2);

        if (scaledTimerValue >= 16 * 1)
            animationPlayer.PlayFront(Fill1);

        if (drawScaledFill)
            animationPlayer.PlayFront(ScaledFill);

        GameInfo.BlueLumsTimer--;

        if (GameInfo.BlueLumsTimer == 78)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTimer_Mix02);
        else if (GameInfo.IsBlueLumsTimeOver())
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumTimer_Mix02);
    }
}