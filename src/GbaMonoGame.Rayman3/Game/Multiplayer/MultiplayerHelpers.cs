namespace GbaMonoGame.Rayman3;

public static class MultiplayerHelpers
{
    public static int HudIndexToMachineId(int hudIndex)
    {
        if (hudIndex == 0)
            return MultiplayerManager.MachineId;
        else if (hudIndex <= MultiplayerManager.MachineId)
            return hudIndex - 1;
        else
            return hudIndex;
    }

    public static int MachineIdToHudIndex(int machineId)
    {
        if (machineId == MultiplayerManager.MachineId)
            return 0;
        else if (machineId <= MultiplayerManager.MachineId)
            return machineId + 1;
        else
            return machineId;
    }
}