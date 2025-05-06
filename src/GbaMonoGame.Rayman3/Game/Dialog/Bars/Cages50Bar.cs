using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

// TODO: Allow this to be moved out when the options menu shows
public class Cages50Bar : Bar
{
    public Cages50Bar(Scene2D scene) : base(scene) { }

    public int DeadCages { get; set; }

    public AnimatedObject CagesIcon { get; set; }
    public AnimatedObject CollectedCagesDigit1 { get; set; }
    public AnimatedObject CollectedCagesDigit2 { get; set; }

    // Custom to allow moving out for pause dialog options menu
    public bool EnableTransitions { get; set; }
    public int OffsetX { get; set; }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.HudAnimations);

        CagesIcon = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            ScreenPos = new Vector2(-68, 41),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        CollectedCagesDigit1 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            ScreenPos = new Vector2(-56, 45),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        CollectedCagesDigit2 = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            ScreenPos = new Vector2(-44, 45),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };
    }

    public override void Set()
    {
        DeadCages = GameInfo.GetTotalDeadCages();

        if (DeadCages == 50)
        {
            CagesIcon.CurrentAnimation = 38;
        }
        else
        {
            CollectedCagesDigit1.CurrentAnimation = DeadCages / 10;
            CollectedCagesDigit2.CurrentAnimation = DeadCages % 10;
            CagesIcon.CurrentAnimation = DeadCages / 10 == 0 ? 37 : 39;
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
                    OffsetX = 65;
                    break;

                case BarDrawStep.MoveIn:
                    if (OffsetX > 0)
                    {
                        OffsetX -= 3;
                    }
                    else
                    {
                        OffsetX = 0;
                        DrawStep = BarDrawStep.Wait;
                        EnableTransitions = false;
                    }
                    break;

                case BarDrawStep.MoveOut:
                    if (OffsetX < 65)
                    {
                        OffsetX += 2;
                    }
                    else
                    {
                        OffsetX = 65;
                        DrawStep = BarDrawStep.Hide;
                    }
                    break;
            }

            if (DrawStep == BarDrawStep.Hide)
                return;

            CagesIcon.ScreenPos = CagesIcon.ScreenPos with { X = -68 + OffsetX };
            CollectedCagesDigit1.ScreenPos = CollectedCagesDigit1.ScreenPos with { X = -56 + OffsetX };
            CollectedCagesDigit2.ScreenPos = CollectedCagesDigit2.ScreenPos with { X = -44 + OffsetX };
        }

        animationPlayer.PlayFront(CagesIcon);

        if (DeadCages < 50)
        {
            if (DeadCages > 9)
                animationPlayer.PlayFront(CollectedCagesDigit1);

            animationPlayer.PlayFront(CollectedCagesDigit2);
        }
    }
}