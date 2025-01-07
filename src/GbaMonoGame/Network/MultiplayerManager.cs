using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// TODO: Update for N-Gage
public static class MultiplayerManager
{
    public static uint InitialGameTime { get; set; }
    public static int MachineId { get; set; }
    public static int PlayersCount { get; set; }
    public static uint[] MachineTimers { get; set; }
    public static bool HasInvalidatedCurrentFrameInputs { get; set; }

    // TODO: Name
    public static int field_0x4 { get; set; }
    public static byte field_0x19 { get; set; }
    public static bool field_0x1a { get; set; }
    public static byte field_0x1b { get; set; }
    
    public static void Init()
    {
        InitialGameTime = 0;
        field_0x4 = 0;
        HasInvalidatedCurrentFrameInputs = field_0x1a;
        field_0x19 = 0;
        field_0x1a = false;
        field_0x1b = 0;

        MachineTimers = new uint[RSMultiplayer.MaxPlayersCount];

        MultiJoyPad.Init();

        MachineId = RSMultiplayer.MachineId;
        PlayersCount = RSMultiplayer.PlayersCount;
    }

    public static MubState Step()
    {
        if (InitialGameTime == 0)
            InitialGameTime = GameTime.ElapsedFrames;

        if (!field_0x1a)
            field_0x1b++;

        RSMultiplayer.CheckForLostConnection();

        if (RSMultiplayer.MubState == MubState.Connected)
        {
            MachineTimers[MachineId] = GameTime.ElapsedFrames - InitialGameTime;

            // Read inputs from other clients
            for (int id = 0; id < PlayersCount; id++)
            {
                if (RSMultiplayer.IsPacketPending(id))
                {
                    if (id != MachineId)
                    {
                        ushort packet = RSMultiplayer.ReadPacket(id)[0];
                        MultiJoyPad.Read(id, MachineTimers[id], (GbaInput)packet);

                        if (!field_0x1a)
                        {
                            if (packet == 0x8000)
                                field_0x1a = true;
                            // TODO: Temporarily disabled to avoid exception when testing
                            //else if ((MachineTimers[id] & 0x1f) != packet >> 10)
                            //    throw new Exception("Desynced multiplayer machine time");
                        }

                        MachineTimers[id]++;
                    }

                    RSMultiplayer.ReleasePacket(id);
                    field_0x1b = 0;
                }
            }

            // Read our inputs
            if (!field_0x1a && (HasInvalidatedCurrentFrameInputs || MachineTimers[MachineId] == 0))
            {
                if (field_0x19 == 0)
                {
                    uint? time = MultiJoyPad.GetNextInvalidTime(MachineId, MachineTimers[MachineId]);

                    if (time == null)
                    {
                        field_0x19 = 1;
                    }
                    else
                    {
                        MultiJoyPad.Read(MachineId, time.Value, InputManager.GetGbaInputs());
                        
                        GbaInput input = MultiJoyPad.GetInput(MachineId, time.Value);

                        field_0x4++;

                        ushort packet = (ushort)(((field_0x4 << 10) & 0x7fff) | ((ushort)input & 0x3ff));
                        RSMultiplayer.SendPacket([packet]);
                    }
                }
                else
                {
                    field_0x19 = 0;
                }
            }
        }

        if (field_0x1b > 4)
            return MubState.Error;

        return RSMultiplayer.MubState;
    }

    public static bool HasReadJoyPads()
    {
        if (field_0x1a) 
            return true;
        
        if (MultiJoyPad.IsValid(0, MachineTimers[MachineId]) &&
            MultiJoyPad.IsValid(1, MachineTimers[MachineId]) &&
            (PlayersCount <= 2 || MultiJoyPad.IsValid(2, MachineTimers[MachineId])) &&
            (PlayersCount <= 3 || MultiJoyPad.IsValid(3, MachineTimers[MachineId])))
            return true;

        return false;
    }

    public static void ReleaseJoyPads()
    {
        HasInvalidatedCurrentFrameInputs = true;
        MultiJoyPad.ReleaseJoyPads(MachineTimers[MachineId]);
    }

    public static void UpdateFromRSMultiplayer()
    {
        MachineId = RSMultiplayer.MachineId;
        PlayersCount = RSMultiplayer.PlayersCount;
    }

    public static uint GetMachineTimer()
    {
        return MachineTimers[MachineId];
    }

    public static uint GetElapsedTime()
    {
        // TODO: N-Gage has a separate value for this
        return GameTime.ElapsedFrames - InitialGameTime;
    }

    public static void FUN_080ae49c()
    {
        // TODO: Implement
    }

    public static void DiscardPendingPackets()
    {
        // TODO: Temporarily disabled to avoid freezing when testing
        //for (int id = 0; id < RSMultiplayer.PlayersCount; id++)
        //{
        //    while (RSMultiplayer.IsPacketPending(id))
        //        RSMultiplayer.ReleasePacket(id);
        //}
    }
}