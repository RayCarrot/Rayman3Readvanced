using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public class NGageMultiplayerManager : MultiplayerManager
{
    public override int MachineId => RSMultiplayer.MachineId;
    public override int PlayersCount => RSMultiplayer.PlayersCount;

    public uint ElapsedFrames { get; set; }
    public byte Flags { get; set; }
    public byte JoyPadReadDelay { get; set; }
    public GbaInput PrevDisconnectedInput { get; set; }
    public byte SyncTime { get; set; }
    public bool PendingSystemSyncPause { get; set; }
    public bool DisconnectedHasReadNewInputs { get; set; }

    public override void ReInit()
    {
        ElapsedFrames = 0;
        Flags = 0x80;
        JoyPadReadDelay = 0;
        PrevDisconnectedInput = GbaInput.None;
        SyncTime = 0;
        PendingSystemSyncPause = false;
        DisconnectedHasReadNewInputs = true;

        Engine.MultiJoyPad.Init();
    }

    public override bool Step()
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

        // Custom for buffered inputs
        if (HasReadJoyPads())
            Engine.MultiJoyPad.PushBufferedJoyPads();

        // NOTE: The game has a condition here
        return true;
    }

    public override bool HasReadJoyPads()
    {
        return JoyPadReadDelay != 0;
    }

    public override void FrameProcessed()
    {
        JoyPadReadDelay--;

        if (JoyPadReadDelay == 0)
        {
            ElapsedFrames++;
            for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                Engine.MultiJoyPad.Clear(i, ElapsedFrames);

            Engine.MultiJoyPad.ReleaseJoyPads(ElapsedFrames);
        }
        else
        {
            for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                Engine.MultiJoyPad.NewFrame(i, ElapsedFrames);
        }

        PendingSystemSyncPause = false;
    }

    public override uint GetMachineTimer()
    {
        return ElapsedFrames;
    }

    public override uint GetElapsedTime()
    {
        return ElapsedFrames;
    }

    public void StepConnected()
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
                        Engine.MultiJoyPad.SetInput(machineId, ElapsedFrames, (GbaInput)packetBuffer[0]);
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
                Engine.MultiJoyPad.SetInput(0, ElapsedFrames, Engine.Input.GetPressedGbaInputs());

                ushort[] packetBuffer = new ushort[RSMultiplayer.PlayersCount];
                for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                    packetBuffer[i] = (ushort)((ushort)Engine.MultiJoyPad.GetInput(i, ElapsedFrames) & 0x7ff);

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

    public void StepDisconnected()
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
                        Engine.MultiJoyPad.SetInput(i, ElapsedFrames, (GbaInput)packetBuffer[i]);

                    JoyPadReadDelay = 4;
                    sendInput = true;
                    DisconnectedHasReadNewInputs = true;
                }

                RSMultiplayer.ReleasePacket(packetBuffer);
            }
        }

        GbaInput input = Engine.Input.GetPressedGbaInputs();

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

    public void StepSync()
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

    public bool HasReadClientJoyPads(uint machineTimer)
    {
        if (Engine.MultiJoyPad.IsValid(1, machineTimer) &&
            (PlayersCount <= 2 || Engine.MultiJoyPad.IsValid(2, machineTimer)) &&
            (PlayersCount <= 3 || Engine.MultiJoyPad.IsValid(3, machineTimer)))
            return true;

        return false;
    }
}