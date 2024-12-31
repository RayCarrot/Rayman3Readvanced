using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class UserInfoMulti2D : Dialog
{
    #region Constructor

    public UserInfoMulti2D(Scene2D scene) : base(scene)
    {
        switch (MultiplayerInfo.GameType)
        {
            case MultiplayerGameType.RayTag:
                Times = new int[RSMultiplayer.MaxPlayersCount];
                EnergyShots = new int[RSMultiplayer.MaxPlayersCount];
                for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
                {
                    Times[i] = 60;
                    EnergyShots[i] = 0;
                }
                break;
            
            case MultiplayerGameType.CatAndMouse:
                Times = new int[RSMultiplayer.MaxPlayersCount];
                EnergyShots = new int[RSMultiplayer.MaxPlayersCount];
                for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
                {
                    Times[i] = 0;
                    EnergyShots[i] = 0;
                }
                break;
            
            case MultiplayerGameType.CaptureTheFlag when Engine.Settings.Platform == Platform.NGage:
                EnergyShots = new int[RSMultiplayer.MaxPlayersCount];
                for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
                    EnergyShots[i] = 0;

                throw new NotImplementedException();
                break;

            case MultiplayerGameType.Missile:
            default:
                throw new InvalidOperationException("Invalid game type");
        }

        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
        {
            TagId = -1;
            UnknownId = -1;
        }
        else
        {
            TagId = (int)(MultiplayerInfo.InitialGameTime % RSMultiplayer.PlayersCount);

            if (TagId == RSMultiplayer.MachineId)
            {
                UnknownId = 0;
            }
            else
            {
                int id = TagId;
                if (id <= RSMultiplayer.MachineId)
                    id++;
                UnknownId = id;
            }
        }

        // TODO: Implement

        ScoreBar = new ScoreBar(Scene);

        State.MoveTo(Fsm_Play);
    }

    #endregion

    #region Properties

    public ScoreBar ScoreBar { get; set; }

    public int TagId { get; set; }
    public int UnknownId { get; set; }

    public uint Timer { get; set; }
    public uint LastTimeChangeTime { get; set; }

    public int[] Times { get; set; }
    public int[] EnergyShots { get; set; }

    public bool IsPaused { get; set; }
    public bool IsGameOver { get; set; }

    public ushort GloboxCountdown { get; set; }
    public int GloboxMachineId { get; set; }

    public ushort UnknownCaptureTheFlagValue1 { get; set; }

    #endregion

    #region Methods

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        return false;
    }

    public int GetTagId()
    {
        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            return -1;
        else
            return TagId;
    }

    public void SetTagId(int machineId)
    {
        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            return;

        if (machineId == TagId || IsGameOver)
            return;

        EnergyShots[TagId] = 0;
        EnergyShots[machineId] = 0;

        if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
        {
            if (machineId == MultiplayerManager.MachineId)
                PrintEnergyShots(machineId);
        }
        else if (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse)
        {
            if (TagId == MultiplayerManager.MachineId)
                PrintEnergyShots(TagId);
        }

        TagId = machineId;
        LastTimeChangeTime = Timer;

        if (TagId == RSMultiplayer.MachineId)
        {
            UnknownId = 0;
        }
        else
        {
            int id = TagId;
            if (id <= RSMultiplayer.MachineId)
                id++;
            UnknownId = id;
        }

        // TODO: Set ChainedSparkle value

        SetArrow();

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
        {
            if (id != TagId)
                Scene.GetGameObject(id).ProcessMessage(this, Message.Main_1076);
        }
    }

    public int GetNewTagId()
    {
        int newTagId = -1;
        int highestTime = 0;

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
        {
            if (Times[id] > highestTime)
            {
                newTagId = id;
                highestTime = Times[id];
            }
        }

        return newTagId;
    }

    public int GetWinnerId()
    {
        if (!IsGameOver)
            throw new Exception("Can only get the winner id if the game is over");

        return TagId;
    }

    public int GetTime(int machineId)
    {
        return Times[machineId];
    }

    // Unused
    public void AddTime(int machineId, int time)
    {
        Times[machineId] += time;
        LastTimeChangeTime = Timer;

        switch (MultiplayerInfo.GameType)
        {
            case MultiplayerGameType.RayTag:
                if (Times[machineId] > 60)
                    Times[machineId] = 60;
                break;
            
            case MultiplayerGameType.CatAndMouse:
                if (Times[machineId] >= 60)
                {
                    Times[machineId] = 60;
                    GameOverCatAndMouse(machineId);
                }
                break;

            case MultiplayerGameType.CaptureTheFlag when Engine.Settings.Platform == Platform.NGage:
                if (UnknownCaptureTheFlagValue1 > 360)
                    UnknownCaptureTheFlagValue1 = 360;
                break;

            case MultiplayerGameType.Missile:
            default:
                throw new InvalidOperationException("Invalid game type");
        }

        PrintTime();
    }

    public void RemoveTime(int machineId, int time)
    {
        if (time < Times[machineId])
        {
            Times[machineId] -= time;
        }
        else
        {
            Times[machineId] = 0;

            if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
                GameOverTag(machineId);
        }

        LastTimeChangeTime = Timer;
        PrintTime();
    }

    public void AddEnergyShots(int machineId, int energyShots)
    {
        EnergyShots[machineId] += energyShots;

        if (EnergyShots[machineId] > 99)
            EnergyShots[machineId] = 99;

        if (machineId == MultiplayerManager.MachineId)
            PrintEnergyShots(machineId);
    }

    public void DecrementEnergyShots(int machineId, int energyShots)
    {
        if (energyShots < EnergyShots[machineId])
            EnergyShots[machineId] -= energyShots;
        else
            EnergyShots[machineId] = 0;

        if (machineId == MultiplayerManager.MachineId)
            PrintEnergyShots(machineId);
    }

    public void GameOverTag(int machineId)
    {
        throw new NotImplementedException();
    }

    public void GameOverCatAndMouse(int machineId)
    {
        throw new NotImplementedException();
    }

    public void GameOverCaptureTheFlag1(int machineId)
    {
        throw new NotImplementedException();
    }

    public void GameOverCaptureTheFlag2()
    {
        throw new NotImplementedException();
    }

    public void InitGlobox(int machineId)
    {
        GloboxCountdown = 360;
        GloboxMachineId = machineId;
    }

    public void PrintTime()
    {
        throw new NotImplementedException();
    }

    public void PrintEnergyShots(int machineId)
    {
        // TODO: Implement
    }

    // Unused
    public void PrintInfo(string info, int time)
    {
        // TODO: Implement
    }

    public void MoveBouncingSprites()
    {
        // TODO: Implement
    }

    public void DrawItemEffect()
    {
        // TODO: Implement
    }

    public void SetArrow()
    {
        // TODO: Implement
    }

    public override void Load()
    {
        // TODO: Implement
    }

    public override void Init() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        // TODO: Implement
    }

    #endregion
}