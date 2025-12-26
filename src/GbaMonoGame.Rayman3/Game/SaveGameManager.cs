using System;
using System.IO;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

// TODO: Add options to import/export to/from emulator saves
public static class SaveGameManager
{
    private static PhysicalFile GetSlotFile(int index)
    {
        Context context = Rom.Context;
        string fileName = $"slot{index + 1}.sav";

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
}