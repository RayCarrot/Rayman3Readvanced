﻿using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class BossMachineBar : Bar
{
    public BossMachineBar(Scene2D scene) : base(scene) { }

    public AnimatedObject BossHealthBar { get; set; }
    public int BossDamage { get; set; }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.BossMachineBarAnimations);

        BossHealthBar = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            CurrentAnimation = BossDamage,
            ScreenPos = new Vector2(-60, 24),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        // Re-position the health bar in time attack to avoid the UI overlapping
        if (TimeAttackInfo.IsActive)
        {
            BossHealthBar.VerticalAnchor = VerticalAnchorMode.Bottom;
            BossHealthBar.ScreenPos = BossHealthBar.ScreenPos with { Y = -14 };
        }
    }

    public override void Set()
    {
        BossDamage++;
        BossHealthBar.CurrentAnimation = BossDamage;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Mode == BarMode.StayHidden) 
            return;
        
        animationPlayer.PlayFront(BossHealthBar);
    }
}