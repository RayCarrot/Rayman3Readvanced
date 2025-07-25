﻿using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// This is the original class used for managing the multiplayer data, like the JoyPad. We don't
// use this with an actual connection, instead it's here to preserve the original code structure.
public static class MultiplayerManager
{
    #region GBA/N-Gage

    public static int MachineId => Rom.Platform switch
    {
        Platform.GBA => CachedMachineId,
        Platform.NGage => RSMultiplayer.MachineId,
        _ => throw new UnsupportedPlatformException()
    };
    public static int PlayersCount => Rom.Platform switch
    {
        Platform.GBA => CachedPlayersCount,
        Platform.NGage => RSMultiplayer.PlayersCount,
        _ => throw new UnsupportedPlatformException()
    };

    public static void Init()
    {
        // NOTE: The game allocates the MultiplayerManager here and calls the ctor on MultiJoyPad
        ReInit();
    }

    public static void ReInit()
    {
        if (Rom.Platform == Platform.GBA)
        {
            InitialGameTime = 0;
            LocalMachineTime = 0;
            HasProcessedFrame = IsLoading;
            SkipFrame = false;
            IsLoading = false;
            LostConnectionTimer = 0;

            ClientMachineTimers = new uint[RSMultiplayer.MaxPlayersCount];

            MultiJoyPad.Init();

            CachedMachineId = RSMultiplayer.MachineId;
            CachedPlayersCount = RSMultiplayer.PlayersCount;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            ElapsedFrames = 0;
            Flags = 0x80;
            JoyPadReadDelay = 0;
            PrevDisconnectedInput = GbaInput.None;
            SyncTime = 0;
            PendingSystemSyncPause = false;
            DisconnectedHasReadNewInputs = true;

            MultiJoyPad.Init();
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    // NOTE: The GBA version returns the state instead of a bool, however it's only ever checked against the connected value
    public static bool Step()
    {
        if (Rom.Platform == Platform.GBA)
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

                            MultiJoyPad.Read(id, ClientMachineTimers[id], (GbaInput)packet);

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
                        uint? time = MultiJoyPad.GetNextInvalidTime(MachineId, ClientMachineTimers[MachineId]);

                        if (time == null)
                        {
                            SkipFrame = true;
                        }
                        else
                        {
                            MultiJoyPad.Read(MachineId, time.Value, InputManager.GetPressedGbaInputs());

                            GbaInput input = MultiJoyPad.GetInput(MachineId, time.Value);

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

            return RSMultiplayer.MubState == MubState.Connected;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            // NOTE: The game has additional error checks which have not been re-implemented

            if (RSMultiplayer.PlayersCount == 2)
                Flags |= 8;

            if (SyncTime == 0)
            {
                // NOTE: The game has a condition here
                if (true)
                    StepConnected();
                else
                    StepDisconnected();
            }
            else
            {
                StepSync();

                if (SyncTime == 0)
                    PendingSystemSyncPause = true;
            }

            // NOTE: The game has a condition here
            return true;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    public static bool HasReadJoyPads()
    {
        if (Rom.Platform == Platform.GBA)
        {
            if (IsLoading)
                return true;

            if (MultiJoyPad.IsValid(0, ClientMachineTimers[MachineId]) &&
                MultiJoyPad.IsValid(1, ClientMachineTimers[MachineId]) &&
                (PlayersCount <= 2 || MultiJoyPad.IsValid(2, ClientMachineTimers[MachineId])) &&
                (PlayersCount <= 3 || MultiJoyPad.IsValid(3, ClientMachineTimers[MachineId])))
                return true;

            return false;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            return JoyPadReadDelay != 0;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    public static void FrameProcessed()
    {
        if (Rom.Platform == Platform.GBA)
        {
            if (MachineId >= RSMultiplayer.PlayersCount)
                throw new Exception("Invalid machine id");

            HasProcessedFrame = true;
            MultiJoyPad.ReleaseJoyPads(ClientMachineTimers[MachineId]);
        }
        else if (Rom.Platform == Platform.NGage)
        {
            JoyPadReadDelay--;

            if (JoyPadReadDelay == 0)
            {
                ElapsedFrames++;
                for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                    MultiJoyPad.Clear(i, ElapsedFrames);

                MultiJoyPad.ReleaseJoyPads(ElapsedFrames);
            }
            else
            {
                for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                    MultiJoyPad.NewFrame(i, ElapsedFrames);
            }

            PendingSystemSyncPause = false;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    public static uint GetMachineTimer()
    {
        if (Rom.Platform == Platform.GBA)
        {
            if (MachineId >= RSMultiplayer.PlayersCount)
                throw new Exception("Invalid machine id");

            return ClientMachineTimers[MachineId];
        }
        else if (Rom.Platform == Platform.NGage)
        {
            return ElapsedFrames;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    public static uint GetElapsedTime()
    {
        if (Rom.Platform == Platform.GBA)
        {
            return GameTime.ElapsedFrames - InitialGameTime;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            return ElapsedFrames;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    #endregion

    #region GBA

    private const ushort IsLoadingMessage = 0x8000;

    public static uint InitialGameTime { get; set; }
    public static int LocalMachineTime { get; set; }
    public static uint[] ClientMachineTimers { get; set; }
    public static bool HasProcessedFrame { get; set; }
    public static bool SkipFrame { get; set; }
    public static bool IsLoading { get; set; }
    public static byte LostConnectionTimer { get; set; }
    public static int CachedMachineId { get; set; }
    public static int CachedPlayersCount { get; set; }

    public static void CacheData()
    {
        Debug.Assert(RSMultiplayer.MubState == MubState.Connected);

        CachedMachineId = RSMultiplayer.MachineId;
        CachedPlayersCount = RSMultiplayer.PlayersCount;
    }

    public static void BeginLoad()
    {
        IsLoading = true;
        RSMultiplayer.SendPacket([IsLoadingMessage]);
    }

    public static void DiscardPendingPackets()
    {
        // NOTE: Return early to avoid freezing since packets are always seen as pending
        return;

        for (int id = 0; id < RSMultiplayer.PlayersCount; id++)
        {
            while (RSMultiplayer.IsPacketPending(id))
                RSMultiplayer.ReleasePacket(id);
        }
    }

    #endregion

    #region N-Gage

    public static uint ElapsedFrames { get; set; }
    public static byte Flags { get; set; }
    public static byte JoyPadReadDelay { get; set; }
    public static GbaInput PrevDisconnectedInput { get; set; }
    public static byte SyncTime { get; set; }
    public static bool PendingSystemSyncPause { get; set; }
    public static bool DisconnectedHasReadNewInputs { get; set; }

    public static void StepConnected()
    {
        if (JoyPadReadDelay == 0)
        {
            bool[] readPlayerPackets = new bool[RSMultiplayer.PlayersCount];

            // NOTE: The game makes a connection function call here

            // Read inputs from other clients
            bool isInvalid = false;
            while (RSMultiplayer.ReadPacket(out ushort[] packetBuffer, out int machineId))
            {
                byte flag = (byte)(1 << (machineId + 3));

                if ((Flags & flag) == flag)
                {
                    // Invalid state
                    if (packetBuffer[0] == 0x4000)
                    {
                        isInvalid = true;
                    }
                    // Invalid or already read packet from this player
                    else if (isInvalid || readPlayerPackets[machineId])
                    {
                        // NOTE: The game makes a connection function call here
                    }
                    // Read input
                    else
                    {
                        MultiJoyPad.SetInput(machineId, ElapsedFrames, (GbaInput)packetBuffer[0]);
                        readPlayerPackets[machineId] = true;
                    }
                }
                else if (packetBuffer[0] == 0x8000)
                {
                    Flags |= flag;
                }

                RSMultiplayer.ReleasePacket(packetBuffer);
            }

            // Read our inputs
            if ((Flags & 8) == 0 || HasReadClientJoyPads(ElapsedFrames))
            {
                MultiJoyPad.SetInput(0, ElapsedFrames, InputManager.GetPressedGbaInputs());
                
                ushort[] packetBuffer = new ushort[RSMultiplayer.PlayersCount];
                for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                    packetBuffer[i] = (ushort)((ushort)MultiJoyPad.GetInput(i, ElapsedFrames) & 0x7ff);

                RSMultiplayer.SendPacket(packetBuffer, RSMultiplayer.PlayersCount * 2, 4);
                JoyPadReadDelay = 4;
            }

            if (isInvalid)
            {
                SyncTime++;
                RSMultiplayer.SendPacket([0x4000], 2, 4);
            }
        }
    }

    public static void StepDisconnected()
    {
        bool sendInput = false;
        
        if ((Flags & 0x80) != 0)
        {
            RSMultiplayer.SendPacket([0x8000], 2, 0);
            Flags = (byte)(Flags & ~0x80);

            if ((Flags & 8) != 0)
                sendInput = true;
        }

        if (JoyPadReadDelay == 0)
        {
            if (RSMultiplayer.ReadPacket(out ushort[] packetBuffer, out _))
            {
                if (packetBuffer[0] == 0x4000)
                {
                    SyncTime++;
                }
                else
                {
                    for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                        MultiJoyPad.SetInput(i, ElapsedFrames, (GbaInput)packetBuffer[i]);

                    JoyPadReadDelay = 4;
                    sendInput = true;
                    DisconnectedHasReadNewInputs = true;
                }

                RSMultiplayer.ReleasePacket(packetBuffer);
            }
        }

        GbaInput input = InputManager.GetPressedGbaInputs();
        
        if ((Flags & 8) == 0 && PrevDisconnectedInput != input && DisconnectedHasReadNewInputs)
        {
            sendInput = true;
            DisconnectedHasReadNewInputs = false;
        }
        
        if (sendInput)
        {
            RSMultiplayer.SendPacket([(ushort)input], 2, 0);
            PrevDisconnectedInput = input;
        }
    }

    public static void StepSync()
    {
        // NOTE: The game has additional error checks which have not been re-implemented

        if (RSMultiplayer.ReadPacket(out ushort[] packetBuffer, out int machineId))
        {
            if (packetBuffer[0] == 0x4000)
            {
                SyncTime++;
                RSMultiplayer.SendPacket([0x4000], 2, 4);
            }
            else if (packetBuffer[0] == 0x2000)
            {
                SyncTime--;
                RSMultiplayer.SendPacket([0x2000], 2, 4);
            }
            else
            {
                // NOTE: The game makes a connection function call here
            }

            RSMultiplayer.ReleasePacket(packetBuffer);
        }
    }

    public static bool HasReadClientJoyPads(uint machineTimer)
    {
        if (MultiJoyPad.IsValid(1, machineTimer) &&
            (PlayersCount <= 2 || MultiJoyPad.IsValid(2, machineTimer)) &&
            (PlayersCount <= 3 || MultiJoyPad.IsValid(3, machineTimer)))
            return true;

        return false;
    }

    #endregion
}