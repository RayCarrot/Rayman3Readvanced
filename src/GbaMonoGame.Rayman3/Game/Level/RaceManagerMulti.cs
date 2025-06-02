using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class RaceManagerMulti
{
    public RaceManagerMulti(Scene2D scene, UserInfoMultiMode7 userInfo, int lapsCount)
    {
        Scene = scene;
        UserInfo = userInfo;
        Timer = 0;
        LapsCount = lapsCount;
        RaceTime = -1;
        IsRacing = false;
        DrivingTheRightWay = true;
        LastAlivePlayer = -1;
        PlayersOrder = -1;

        PlayersCurrentLap = new int[RSMultiplayer.MaxPlayersCount];
        PlayersCurrentTempLap = new int[RSMultiplayer.MaxPlayersCount];
        PlayerDistances = new int[RSMultiplayer.MaxPlayersCount];
        PlayerRanks = new int[RSMultiplayer.MaxPlayersCount];
        PlayersIsDead = new bool[RSMultiplayer.MaxPlayersCount];
        PlayersOrderTimer = new int[RSMultiplayer.MaxPlayersCount];
        PlayersLastLapRaceTime = new int[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
        {
            PlayersCurrentLap[i] = 1;
            PlayersCurrentTempLap[i] = 0;
            PlayerDistances[i] = 0;
            PlayerRanks[i] = i;
            PlayersIsDead[i] = false;
            PlayersOrderTimer[i] = PlayersOrder;
            PlayersOrder--;
            PlayersLastLapRaceTime[i] = 0;
        }
    }

    public Scene2D Scene { get; }
    public UserInfoMultiMode7 UserInfo { get; }
    public uint Timer { get; set; }
    public int RaceTime { get; set; } // Unused
    public int[] PlayersLastLapRaceTime { get; set; } // Unused
    public int LapsCount { get; set; }
    public int[] PlayersCurrentTempLap { get; set; }
    public int[] PlayersCurrentLap { get; set; }
    public bool IsRacing { get; set; }
    public bool DrivingTheRightWay { get; set; }
    public bool[] PlayersIsDead { get; set; }
    public int[] PlayerDistances { get; set; }
    public int[] PlayersOrderTimer { get; set; }
    public int[] PlayerRanks { get; set; }
    public int PlayersOrder { get; set; }
    public int LastAlivePlayer { get; set; }

    public void Step()
    {
        Timer++;

        switch (Timer)
        {
            case 1:
                RaceTime = 0;
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
            RaceTime++;

        // End race if only 1 player is left alive
        if (LastAlivePlayer != -1)
        {
            UserInfo.IsGameOver = true;

            for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                Scene.GetGameObject(id).ProcessMessage(this, Message.MissileMode7_EndRace);

            IsRacing = false;
            LastAlivePlayer = -1;
        }
    }

    public void IncDistance(int machineId)
    {
        PlayerDistances[machineId]++;
        PlayersOrderTimer[machineId] = PlayersOrder;
        PlayersOrder--;

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
            PlayerRanks[id] = id;

        for (int i = 0; i < MultiplayerManager.PlayersCount - 1; i++)
        {
            for (int j = 0; j < MultiplayerManager.PlayersCount - 1 - i; j++)
            {
                int current = PlayerRanks[j];
                int next = PlayerRanks[j + 1];

                if (PlayerDistances[current] * 0x10000 + PlayersOrderTimer[current] <
                    PlayerDistances[next] * 0x10000 + PlayersOrderTimer[next])
                {
                    PlayerRanks[j] = next;
                    PlayerRanks[j + 1] = current;
                }
            }
        }
    }

    public void DecDistance(int machineId)
    {
        PlayerDistances[machineId]--;
        PlayersOrderTimer[machineId] = PlayersOrder;
        PlayersOrder--;

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
            PlayerRanks[id] = id;

        for (int i = 0; i < MultiplayerManager.PlayersCount - 1; i++)
        {
            for (int j = 0; j < MultiplayerManager.PlayersCount - 1 - i; j++)
            {
                int current = PlayerRanks[j];
                int next = PlayerRanks[j + 1];

                if (PlayerDistances[current] * 0x10000 + PlayersOrderTimer[current] <
                    PlayerDistances[next] * 0x10000 + PlayersOrderTimer[next])
                {
                    PlayerRanks[j] = next;
                    PlayerRanks[j + 1] = current;
                }
            }
        }
    }

    public int GetGridPos(int machineId)
    {
        int pos = Array.IndexOf(PlayerRanks, machineId);

        Debug.Assert(pos >= 0, "There Should be a player 1st in the race");

        if (pos == -1)
            pos = 0;

        return pos;
    }

    public void SetPlayerOut(int machineId)
    {
        PlayersIsDead[machineId] = true;

        int deadPlayers = 0;
        int alivePlayer = 0;
        int prevAlivePlayer = alivePlayer;
        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
        {
            alivePlayer = id;
            if (PlayersIsDead[id])
            {
                deadPlayers++;
                alivePlayer = prevAlivePlayer;
            }
            prevAlivePlayer = alivePlayer;
        }

        Debug.Assert(deadPlayers < MultiplayerManager.PlayersCount, "Too many players are out... no winner");

        // If only 1 player is left alive
        if (deadPlayers == MultiplayerManager.PlayersCount - 1)
        {
            PlayerDistances[alivePlayer] = 32000;
            PlayersCurrentTempLap[alivePlayer] = LapsCount + 1;
            LastAlivePlayer = alivePlayer;
            IncDistance(alivePlayer);
        }
    }
}