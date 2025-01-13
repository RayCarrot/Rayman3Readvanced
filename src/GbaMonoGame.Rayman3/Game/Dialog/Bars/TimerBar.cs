using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class TimerBar : Bar
{
    public TimerBar(Scene2D scene) : base(scene) { }

    public AnimatedObject TimerFrame { get; set; }
    public AnimatedObject[] Digits { get; set; }
    public int NGage_Value { get; set; }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.TimerAnimations);

        TimerFrame = new AnimatedObject(resource, false)
        {
            CurrentAnimation = 14,
            ScreenPos = new Vector2(82, 20),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };

        Digits = new AnimatedObject[6];
        for (int i = 0; i < Digits.Length; i++)
        {
            Digits[i] = new AnimatedObject(resource, false)
            {
                CurrentAnimation = 0,
                ScreenPos = new Vector2(i switch
                {
                    0 => 82,
                    1 => 92,
                    2 => 104,
                    3 => 114,
                    4 => 126,
                    5 => 136,
                    _ => throw new ArgumentOutOfRangeException()
                } + 8, 18),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                BgPriority = 0,
                ObjPriority = 0,
                RenderContext = Scene.HudRenderContext,
            };
        }
    }

    public override void Set() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        throw new InvalidOperationException($"Use {nameof(DrawTime)} when drawing the timer bar");
    }

    public void DrawTime(AnimationPlayer animationPlayer, int time)
    {
        if (Mode is BarMode.StayHidden or BarMode.Disabled)
            return;

        // Code copied from the N-Gage decompilation. Could probably be cleaned up a bit.
        int iVar1 = time / 3600;
        int uVar2 = time % 3600;
        int digit3 = uVar2 / 60;
        int iVar3 = uVar2 % 60;
        iVar3 = iVar3 * 100 / 60;

        // TODO: What is this? Why is it not on GBA?
        if (Rom.Platform == Platform.NGage)
        {
            if (iVar1 == 0 && digit3 < 11 && digit3 != 0 && iVar3 == 0 && NGage_Value != digit3)
            {
                NGage_Value = digit3;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__GameOver_BeepFX01_Mix02);
            }
        }

        int digit0 = iVar1 / 10;
        int digit2 = digit3 / 10;
        int digit4 = iVar3 / 10;
        
        int digit1 = iVar1 + digit0 * -10;
        if (9 < digit1)
            digit1 = 9;

        digit3 += digit2 * -10;
        if (9 < digit3)
            digit3 = 9;

        int digit5 = iVar3 + digit4 * -10;
        if (9 < digit5)
            digit5 = 9;

        if (10 < digit4)
            digit4 = 9;

        Digits[0].CurrentAnimation = digit0;
        Digits[1].CurrentAnimation = digit1;
        Digits[2].CurrentAnimation = digit2;
        Digits[3].CurrentAnimation = digit3;
        Digits[4].CurrentAnimation = digit4;
        Digits[5].CurrentAnimation = digit5;

        animationPlayer.PlayFront(TimerFrame);
        foreach (AnimatedObject digit in Digits)
            animationPlayer.PlayFront(digit);
    }
}