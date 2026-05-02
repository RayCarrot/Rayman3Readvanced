using System;
using System.IO;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class SaveGameManager
{
    private const string SaveDirectoryName = "Saves";
    private const string TimeAttackGhostsDirectoryName = "Ghosts";
    
    private const string SaveFileExtension = ".sav";
    private const string SaveSlotFileName = "slot";
    private const string AchievementsSaveFileName = "achievements";
    private const string TimeAttackSaveFileName = "timeattack";
    private const string TimeAttackGhostFileName = "ghost";

    private static PhysicalFile GetSlotFile(int index)
    {
        string fileName = $"{SaveSlotFileName}{index + 1}";
        return GetSaveFile(Path.Combine(SaveDirectoryName, fileName));
    }

    private static PhysicalFile GetAchievementsFile()
    {
        return GetSaveFile(Path.Combine(SaveDirectoryName, AchievementsSaveFileName));
    }

    private static PhysicalFile GetTimeAttackFile()
    {
        return GetSaveFile(Path.Combine(SaveDirectoryName, TimeAttackSaveFileName));
    }

    private static PhysicalFile GetTimeAttackGhostFile(MapId mapId)
    {
        string fileName = $"{TimeAttackGhostFileName}_{(int)mapId}";
        return GetSaveFile(Path.Combine(TimeAttackGhostsDirectoryName, fileName));
    }

    private static PhysicalFile GetSaveFile(string fileName)
    {
        Context context = Rom.Context;

        fileName += SaveFileExtension;

        if (context.FileExists(fileName))
        {
            return (PhysicalFile)context.GetFile(fileName);
        }
        else
        {
            return context.AddFile(new LinearFile(context, fileName)
            {
                IgnoreCacheOnRead = true,
                RecreateOnWrite = true,
            });
        }
    }

    public static bool SlotExists(int index)
    {
        try
        {
            PhysicalFile file = GetSlotFile(index);
            return file.SourceFileExists;
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when finding the save.",
                header: "Error finding game save");

            return false;
        }
    }

    public static ReadvancedSlot LoadSlot(int index)
    {
        try
        {
            PhysicalFile file = GetSlotFile(index);

            if (!file.SourceFileExists)
                return null;

            Context context = Rom.Context;

            using (context)
                return FileFactory.Read<ReadvancedSlot>(context, file.FilePath);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the save.",
                header: "Error reading game save");

            return null;
        }
    }

    public static void SaveSlot(int index, ReadvancedSlot save)
    {
        try
        {
            PhysicalFile file = GetSlotFile(index);

            Context context = Rom.Context;

            using (context)
                FileFactory.Write<ReadvancedSlot>(context, file.FilePath, save);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game.",
                header: "Error saving game");   
        }
    }

    public static void DeleteSlot(int index)
    {
        try
        {
            PhysicalFile file = GetSlotFile(index);
            File.Delete(file.AbsolutePath);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when deleting the save.",
                header: "Error deleting game save");
        }
    }

    public static AchievementsSave LoadAchievementsSave()
    {
        try
        {
            PhysicalFile file = GetAchievementsFile();

            if (!file.SourceFileExists)
                return null;

            Context context = Rom.Context;

            using (context)
                return FileFactory.Read<AchievementsSave>(context, file.FilePath);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the achievements save.",
                header: "Error reading achievements save");

            return null;
        }
    }

    public static void SaveAchievementsSave(AchievementsSave save)
    {
        try
        {
            PhysicalFile file = GetAchievementsFile();

            Context context = Rom.Context;

            using (context)
                FileFactory.Write<AchievementsSave>(context, file.FilePath, save);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the achievements save.",
                header: "Error saving achievements save");
        }
    }

    public static TimeAttackSave LoadTimeAttackSave()
    {
        try
        {
            PhysicalFile file = GetTimeAttackFile();

            if (!file.SourceFileExists)
                return null;

            Context context = Rom.Context;

            using (context)
                return FileFactory.Read<TimeAttackSave>(context, file.FilePath);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the time attack save.",
                header: "Error reading time attack save");

            return null;
        }
    }

    public static void SaveTimeAttackSave(TimeAttackSave save)
    {
        try
        {
            PhysicalFile file = GetTimeAttackFile();

            Context context = Rom.Context;

            using (context)
                FileFactory.Write<TimeAttackSave>(context, file.FilePath, save);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the time attack save.",
                header: "Error saving time attack save");
        }
    }

    public static TimeAttackGhostSave LoadTimeAttackGhost(MapId mapId)
    {
        try
        {
            PhysicalFile file = GetTimeAttackGhostFile(mapId);

            if (!file.SourceFileExists)
                return null;

            Context context = Rom.Context;

            using (context)
                return FileFactory.Read<TimeAttackGhostSave>(context, file.FilePath);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the time attack ghost.",
                header: "Error reading time attack ghost");

            return null;
        }
    }

    public static void SaveTimeAttackGhost(TimeAttackGhostSave save, MapId mapId)
    {
        try
        {
            PhysicalFile file = GetTimeAttackGhostFile(mapId);

            Context context = Rom.Context;

            using (context)
                FileFactory.Write<TimeAttackGhostSave>(context, file.FilePath, save);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the time attack ghost.",
                header: "Error saving time attack ghost");
        }
    }
}