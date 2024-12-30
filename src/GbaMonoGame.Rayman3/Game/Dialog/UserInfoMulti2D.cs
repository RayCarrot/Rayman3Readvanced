using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class UserInfoMulti2D : Dialog
{
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

        State.MoveTo(FUN_08012738);
    }

    public ScoreBar ScoreBar { get; set; }

    public int TagId { get; set; }
    public int UnknownId { get; set; }
    public int[] Times { get; set; }
    public int[] EnergyShots { get; set; }
    public bool IsPaused { get; set; }
    public bool IsGameOver { get; set; }
    public ushort GloboxCountdown { get; set; }
    public int GloboxMachineId { get; set; }

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

    public int GetWinnerId()
    {
        if (!IsGameOver)
            throw new Exception("Can only get the winner id if the game is over");

        return TagId;
    }

    public int GetTime(int id)
    {
        return Times[id];
    }

    public void RemoveTime(int id, int time)
    {
        if (time < Times[id])
        {
            Times[id] -= time;
        }
        else
        {
            Times[id] = 0;

            if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
                GameOver(id);
        }

        throw new NotImplementedException();
    }

    public void GameOver(int id)
    {
        throw new NotImplementedException();
    }

    public void BeginGlobox(int machineId)
    {
        GloboxCountdown = 360;
        GloboxMachineId = machineId;
    }

    public void AddEnergyShots(int machineId, int energyShots)
    {
        EnergyShots[machineId] += energyShots;

        if (EnergyShots[machineId] > 99)
            EnergyShots[machineId] = 99;

        if (machineId == MultiplayerManager.MachineId)
            PrintEnergyShots(machineId);
    }

    public void PrintEnergyShots(int machineId)
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
}