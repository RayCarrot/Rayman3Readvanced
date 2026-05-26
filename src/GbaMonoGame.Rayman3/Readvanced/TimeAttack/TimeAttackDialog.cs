using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Load and Init might be called multiple times
[GenerateFsmFields]
public partial class TimeAttackDialog : Dialog
{
    public TimeAttackDialog(Scene2D scene) : base(scene)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Countdown);
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

    public void SetCountdownValue(int value)
    {
        CountdownValue = value;
        Countdown.CurrentAnimation = value;
    }

    public void UpdateTargetTime()
    {
        // Determine the current target time
        int targetTime = -1;
        for (int i = Rayman3.TimeAttack.TargetTimes.Length - 1; i >= 0; i--)
        {
            if (Rayman3.TimeAttack.Timer <= Rayman3.TimeAttack.TargetTimes[i].Time)
            {
                targetTime = i;
                break;
            }
        }

        if (targetTime == TargetTimeIndex)
            return;

        TargetTimeIndex = targetTime;
        TargetTime = TargetTimeIndex == -1 ? default : Rayman3.TimeAttack.TargetTimes[TargetTimeIndex];

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

        AnimatedObjectResource countdownResource = Rom.Loader.ReadResource<AnimatedObjectResource>(Rayman3DefinedResource.CountdownAnimations);

        Countdown = new AnimatedObject(countdownResource, true)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, 90),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        if (Rayman3.TimeAttack.Mode == TimeAttackMode.Countdown && CountdownValue != -1)
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

        TargetTimeIndex = -1;
        UpdateTargetTime();
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        TimerBar.DrawTime(animationPlayer, Rayman3.TimeAttack.Timer);

        if (Rayman3.TimeAttack.Mode == TimeAttackMode.Countdown && CountdownValue != -1 && !Rayman3.TimeAttack.IsPaused)
            animationPlayer.PlayFront(Countdown);

        if (TargetTimeIndex != -1 && !HideTargetTime)
        {
            int timeDiff = TargetTime.Time - Rayman3.TimeAttack.Timer;
            bool blink = !Rayman3.TimeAttack.IsPaused && timeDiff <= TargetBlinkRange;

            if (blink && timeDiff % 60 == 30)
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__GameOver_BeepFX01_Mix02);

            if (!blink || timeDiff % 60 < 30)
            {
                animationPlayer.PlayFront(TargetTimeIcon);
                animationPlayer.PlayFront(TargetTimeText);
            }
        }
    }
}