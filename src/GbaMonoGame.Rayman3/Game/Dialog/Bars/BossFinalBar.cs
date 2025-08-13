using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class BossFinalBar : Bar
{
    public BossFinalBar(Scene2D scene, int phase) : base(scene)
    {
        Phase = phase;
    }

    public AnimatedObject BossHealthBar { get; set; }
    public int BossDamage { get; set; }
    public int Phase { get; set; }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.BossFinalBarAnimations);

        BossHealthBar = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = BossDamage + 4,
            ScreenPos = new Vector2(-80, Phase == 0 ? 28 : -9),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            VerticalAnchor = Phase == 0 ? VerticalAnchorMode.Top : VerticalAnchorMode.Bottom,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        // Re-position the health bar in time attack to avoid the UI overlapping
        if (Phase == 0 && TimeAttackInfo.IsActive)
        {
            BossHealthBar.VerticalAnchor = VerticalAnchorMode.Bottom;
            BossHealthBar.ScreenPos = BossHealthBar.ScreenPos with { Y = -9 };
        }
    }

    public override void Set()
    {
        BossDamage++;
        BossHealthBar.CurrentAnimation = BossDamage + 4;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Mode == BarMode.StayHidden) 
            return;
        
        animationPlayer.PlayFront(BossHealthBar);
    }
}