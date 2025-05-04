using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class FrameMultiCaptureTheFlag : FrameMultiSideScroller
{
    public FrameMultiCaptureTheFlag(MapId mapId) : base(mapId)
    {
        IsMatchOver = false;
        IsCurrentRoundOver = false;
        IsFirstRound = true;
        PlayerFlagCounts = new int[RSMultiplayer.MaxPlayersCount];
        TargetFlagsCount = 3;
    }

    public uint RemainingTime { get; set; }
    public bool IsTimed { get; set; }
    public int TargetFlagsCount { get; set; }
    public CaptureTheFlagMode Mode { get; set; }

    public bool IsCurrentRoundOver { get; set; }
    public bool IsMatchOver { get; set; }
    public bool IsFirstRound { get; set; } // Never checked for
    public int[] PlayerFlagCounts { get; }
    public int LastPlayerToGetFlag { get; set; }

    public void InitNewGame(ushort remainingTime, int targetFlagsCount, CaptureTheFlagMode mode)
    {
        RemainingTime = remainingTime;
        IsTimed = remainingTime != 0;
        TargetFlagsCount = targetFlagsCount;
        Mode = mode;
    }

    public void AddFlag(int machineId)
    {
        IsFirstRound = false;

        if (MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Solo)
        {
            if (TargetFlagsCount == 0)
            {
                if (PlayerFlagCounts[machineId] < 99)
                {
                    PlayerFlagCounts[machineId]++;
                }

                IsCurrentRoundOver = true;
            }
            else
            {
                if (PlayerFlagCounts[machineId] < 9)
                    PlayerFlagCounts[machineId]++;

                if (PlayerFlagCounts[machineId] == TargetFlagsCount)
                    IsMatchOver = true;
                else
                    IsCurrentRoundOver = true;
            }
        }
        else
        {
            if (TargetFlagsCount == 0)
            {
                if (PlayerFlagCounts[machineId / 2] < 99)
                    PlayerFlagCounts[machineId / 2]++;
            }
            else
            {
                if (PlayerFlagCounts[machineId / 2] < 9)
                    PlayerFlagCounts[machineId / 2]++;

                if (PlayerFlagCounts[machineId / 2] == TargetFlagsCount)
                    IsMatchOver = true;
                else
                    IsCurrentRoundOver = true;
            }
        }

        if (UserInfo.IsSuddenDeath)
            IsMatchOver = true;

        LastPlayerToGetFlag = machineId;
    }

    public int GetWinnerFromTimeOut()
    {
        int winnerId = 0;
        bool isTie = false;
        bool[] playersIsLoosing = new bool[4];

        playersIsLoosing[0] = false;
        for (int i = 1; i < MultiplayerManager.PlayersCount; i++)
        {
            if (PlayerFlagCounts[i] > PlayerFlagCounts[winnerId])
            {
                for (int j = 0; j < i; j++)
                    playersIsLoosing[j] = true;

                playersIsLoosing[i] = false;

                isTie = false;
                winnerId = i;
            }
            else if (PlayerFlagCounts[i] == PlayerFlagCounts[winnerId] && i > 0)
            {
                playersIsLoosing[i] = false;
                isTie = true;
            }
            else
            {
                playersIsLoosing[i] = true;
            }
        }

        if (isTie)
        {
            // If it's a tie and a solo match, then the players with a loosing score will spectate the tied players
            if (MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Solo)
            {
                for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                {
                    if (playersIsLoosing[i])
                    {
                        Rayman player = Scene.GetGameObject<Rayman>(i);

                        int tiedPlayerId;
                        do
                        {
                            tiedPlayerId = Random.GetNumber(MultiplayerManager.PlayersCount);
                        } while (playersIsLoosing[tiedPlayerId]);

                        player.ProcessMessage(this, Message.Rayman_SpectateTiedPlayer, tiedPlayerId);
                    }
                }
            }

            winnerId = -1;
        }

        return winnerId;
    }

    public void SetPlayerRanks(int[] playerRanks)
    {
        if (MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Solo)
        {
            int[] playerFlagCountsCopy = new int[PlayerFlagCounts.Length];
            Array.Copy(PlayerFlagCounts, playerFlagCountsCopy, PlayerFlagCounts.Length);

            for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
            {
                for (int j = 0; j < MultiplayerManager.PlayersCount - 1; j++)
                {
                    if (playerFlagCountsCopy[j] < playerFlagCountsCopy[j + 1])
                    {
                        (playerFlagCountsCopy[j], playerFlagCountsCopy[j + 1]) = (playerFlagCountsCopy[j + 1], playerFlagCountsCopy[j]);
                        (playerRanks[j], playerRanks[j + 1]) = (playerRanks[j + 1], playerRanks[j]);
                    }
                }
            }
        }
        else
        {
            if (PlayerFlagCounts[0] < PlayerFlagCounts[1])
            {
                playerRanks[0] = 2;
                playerRanks[1] = 3;
                playerRanks[2] = 0;
                playerRanks[3] = 1;
            }
            else
            {
                playerRanks[0] = 0;
                playerRanks[1] = 1;
                playerRanks[2] = 2;
                playerRanks[3] = 3;
            }
        }
    }

    public override void Init()
    {
        IsCurrentRoundOver = false;
        LastPlayerToGetFlag = -1;
        base.Init();
    }
}