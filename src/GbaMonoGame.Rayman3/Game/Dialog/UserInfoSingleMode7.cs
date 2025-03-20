using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class UserInfoSingleMode7 : Dialog
{
    public UserInfoSingleMode7(Scene2D scene) : base(scene)
    {
        LifeBar = new LifeBar(Scene);
        LumsBar = new LumsBar(Scene);
        TimerBar = new TimerBar(Scene);

        IsCountdownActive = false;
        IsPaused = false;
    }

    public LifeBar LifeBar { get; }
    public LumsBar LumsBar { get; }
    public TimerBar TimerBar { get; }
    public AnimatedObject Countdown { get; set; }
    public AnimatedObject Laps { get; set; }
    public AnimatedObject[] LapDigits { get; set; }
    public SpriteTextObject WrongWayText { get; set; }
    public int CountdownValue { get; set; }
    public bool IsCountdownActive { get; set; }
    public bool IsPaused { get; set; }

    // Custom to allow hiding the bars for the pause dialog options menu
    public bool HideLaps { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Handle messages
        switch (message)
        {
            case Message.UserInfo_Pause:
                IsPaused = true;
                return true;

            case Message.UserInfo_Unpause:
                IsPaused = false;
                return true;

            default:
                return false;
        }
    }

    public void SetCountdownValue(int value)
    {
        CountdownValue = value;
        IsCountdownActive = true;
        Countdown.CurrentAnimation = value;
        Countdown.IsFramed = true;
    }

    // TODO: Add move in/out transitions for timer and laps?
    // Custom to allow hiding the bars for the pause dialog options menu
    public void MoveInBars()
    {
        LifeBar.SetToStayVisible();
        LifeBar.MoveIn();

        LumsBar.SetToStayVisible();
        LumsBar.MoveIn();

        TimerBar.SetToStayVisible();

        HideLaps = false;
    }

    // Custom to allow hiding the bars for the pause dialog options menu
    public void MoveOutBars()
    {
        LifeBar.MoveOut();
        LumsBar.MoveOut();
        TimerBar.SetToStayHidden();
        HideLaps = true;
    }

    public override void Load()
    {
        // NOTE: Game has it set up so Load can be called multiple times. Dynamic objects don't get recreated after the first time, but instead
        //       reloaded into VRAM. We don't need to do that though due to how the graphics system works here, so just always create everything.

        LifeBar.Load();
        LumsBar.Load();
        TimerBar.Load();
        
        LifeBar.Set();
        // If the dialog is reloaded because the game is paused then we don't want to update the lums count since it's stored in the level until finished
        if (((FrameMode7)Frame.Current).IsPaused())
            LumsBar.SetWithoutUpdating();
        else
            LumsBar.Set();
        TimerBar.Set();

        LifeBar.SetToStayVisible();
        LumsBar.SetToStayVisible();
        TimerBar.SetToStayVisible();

        AnimatedObjectResource countdownResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.CountdownAnimations);

        Countdown = new AnimatedObject(countdownResource, true)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, 90),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        if (IsCountdownActive)
            Countdown.CurrentAnimation = CountdownValue;

        AnimatedObjectResource lapsResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.LapAndTimerAnimations);

        Laps = new AnimatedObject(lapsResource, false)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(-60, 50),
            CurrentAnimation = 10,
            HorizontalAnchor = HorizontalAnchorMode.Right,
            RenderContext = Scene.HudRenderContext,
        };

        // Hide the boost bar
        Laps.DeactivateChannel(0);
        Laps.DeactivateChannel(1);
        Laps.DeactivateChannel(2);

        LapDigits = new AnimatedObject[2];
        for (int i = 0; i < LapDigits.Length; i++)
        {
            LapDigits[i] = new AnimatedObject(lapsResource, false)
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(-30 + i * 16, 48),
                CurrentAnimation = 0,
                HorizontalAnchor = HorizontalAnchorMode.Right,
                RenderContext = Scene.HudRenderContext,
            };
        }

        WrongWayText = new SpriteTextObject()
        {
            Color = TextColor.RaceWrongWayText,
            FontSize = FontSize.Font16,
            Text = Localization.GetText(TextBankId.Connectivity, 16)[0],
            ScreenPos = new Vector2(0, 70),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };
        WrongWayText.ScreenPos = WrongWayText.ScreenPos with { X = -WrongWayText.GetStringWidth() / 2f };
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        RaceManager raceManager = ((FrameSingleMode7)Frame.Current).RaceManager;

        LifeBar.Draw(animationPlayer);
        LumsBar.Draw(animationPlayer);
        TimerBar.DrawTime(animationPlayer, raceManager.RemainingTime);

        MissileMode7 mainActor = (MissileMode7)Scene.MainActor;

        if (mainActor.CollectedBlueLums == 3 && (mainActor.BoostTimer & 0x10) != 0)
            Laps.CurrentAnimation = 10;
        else
            Laps.CurrentAnimation = 10 + mainActor.CollectedBlueLums;

        LapDigits[0].CurrentAnimation = raceManager.CurrentLap;
        LapDigits[1].CurrentAnimation = raceManager.LapsCount;

        if (!HideLaps)
        {
            animationPlayer.PlayFront(Laps);
            animationPlayer.PlayFront(LapDigits[0]);
            animationPlayer.PlayFront(LapDigits[1]);
        }

        if (!raceManager.DrivingTheRightWay && 
            (GameTime.ElapsedFrames & 0x20) != 0 && 
            !IsPaused && 
            raceManager.IsRacing)
        {
            animationPlayer.PlayFront(WrongWayText);

            if ((GameTime.ElapsedFrames & 0x4f) == 0x4f && 
                (GameTime.ElapsedFrames & 0x20) != 0 && 
                !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__OnoEquil_Mix03))
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoEquil_Mix03);
            }
        }

        if (IsCountdownActive && !IsPaused)
            animationPlayer.PlayFront(Countdown);
    }
}