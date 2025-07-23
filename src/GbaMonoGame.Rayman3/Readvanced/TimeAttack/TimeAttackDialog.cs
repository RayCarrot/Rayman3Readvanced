using BinarySerializer.Ubisoft.GbaEngine;
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

    public TimerBar TimerBar { get; set; }
    public AnimatedObject Countdown { get; set; }

    public uint CountdownTimer { get; set; }
    public int CountdownValue { get; set; }
    
    public uint LevelTimer { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public void SetCountdownValue(int value)
    {
        CountdownValue = value;
        Countdown.CurrentAnimation = value;
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
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        TimerBar.DrawTime(animationPlayer, (int)LevelTimer);

        if (TimeAttackInfo.Mode == TimeAttackMode.Countdown && CountdownValue != -1)
            animationPlayer.PlayFront(Countdown);
    }
}