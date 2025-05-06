using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class LumsBar : Bar
{
    public LumsBar(Scene2D scene) : base(scene) { }

    public int WaitTimer { get; set; }
    public int OffsetY { get; set; }
    public int CollectedLumsDigitValue1 { get; set; }
    public int CollectedLumsDigitValue2 { get; set; }

    public AnimatedObject LumsIcon { get; set; }
    public AnimatedObject CollectedLumsDigit1 { get; set; }
    public AnimatedObject CollectedLumsDigit2 { get; set; }
    public AnimatedObject TotalLumsDigit1 { get; set; }
    public AnimatedObject TotalLumsDigit2 { get; set; }

    public void AddLums(int count)
    {
        Debug.Assert(count <= 10, "Cannot add more than 10 lums at one time");

        DrawStep = BarDrawStep.MoveIn;
        WaitTimer = 0;

        CollectedLumsDigitValue2 += count;
        
        if (CollectedLumsDigitValue2 >= 10)
        {
            CollectedLumsDigitValue2 -= 10;
            CollectedLumsDigitValue1++;
        }

        LumsIcon.CurrentAnimation = CollectedLumsDigitValue1 == 0 ? 24 : 21;
    }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.HudAnimations);

        LumsIcon = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 24,
            ScreenPos = new Vector2(-77, 8),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        CollectedLumsDigit1 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(-52, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        CollectedLumsDigit2 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(-40, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        TotalLumsDigit1 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(-22, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        TotalLumsDigit2 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(-10, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };
    }

    public override void Set()
    {
        int lumsCount = GameInfo.GetLumsCountForCurrentMap();

        TotalLumsDigit1.CurrentAnimation = lumsCount / 10;
        TotalLumsDigit2.CurrentAnimation = lumsCount % 10;

        int collectedLums = GameInfo.GetDeadLumsForCurrentMap(GameInfo.MapId);

        CollectedLumsDigitValue1 = collectedLums / 10;
        CollectedLumsDigitValue2 = collectedLums % 10;

        LumsIcon.CurrentAnimation = CollectedLumsDigitValue1 == 0 ? 24 : 21;
    }

    public void SetWithoutUpdating()
    {
        int lumsCount = GameInfo.GetLumsCountForCurrentMap();

        TotalLumsDigit1.CurrentAnimation = lumsCount / 10;
        TotalLumsDigit2.CurrentAnimation = lumsCount % 10;

        LumsIcon.CurrentAnimation = CollectedLumsDigitValue1 == 0 ? 24 : 21;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Mode is BarMode.StayHidden or BarMode.Disabled)
            return;

        switch (DrawStep)
        {
            case BarDrawStep.Hide:
                OffsetY = 35;
                break;
                
            case BarDrawStep.MoveIn:
                if (OffsetY > 0)
                {
                    OffsetY -= 2;
                }
                else
                {
                    DrawStep = Mode == BarMode.StayVisible ? BarDrawStep.Wait : BarDrawStep.Bounce;
                    WaitTimer = 0;
                }
                break;

            case BarDrawStep.MoveOut:
                if (OffsetY < 35)
                {
                    OffsetY++;
                }
                else
                {
                    OffsetY = 35;
                    DrawStep = BarDrawStep.Hide;
                }
                break;

            case BarDrawStep.Bounce:
                if (WaitTimer < 35)
                {
                    OffsetY = -BounceData[WaitTimer];
                    WaitTimer++;
                }
                else
                {
                    OffsetY = 0;
                    DrawStep = BarDrawStep.Wait;
                    WaitTimer = 0;
                }
                break;

            case BarDrawStep.Wait:
                if (Mode != BarMode.StayVisible)
                {
                    if (WaitTimer >= 180)
                    {
                        OffsetY = 0;
                        DrawStep = BarDrawStep.MoveOut;
                    }
                    else
                    {
                        WaitTimer++;
                    }
                }
                break;
        }

        if (DrawStep != BarDrawStep.Hide)
        {
            LumsIcon.ScreenPos = LumsIcon.ScreenPos with { Y = 8 - OffsetY };
            CollectedLumsDigit1.ScreenPos = CollectedLumsDigit1.ScreenPos with { Y = 24 - OffsetY };
            CollectedLumsDigit2.ScreenPos = CollectedLumsDigit2.ScreenPos with { Y = 24 - OffsetY };
            TotalLumsDigit1.ScreenPos = TotalLumsDigit1.ScreenPos with { Y = 24 - OffsetY };
            TotalLumsDigit2.ScreenPos = TotalLumsDigit2.ScreenPos with { Y = 24 - OffsetY };

            CollectedLumsDigit1.CurrentAnimation = CollectedLumsDigitValue1;
            CollectedLumsDigit2.CurrentAnimation = CollectedLumsDigitValue2;

            animationPlayer.PlayFront(LumsIcon);

            if (CollectedLumsDigitValue1 != 0)
                animationPlayer.PlayFront(CollectedLumsDigit1);

            animationPlayer.PlayFront(CollectedLumsDigit2);
            animationPlayer.PlayFront(TotalLumsDigit1);
            animationPlayer.PlayFront(TotalLumsDigit2);
        }
    }
}