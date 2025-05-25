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
    public int PreviousSecondsValue { get; set; } // N-Gage only

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.LapAndTimerAnimations);

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

        // NOTE: The original game does not do this and instead displays incorrect values (-1 is displayed as 00:00:99)
        if (time < 0)
            time = 0;

        // Get the minutes value
        int minutes = time / (60 * 60);

        // Get the seconds value
        int minutesRemainingTime = time % (60 * 60);
        int seconds = minutesRemainingTime / 60;
        
        // Get the centiseconds value
        int secondsRemainingTime = minutesRemainingTime % 60;
        int centiSeconds = secondsRemainingTime * 100 / 60;

        // On N-Gage it plays a beep every second for the last 10 seconds
        if (Rom.Platform == Platform.NGage)
        {
            if (minutes == 0 && seconds <= 10 && seconds != 0 && centiSeconds == 0 && PreviousSecondsValue != seconds)
            {
                PreviousSecondsValue = seconds;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__GameOver_BeepFX01_Mix02);
            }
        }

        int minutesDigit1 = minutes / 10;
        int secondsDigit1 = seconds / 10;
        int centisecondsDigit1 = centiSeconds / 10;
        
        int minutesDigit2 = minutes + minutesDigit1 * -10;
        if (minutesDigit2 > 9)
            minutesDigit2 = 9;

        int secondsDigit2 = seconds + secondsDigit1 * -10;
        if (secondsDigit2 > 9)
            secondsDigit2 = 9;

        int centisecondsDigit2 = centiSeconds + centisecondsDigit1 * -10;
        if (centisecondsDigit2 > 9)
            centisecondsDigit2 = 9;

        if (centisecondsDigit1 > 10)
            centisecondsDigit1 = 9;

        // Minutes
        Digits[0].CurrentAnimation = minutesDigit1;
        Digits[1].CurrentAnimation = minutesDigit2;

        // Seconds
        Digits[2].CurrentAnimation = secondsDigit1;
        Digits[3].CurrentAnimation = secondsDigit2;
        
        // Centiseconds
        Digits[4].CurrentAnimation = centisecondsDigit1;
        Digits[5].CurrentAnimation = centisecondsDigit2;

        animationPlayer.PlayFront(TimerFrame);
        foreach (AnimatedObject digit in Digits)
            animationPlayer.PlayFront(digit);
    }
}