namespace GbaMonoGame.Rayman3;

public static class MultiplayerHelpers
{
    public static int HudIndexToMachineId(int hudIndex)
    {
        if (hudIndex == 0)
            return Engine.Multiplayer.MachineId;
        else if (hudIndex <= Engine.Multiplayer.MachineId)
            return hudIndex - 1;
        else
            return hudIndex;
    }

    public static int MachineIdToHudIndex(int machineId)
    {
        if (machineId == Engine.Multiplayer.MachineId)
            return 0;
        else if (machineId <= Engine.Multiplayer.MachineId)
            return machineId + 1;
        else
            return machineId;
    }
}