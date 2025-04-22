using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class FrameWaterSkiMode7 : FrameMode7
{
    public FrameWaterSkiMode7(MapId mapId) : base(mapId) { }

    public new UserInfoWaterskiMode7 UserInfo
    {
        get => (UserInfoWaterskiMode7)base.UserInfo;
        set => base.UserInfo = value;
    }

    public uint WaterskiTimer { get; set; }

    public GfxScreen FogScreen { get; set; }
    public Mode7FogScreenRenderer FogScreenRenderer { get; set; }
    public float FadeDecrease { get; set; }

    private void InitFog()
    {
        // NOTE: The game handles the fog by updating the backdrop color based on the following scanlines and then blends it with the screen

        FogScreenRenderer = new Mode7FogScreenRenderer(
        [
            new Mode7FogScreenRenderer.FogLine(0x1, -0x20),
            new Mode7FogScreenRenderer.FogLine(0x2, -0x1C),
            new Mode7FogScreenRenderer.FogLine(0x3, -0x18),
            new Mode7FogScreenRenderer.FogLine(0x4, -0x15),
            new Mode7FogScreenRenderer.FogLine(0x5, -0x12),
            new Mode7FogScreenRenderer.FogLine(0x6, -0x0E),
            new Mode7FogScreenRenderer.FogLine(0x7, -0x0A),
            new Mode7FogScreenRenderer.FogLine(0x8, -0x06),
            new Mode7FogScreenRenderer.FogLine(0x9, -0x04),
            new Mode7FogScreenRenderer.FogLine(0xA, -0x02),
            new Mode7FogScreenRenderer.FogLine(0x9, 0x01),
            new Mode7FogScreenRenderer.FogLine(0x8, 0x04),
            new Mode7FogScreenRenderer.FogLine(0x6, 0x08),
            new Mode7FogScreenRenderer.FogLine(0x4, 0x0C),
            new Mode7FogScreenRenderer.FogLine(0x2, 0x10),
            new Mode7FogScreenRenderer.FogLine(0x0, 0x16),
        ]);

        FogScreen = new GfxScreen(5)
        {
            Priority = 0,
            Wrap = false,
            Is8Bit = null,
            Offset = Vector2.Zero,
            IsEnabled = true,
            Renderer = FogScreenRenderer,
            RenderOptions = { RenderContext = Scene.RenderContext }
        };

        Gfx.AddScreen(FogScreen);
    }

    public override void Init()
    {
        base.Init();

        InitFog();

        UserInfo = new UserInfoWaterskiMode7(Scene);
        Scene.AddDialog(UserInfo, false, false);

        WaterskiTimer = 0;
        FadeDecrease = 10;
    }

    public override void Step()
    {
        if (!IsPaused())
        {
            WaterskiTimer++;

            if (WaterskiTimer <= 200)
            {
                if (WaterskiTimer == 32)
                {
                    UserInfo.CountdownValue = 1;
                    UserInfo.ShowCountdown = true;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P1_);
                }
                else if (WaterskiTimer == 64)
                {
                    UserInfo.CountdownValue = 2;
                    UserInfo.ShowCountdown = true;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P2_);
                }
                else if (WaterskiTimer == 96)
                {
                    UserInfo.CountdownValue = 3;
                    UserInfo.ShowCountdown = true;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P3_);
                }
                else if (WaterskiTimer == 128)
                {
                    Scene.Camera.ProcessMessage(this, Message.CamMode7_Reset);
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Start);
                    UserInfo.CountdownValue = 0;
                    UserInfo.ShowCountdown = true;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoGO_Mix02);
                }
                else if (WaterskiTimer == 180)
                {
                    UserInfo.Countdown.IsFramed = false;
                    UserInfo.ShowCountdown = false;
                }

                if (WaterskiTimer <= 90)
                    FadeDecrease = (90 - WaterskiTimer) / 4f;
            }

            FogScreenRenderer.Horizon = ((TgxCameraMode7)Scene.Playfield.Camera).Horizon;
        }

        base.Step();

        FogScreen.IsEnabled = !TransitionsFX.IsFadingIn && !TransitionsFX.IsFadingOut && !IsPaused();
        FogScreenRenderer.FadeDecrease = FadeDecrease;

        if (EndOfFrame)
            GameInfo.LoadLevel(GameInfo.GetNextLevelId());
    }
}