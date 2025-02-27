using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

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
    public float FadeAdd { get; set; }

    public override void Init()
    {
        base.Init();

        ExtendMap(
        [
            new(1), new(2), new(3),
            new(33), new(34), new(32),
            new(31), new(14), new(15)
        ], 3, 3);

        // TODO: Init fog

        UserInfo = new UserInfoWaterskiMode7(Scene);
        Scene.AddDialog(UserInfo, false, false);

        WaterskiTimer = 0;
        FadeAdd = 10;
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
                    Scene.MainActor.ProcessMessage(this, Message.MainMode7_LevelStart);
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
                    FadeAdd = 90 - WaterskiTimer / 4f;
            }

            // TODO: Update fog horizon
        }

        base.Step();
        
        // TODO: Update fog

        if (EndOfFrame)
            GameInfo.LoadLevel(GameInfo.GetNextLevelId());
    }
}