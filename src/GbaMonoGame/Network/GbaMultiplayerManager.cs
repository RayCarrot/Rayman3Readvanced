using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public class GbaMultiplayerManager : MultiplayerManager
{
    private const ushort IsLoadingMessage = 0x8000;

    public override int MachineId => CachedMachineId;
    public override int PlayersCount => CachedPlayersCount;

    public uint InitialGameTime { get; set; }
    public int LocalMachineTime { get; set; }
    public uint[] ClientMachineTimers { get; set; }
    public bool HasProcessedFrame { get; set; }
    public bool SkipFrame { get; set; }
    public bool IsLoading { get; set; }
    public byte LostConnectionTimer { get; set; }
    public int CachedMachineId { get; set; }
    public int CachedPlayersCount { get; set; }

    public override void ReInit()
    {
        InitialGameTime = 0;
        LocalMachineTime = 0;
        HasProcessedFrame = IsLoading;
        SkipFrame = false;
        IsLoading = false;
        LostConnectionTimer = 0;

        ClientMachineTimers = new uint[RSMultiplayer.MaxPlayersCount];

        Engine.MultiJoyPad.Init();

        CachedMachineId = RSMultiplayer.MachineId;
        CachedPlayersCount = RSMultiplayer.PlayersCount;
    }

    // NOTE: The GBA version returns the state instead of a bool, however it's only ever checked against the connected value
    public override bool Step()
    {
        // Set the initial game time for the first time if unset
        if (InitialGameTime == 0)
            InitialGameTime = GameTime.ElapsedFrames;

        if (!IsLoading)
            LostConnectionTimer++;

        // Check if the connection is still valid
        RSMultiplayer.CheckForLostConnection();

        // If connected...
        if (RSMultiplayer.MubState == MubState.Connected)
        {
            if (MachineId >= RSMultiplayer.PlayersCount)
                throw new Exception("Invalid machine id");

            // Set the timer for this machine
            ClientMachineTimers[MachineId] = GameTime.ElapsedFrames - InitialGameTime;

            // Read inputs from other clients
            for (int id = 0; id < PlayersCount; id++)
            {
                // Check if there's a packet pending
                if (RSMultiplayer.IsPacketPending(id))
                {
                    // Make sure it's not this machine
                    if (id != MachineId)
                    {
                        RSMultiplayer.ReadPacket(id, out ushort[] _);

                        // NOTE: Hard-code packet data
                        ushort packet = (ushort)((ClientMachineTimers[id] & 0x1f) << 10);

                        Engine.MultiJoyPad.Read(id, ClientMachineTimers[id], (GbaInput)packet);

                        if (!IsLoading)
                        {
                            if (packet == IsLoadingMessage)
                                IsLoading = true;
                            else if ((ClientMachineTimers[id] & 0x1f) != packet >> 10)
                                throw new Exception("Desynced multiplayer machine time");
                        }

                        // Increment timer since we've processes one frame now
                        ClientMachineTimers[id]++;
                    }

                    // Release the packet we read
                    RSMultiplayer.ReleasePacket(id);
                    LostConnectionTimer = 0;
                }
            }

            // Read our inputs
            if (!IsLoading && (HasProcessedFrame || ClientMachineTimers[MachineId] == 0))
            {
                if (!SkipFrame)
                {
                    uint? time = Engine.MultiJoyPad.GetNextInvalidTime(MachineId, ClientMachineTimers[MachineId]);

                    if (time == null)
                    {
                        SkipFrame = true;
                    }
                    else
                    {
                        Engine.MultiJoyPad.Read(MachineId, time.Value, Engine.Input.GetPressedGbaInputs());

                        GbaInput input = Engine.MultiJoyPad.GetInput(MachineId, time.Value);

                        LocalMachineTime++;

                        ushort packet = (ushort)(((LocalMachineTime << 10) & 0x7fff) | ((ushort)input & 0x3ff));
                        RSMultiplayer.SendPacket([packet]);
                    }
                }
                else
                {
                    SkipFrame = false;
                }
            }
        }

        if (LostConnectionTimer > 4)
            return false;

        if (RSMultiplayer.MubState != MubState.Connected)
            return false;

        // Custom for buffered inputs
        if (HasReadJoyPads())
            Engine.MultiJoyPad.PushBufferedJoyPads();

        return true;
    }

    public override bool HasReadJoyPads()
    {
        if (IsLoading)
            return true;

        if (Engine.MultiJoyPad.IsValid(0, ClientMachineTimers[MachineId]) &&
            Engine.MultiJoyPad.IsValid(1, ClientMachineTimers[MachineId]) &&
            (PlayersCount <= 2 || Engine.MultiJoyPad.IsValid(2, ClientMachineTimers[MachineId])) &&
            (PlayersCount <= 3 || Engine.MultiJoyPad.IsValid(3, ClientMachineTimers[MachineId])))
            return true;

        return false;
    }

    public override void FrameProcessed()
    {
        if (MachineId >= RSMultiplayer.PlayersCount)
            throw new Exception("Invalid machine id");

        HasProcessedFrame = true;
        Engine.MultiJoyPad.ReleaseJoyPads(ClientMachineTimers[MachineId]);
    }

    public override uint GetMachineTimer()
    {
        if (MachineId >= RSMultiplayer.PlayersCount)
            throw new Exception("Invalid machine id");

        return ClientMachineTimers[MachineId];
    }

    public override uint GetElapsedTime()
    {
        return GameTime.ElapsedFrames - InitialGameTime;
    }

    public void CacheData()
    {
        Debug.Assert(RSMultiplayer.MubState == MubState.Connected);

        CachedMachineId = RSMultiplayer.MachineId;
        CachedPlayersCount = RSMultiplayer.PlayersCount;
    }

    public void BeginLoad()
    {
        IsLoading = true;
        RSMultiplayer.SendPacket([IsLoadingMessage]);
    }

    public void DiscardPendingPackets()
    {
        // NOTE: Return early to avoid freezing since packets are always seen as pending
        return;

        for (int id = 0; id < RSMultiplayer.PlayersCount; id++)
        {
            while (RSMultiplayer.IsPacketPending(id))
                RSMultiplayer.ReleasePacket(id);
        }
    }
}