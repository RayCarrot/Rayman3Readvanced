using System;
using System.IO;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public static class SaveGameManager
{
    private const string SaveFileExtension = ".sav";
    private const string TimeAttackSaveFileName = "timeattack";
    private const string SaveSlotFileName = "slot";

    private static PhysicalFile GetSlotFile(int index)
    {
        string fileName = $"{SaveSlotFileName}{index + 1}";
        return GetSaveFile(fileName);
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

    public static SaveGameSlot LoadSlot(int index)
    {
        try
        {
            PhysicalFile file = GetSlotFile(index);

            if (!file.SourceFileExists)
                return null;

            Context context = Rom.Context;

            using (context)
                return FileFactory.Read<SaveGameSlot>(context, file.FilePath);
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

    public static void SaveSlot(int index, SaveGameSlot save)
    {
        try
        {
            PhysicalFile file = GetSlotFile(index);

            Context context = Rom.Context;

            using (context)
                FileFactory.Write<SaveGameSlot>(context, file.FilePath, save);
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

    public static TimeAttackSave LoadTimeAttackSave()
    {
        try
        {
            PhysicalFile file = GetSaveFile(TimeAttackSaveFileName);

            if (!file.SourceFileExists)
                return new TimeAttackSave();

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

            return new TimeAttackSave();
        }
    }

    public static void SaveTimeAttackSave(TimeAttackSave save)
    {
        try
        {
            PhysicalFile file = GetSaveFile(TimeAttackSaveFileName);

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
}