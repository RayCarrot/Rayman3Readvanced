using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class UserInfoMulti2D : Dialog
{
    public UserInfoMulti2D(Scene2D scene) : base(scene)
    {
        throw new NotImplementedException();
        EnergyShots = new int[RSMultiplayer.MaxPlayersCount];
    }

    public int WinnerId { get; set; }
    public int[] Times { get; set; }
    public bool IsPaused { get; set; }
    public bool IsGameOver { get; set; }
    public int[] EnergyShots { get; set; }
    public ushort GloboxCountdown { get; set; }
    public int GloboxMachineId { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        throw new NotImplementedException();
    }

    public int GetWinnerId()
    {
        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            return -1;
        else
            return WinnerId;
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
        throw new NotImplementedException();
    }

    public override void Init() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        throw new NotImplementedException();
    }
}