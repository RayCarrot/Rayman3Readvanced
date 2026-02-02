namespace GbaMonoGame.Rayman3.Readvanced;

public static class TimeAttackDataManager
{
    private static TimeAttackSave Save { get; set; }

    private static void EnsureSaveIsLoaded()
    {
        Save ??= SaveGameManager.LoadTimeAttackSave() ?? new TimeAttackSave();
    }

    public static void Unload()
    {
        Save = null;
    }

    public static TimeAttackTime? GetRecordTime(MapId mapId)
    {
        EnsureSaveIsLoaded();

        int time = Save.Times[(int)mapId];

        if (time <= 0)
            return null;
        else
            return new TimeAttackTime(TimeAttackTimeType.Record, time);
    }

    public static void SaveRecordTime(MapId mapId, int time, TimeAttackGhostSave ghostSave)
    {
        EnsureSaveIsLoaded();

        // Save the time
        Save.Times[(int)mapId] = time;
        SaveGameManager.SaveTimeAttackSave(Save);

        // Save the ghost data
        if (ghostSave != null)
            SaveGameManager.SaveTimeAttackGhost(ghostSave, mapId);
    }
}