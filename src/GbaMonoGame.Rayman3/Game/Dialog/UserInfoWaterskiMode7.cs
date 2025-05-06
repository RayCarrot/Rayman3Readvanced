using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class UserInfoWaterskiMode7 : Dialog
{
    public UserInfoWaterskiMode7(Scene2D scene) : base(scene)
    {
        LifeBar = new LifeBar(Scene);
        LumsBar = new LumsBar(Scene);

        ShowCountdown = false;
        IsPaused = false;
        CountdownValue = 0;
    }

    public LifeBar LifeBar { get; }
    public LumsBar LumsBar { get; }
    public AnimatedObject Countdown { get; set; }
    public AnimatedObject Birds { get; set; }
    public int CountdownValue { get; set; }
    public bool ShowCountdown { get; set; }
    public bool IsPaused { get; set; }
    public float BirdsXPosition { get; set; } // NOTE: This value is uninitialized by the game and thus might start at any value

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

    // Custom to allow hiding the bars for the pause dialog options menu
    public void MoveInBars()
    {
        LifeBar.SetToStayVisible();
        LifeBar.MoveIn();

        LumsBar.SetToStayVisible();
        LumsBar.MoveIn();
    }

    // Custom to allow hiding the bars for the pause dialog options menu
    public void MoveOutBars()
    {
        LifeBar.MoveOut();
        LumsBar.MoveOut();
    }

    public override void Load()
    {
        // NOTE: Game has it set up so Load can be called multiple times. Dynamic objects don't get recreated after the first time, but instead
        //       reloaded into VRAM. We don't need to do that though due to how the graphics system works here, so just always create everything.

        LifeBar.Load();
        LumsBar.Load();
        
        LifeBar.Set();
        LumsBar.Set();

        LifeBar.SetToStayVisible();
        LumsBar.SetToStayVisible();

        AnimatedObjectResource countdownResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.CountdownAnimations);

        Countdown = new AnimatedObject(countdownResource, true)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, 90),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        AnimatedObjectResource birdsResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.WaterskiBirdAnimations);

        Birds = new AnimatedObject(birdsResource, false)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(0, 30),
            RenderContext = Scene.HudRenderContext,
        };
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        Birds.ScreenPos = new Vector2(
            x: (cam.Direction.Inverse() + BirdsXPosition) * 2,
            y: cam.Horizon - 42);

        if (!IsPaused)
            animationPlayer.PlayFront(Birds);

        // NOTE: In the original game it increments the position every 4 frames
        BirdsXPosition += 1 / 4f;
        BirdsXPosition %= 256;

        LifeBar.Draw(animationPlayer);
        LumsBar.Draw(animationPlayer);

        if (!IsPaused && ShowCountdown)
        {
            Countdown.CurrentAnimation = CountdownValue;
            animationPlayer.PlayFront(Countdown);
        }
    }
}