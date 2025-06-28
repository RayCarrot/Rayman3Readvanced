using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// This is the original class used for managing the multiplayer connection (receive/send). We don't
// use this to establish a connection, instead it's here to preserve the original code structure.
public static class RSMultiplayer
{
    #region GBA/N-Gage

    public const int MaxPlayersCount = 4;

    public static bool IsActive { get; set; }
    public static int MachineId { get; set; }
    public static bool IsMaster => MachineId == 0;
    public static bool IsSlave => MachineId != 0;

    public static void Init()
    {
        if (Rom.Platform == Platform.GBA)
        {
            InitImpl(2, 8);
            Reset();

            // NOTE: The game adds IRQ hooks for SERIAL_COMMUNICATION and TIMER2_OVERFLOW
            // NOTE: The game sets IME and IE registers
        }

        IsActive = true;
    }

    public static void UnInit()
    {
        if (Rom.Platform == Platform.GBA)
        {
            UnInitImpl();
            UnInitState();
        }

        // NOTE: The game calls MemoryManager_SetInternalPoolEnd()

        if (Rom.Platform == Platform.GBA)
        {
            // NOTE: The game sets IME and IE registers
            // NOTE: The game removes the IRQ hooks
        }

        IsActive = false;
    }

    #endregion

    #region GBA

    public static MubState MubState { get; set; }
    public static int PlayersCount { get; set; }
    public static byte Byte_0x57 { get; set; }

    public static void InitImpl(uint cusrgPacketSize, uint cusobjPacketBufferSize)
    {
        // NOTE: The game does more initialization here
        MubState = MubState.State0;
        Byte_0x57 = 0xFF;
        PlayersCount = 0;
    }

    public static void UnInitImpl()
    {
        Disconnect(MubState.State0);
    }

    public static void UnInitState()
    {
        MubState = MubState.UnInit;
    }

    public static void Reset()
    {
        // NOTE: The game sets IME, IE, SIODATA8, RCNT and SIOCNT registers

        MubState = MubState.State1;
        Byte_0x57 = 24;
        PlayersCount = 0;
        MachineId = 5;

        // NOTE: The game resets more data
        // NOTE: The game sets TM2CNT_L and IE registers
    }

    public static void CheckForLostConnection()
    {
        if (MubState is MubState.EstablishConnections or MubState.Connected)
        {
            // NOTE: The game checks the SIOCNT register to see if there's still a valid
            //       connection, and if not then it resets data and changes the state
        }
    }

    public static bool SendPacket(ushort[] parusPacketData)
    {
        if (MubState != MubState.Connected)
            return false;

        // NOTE: The game saves the packet in the send buffer queue
        return true;
    }

    public static bool ReleasePacket(int hubMachine)
    {
        if (MubState != MubState.Connected)
            return false;

        // NOTE: The game removes the packet from the receive buffer queue
        return true;
    }

    public static void Connect()
    {
        if (MubState == MubState.EstablishConnections && Byte_0x57 == 3 && IsMaster)
        {
            MubState = MubState.EstablishConnections;
            Byte_0x57 = 4;
        }
    }

    public static bool ReadPacket(int hubMachine, out ushort[] pparusPacketBuffer)
    {
        if (MubState != MubState.Connected)
        {
            pparusPacketBuffer = [];
            return false;
        }

        // NOTE: The game sets the packet buffer from the receive buffer
        pparusPacketBuffer = [];
        return true;
    }

    public static bool IsPacketPending(int hubMachine)
    {
        if (MubState != MubState.Connected)
            return false;

        // NOTE: The game checks if a packet is pending in the receive buffer
        return true;
    }

    public static void Disconnect(MubState state)
    {
        // NOTE: The game sets IME, IE, TM2CNT_L and RCNT registers

        MubState = state;
        Byte_0x57 = 0xFF;
        PlayersCount = 0;
        MachineId = 5;
    }

    public static void MadrInterruptHandler()
    {
        // NOTE: This is where the game handles all the connectivity code, reading and sending packets from the buffers
    }

    #endregion

    #region N-Gage

    public static bool FinishedSearchingForHosts { get; set; }
    public static PlayerConnectionState[] PlayerConnectionStates { get; set; } = new PlayerConnectionState[MaxPlayersCount];
    public static string CurrentHostName { get; set; } = "Local"; // NOTE: Hard-code
    public static int PlayerIdWhoLeftGame { get; set; }

    public static bool ReadPacket(out ushort[] packetBuffer, out int machineId)
    {
        // NOTE: The game sets the packet buffer from some data
        packetBuffer = [0];
        machineId = 1;
        return false;
    }

    public static void ReleasePacket(ushort[] packetBuffer)
    {
        // NOTE: The game deletes the allocated packet buffer here
    }

    public static void SendPacket(ushort[] packetBuffer, int dataSize, int param3)
    {
        // NOTE: The game sends the packet buffer here
    }

    public static void InitSearchForHosts()
    {
        // NOTE: The game initializes a lot of other stuff here
        PlayerIdWhoLeftGame = -1;
        MachineId = -1;
        PlayersCount = 0;
        Array.Clear(PlayerConnectionStates);

        FinishedSearchingForHosts = false;
    }

    public static int GetHostsCount()
    {
        // NOTE: The game returns the length of the array of hosts, but we hard-code it
        return MaxPlayersCount;
    }

    public static void DeInit()
    {
        // NOTE: The game de-initializes data here
        MachineId = -1;
        PlayersCount = 0;
        PlayerConnectionStates[0] = PlayerConnectionState.Disconnected;
    }

    public static void SetHost(int hostIndex)
    {
        // NOTE: The game initializes for the specified host here
        PlayerConnectionStates[0] = PlayerConnectionState.Connecting;
    }

    public static string GetHostName(int hostIndex)
    {
        // NOTE: The game gets the host name from the connection data, we hard-code it to a generic readable name
        return $"Host {hostIndex + 1}";
    }

    public static string GetCurrentHostName()
    {
        // NOTE: The game gets the host name from the connection data, we hard-code it to a generic readable name
        return "Local";
    }

    public static bool CheckAllPlayersWaiting()
    {
        for (int i = 1; i < PlayersCount; i++)
        {
            if (PlayerConnectionStates[i] is not (PlayerConnectionState.Disconnected or PlayerConnectionState.Wait))
                return false;
        }

        return true;
    }

    public static bool CheckAnyPlayerConnecting()
    {
        for (int i = 1; i < PlayersCount; i++)
        {
            if (PlayerConnectionStates[i] == PlayerConnectionState.Connecting)
                return true;
        }

        return false;
    }

    public static bool CheckAllPlayersReady()
    {
        for (int i = 1; i < PlayersCount; i++)
        {
            if (PlayerConnectionStates[i] is not (PlayerConnectionState.Disconnected or PlayerConnectionState.Ready))
                return false;
        }

        return true;
    }

    public enum PlayerConnectionState
    {
        Disconnected = 0,
        Wait = 1,
        Connecting = 2,
        Connected = 3,
        Ready = 4,
    }

    #endregion
}