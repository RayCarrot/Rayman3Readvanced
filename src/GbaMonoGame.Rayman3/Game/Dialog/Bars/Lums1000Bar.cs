using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

// TODO: Allow this to be moved out when the options menu shows
public class Lums1000Bar : Bar
{
    public Lums1000Bar(Scene2D scene) : base(scene) { }

    public int DeadLums { get; set; }

    public AnimatedObject LumsIcon { get; set; }
    public AnimatedObject CollectedLumsDigit1 { get; set; }
    public AnimatedObject CollectedLumsDigit2 { get; set; }
    public AnimatedObject CollectedLumsDigit3 { get; set; }

    // Custom to allow moving out for pause dialog options menu
    public bool EnableTransitions { get; set; }
    public int OffsetY { get; set; }

    public void AddLastLums()
    {
        // Original assertion messages here are kinda funny. They say "beuh!" and "beuh! encore".
        Debug.Assert(GameInfo.MapId == MapId._1000Lums, "Map is wrong");
        Debug.Assert(DeadLums == 999, "Dead lums is not 999");

        DeadLums = 1000;
        LumsIcon.CurrentAnimation = 35;
        LumsIcon.ScreenPos = LumsIcon.ScreenPos with { X = -70 };
    }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.HudAnimations);

        LumsIcon = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            ScreenPos = new Vector2(-88, 8),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        CollectedLumsDigit1 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            ScreenPos = new Vector2(-72, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        CollectedLumsDigit2 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            ScreenPos = new Vector2(-61, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        CollectedLumsDigit3 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            ScreenPos = new Vector2(-50, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };
    }

    public override void Set()
    {
        DeadLums = GameInfo.GetTotalDeadLums();

        if (DeadLums == 1000)
        {
            LumsIcon.CurrentAnimation = 35;
            LumsIcon.ScreenPos = LumsIcon.ScreenPos with { X = -100 };
        }
        else
        {
            LumsIcon.ScreenPos = LumsIcon.ScreenPos with { X = -88 };
            CollectedLumsDigit1.ScreenPos = CollectedLumsDigit1.ScreenPos with { X = -72 };
            CollectedLumsDigit2.ScreenPos = CollectedLumsDigit2.ScreenPos with { X = -61 };
            CollectedLumsDigit3.ScreenPos = CollectedLumsDigit3.ScreenPos with { X = -50 };

            int digit1 = DeadLums / 100;
            int digit2 = DeadLums % 100 / 10;
            int digit3 = DeadLums % 10;

            CollectedLumsDigit1.CurrentAnimation = digit1;
            CollectedLumsDigit2.CurrentAnimation = digit2;
            CollectedLumsDigit3.CurrentAnimation = digit3;

            if (DeadLums == 999)
            {
                LumsIcon.ScreenPos = LumsIcon.ScreenPos with { X = -100 };
                CollectedLumsDigit1.ScreenPos = CollectedLumsDigit1.ScreenPos with { X = -86 };
                CollectedLumsDigit2.ScreenPos = CollectedLumsDigit2.ScreenPos with { X = -75 };
                CollectedLumsDigit3.ScreenPos = CollectedLumsDigit3.ScreenPos with { X = -64 };

                LumsIcon.CurrentAnimation = 36;
            }
            else
            {
                if (digit1 != 0)
                    LumsIcon.CurrentAnimation = 34;
                else if (digit2 != 0)
                    LumsIcon.CurrentAnimation = 32;
                else
                    LumsIcon.CurrentAnimation = 33;

                LumsIcon.ScreenPos = LumsIcon.ScreenPos with { X = -88 };
            }
        }
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        // Custom transition code for the pause dialog options menu
        if (EnableTransitions)
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
                        OffsetY = 0;
                        DrawStep = BarDrawStep.Wait;
                        EnableTransitions = false;
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
            }

            if (DrawStep == BarDrawStep.Hide)
                return;

            LumsIcon.ScreenPos = LumsIcon.ScreenPos with { Y = 8 - OffsetY };
            CollectedLumsDigit1.ScreenPos = CollectedLumsDigit1.ScreenPos with { Y = 24 - OffsetY };
            CollectedLumsDigit2.ScreenPos = CollectedLumsDigit2.ScreenPos with { Y = 24 - OffsetY };
            CollectedLumsDigit3.ScreenPos = CollectedLumsDigit3.ScreenPos with { Y = 24 - OffsetY };
        }

        animationPlayer.PlayFront(LumsIcon);

        if (DeadLums < 1000)
        {
            if (DeadLums > 99)
                animationPlayer.PlayFront(CollectedLumsDigit1);

            if (DeadLums > 9)
                animationPlayer.PlayFront(CollectedLumsDigit2);

            animationPlayer.PlayFront(CollectedLumsDigit3);
        }
    }
}