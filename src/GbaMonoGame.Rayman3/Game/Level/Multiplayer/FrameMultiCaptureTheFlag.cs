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
            if (MultiplayerInfo.CaptureTheFlagMode != CaptureTheFlagMode.Teams)
            {
                for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                {
                    if (playersIsLoosing[i])
                    {
                        Rayman player = Scene.GetGameObject<Rayman>(i);

                        int otherPlayerId;
                        do
                        {
                            otherPlayerId = Random.GetNumber(MultiplayerManager.PlayersCount);
                        } while (playersIsLoosing[otherPlayerId]);

                        player.ProcessMessage(this, Message.Rayman_1116, otherPlayerId);
                    }
                }
            }

            winnerId = -1;
        }

        return winnerId;
    }

    public override void Init()
    {
        IsCurrentRoundOver = false;
        LastPlayerToGetFlag = -1;
        base.Init();
    }
}