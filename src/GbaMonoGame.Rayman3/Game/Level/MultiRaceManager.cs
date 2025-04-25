using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class MultiRaceManager
{
    public MultiRaceManager(Scene2D scene, UserInfoMultiMode7 userInfo, int lapsCount)
    {
        Scene = scene;
        UserInfo = userInfo;
        Timer = 0;
        LapsCount = lapsCount;
        RemainingTime = -1;
        IsRacing = false;
        DrivingTheRightWay = true;
        Data8 = 0xff;
        Data7 = -1;

        PlayersCurrentLap = new int[RSMultiplayer.MaxPlayersCount];
        Data2 = new int[RSMultiplayer.MaxPlayersCount];
        Data4 = new int[RSMultiplayer.MaxPlayersCount];
        PlayerRanks = new int[RSMultiplayer.MaxPlayersCount];
        Data3 = new int[RSMultiplayer.MaxPlayersCount];
        Data5 = new int[RSMultiplayer.MaxPlayersCount];
        Data1 = new int[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
        {
            PlayersCurrentLap[i] = 1;
            Data2[i] = 0;
            Data4[i] = 0;
            PlayerRanks[i] = i;
            Data3[i] = 0;
            Data5[i] = Data7;
            Data7--;
            Data1[i] = 0;
        }
    }

    // TODO: Name
    public Scene2D Scene { get; }
    public UserInfoMultiMode7 UserInfo { get; }
    public uint Timer { get; set; }
    public int RemainingTime { get; set; }
    public int[] Data1 { get; set; }
    public int LapsCount { get; set; }
    public int[] Data2 { get; set; }
    public int[] PlayersCurrentLap { get; set; }
    public bool IsRacing { get; set; }
    public bool DrivingTheRightWay { get; set; }
    public int[] Data3 { get; set; }
    public int[] Data4 { get; set; }
    public int[] Data5 { get; set; }
    public int[] PlayerRanks { get; set; }
    public int Data7 { get; set; }
    public int Data8 { get; set; }

    public void Step()
    {
        Timer++;

        switch (Timer)
        {
            case 1:
                RemainingTime = 0;
                break;

            case 60:
                UserInfo.SetCountdownValue(3);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P1_);
                break;

            case 120:
                UserInfo.SetCountdownValue(2);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P2_);
                break;

            case 180:
                UserInfo.SetCountdownValue(1);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P3_);
                break;

            case 240:
                UserInfo.SetCountdownValue(0);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoGO_Mix02);
                IsRacing = true;

                for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                    Scene.GetGameObject(id).ProcessMessage(this, Message.MissileMode7_StartRace);
                break;

            case 300:
                UserInfo.IsCountdownActive = false;
                break;
        }

        if (IsRacing)
            RemainingTime++;

        if (Data8 != 0xFF)
        {
            UserInfo.IsGameOver = true;

            for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                Scene.GetGameObject(id).ProcessMessage(this, Message.MissileMode7_EndRace);

            IsRacing = false;
            Data8 = 0xFF;
        }
    }
}