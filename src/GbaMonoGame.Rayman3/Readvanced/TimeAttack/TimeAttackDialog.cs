using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class TimeAttackDialog : Dialog
{
    public TimeAttackDialog(Scene2D scene) : base(scene)
    {
        State.SetTo(Fsm_Countdown);
    }

    private const int CountdownStartTime = 50;
    private const int CountdownSpeed = 25;
    private const int TargetTimeMargin = 8;
    private const int TargetBlinkRange = 60 * 3; // 3 seconds

    public TimerBar TimerBar { get; set; }
    public AnimatedObject Countdown { get; set; }
    public SpriteTextureObject TargetTimeIcon { get; set; }
    public SpriteTimeAttackTimeObject TargetTimeText { get; set; }

    public uint CountdownTimer { get; set; }
    public int CountdownValue { get; set; }
    public int TargetTimeIndex { get; set; }
    public TimeAttackTime TargetTime { get; set; }
    public bool HideTargetTime { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public void SetCountdownValue(int value)
    {
        CountdownValue = value;
        Countdown.CurrentAnimation = value;
    }

    public void SetTargetTime(int targetTimeIndex)
    {
        TargetTimeIndex = targetTimeIndex;
        TargetTime = TargetTimeIndex == -1 ? default : TimeAttackInfo.TargetTimes[TargetTimeIndex];

        if (TargetTimeIndex != -1)
        {
            TargetTimeText.Time = TargetTime;
            TargetTimeText.ScreenPos = new Vector2(-(TargetTimeText.GetWidth() + TargetTimeMargin), TargetTimeMargin);

            TargetTimeIcon.Texture = TargetTime.LoadIcon(true);
            TargetTimeIcon.ScreenPos = TargetTimeText.ScreenPos + new Vector2(-18, -2);
        }
    }

    public void MoveInBars()
    {
        TimerBar.SetToStayVisible();
        HideTargetTime = false;
    }

    public void MoveOutBars()
    {
        TimerBar.SetToStayHidden();
        HideTargetTime = true;
    }

    public override void Load()
    {
        TimerBar = new TimerBar(Scene);
        TimerBar.Load();
        TimerBar.Set();

        AnimatedObjectResource countdownResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.CountdownAnimations);

        Countdown = new AnimatedObject(countdownResource, true)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, 90),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        if (TimeAttackInfo.Mode == TimeAttackMode.Countdown && CountdownValue != -1)
            Countdown.CurrentAnimation = CountdownValue;

        TargetTimeIcon = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            HorizontalAnchor = HorizontalAnchorMode.Right,
            RenderContext = Scene.HudRenderContext,
        };

        TargetTimeText = new SpriteTimeAttackTimeObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            HorizontalAnchor = HorizontalAnchorMode.Right,
            RenderContext = Scene.HudRenderContext,
        };

        // Set the initial target time
        int targetTime = -1;
        for (int i = TimeAttackInfo.TargetTimes.Length - 1; i >= 0; i--)
        {
            if (TimeAttackInfo.Timer <= TimeAttackInfo.TargetTimes[i].Time)
            {
                targetTime = i;
                break;
            }
        }
        SetTargetTime(targetTime);
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        TimerBar.DrawTime(animationPlayer, TimeAttackInfo.Timer);

        if (TimeAttackInfo.Mode == TimeAttackMode.Countdown && CountdownValue != -1 && !TimeAttackInfo.IsPaused)
            animationPlayer.PlayFront(Countdown);

        if (TargetTimeIndex != -1 && !HideTargetTime)
        {
            int timeDiff = TargetTime.Time - TimeAttackInfo.Timer;
            bool blink = !TimeAttackInfo.IsPaused && timeDiff <= TargetBlinkRange;

            if (blink && timeDiff % 60 == 30)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__GameOver_BeepFX01_Mix02);

            if (!blink || timeDiff % 60 < 30)
            {
                animationPlayer.PlayFront(TargetTimeIcon);
                animationPlayer.PlayFront(TargetTimeText);
            }
        }
    }
}