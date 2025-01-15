using System.IO;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

// TODO: Handle exceptions
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
        PhysicalFile file = GetSlotFile(index);
        return file.SourceFileExists;
    }

    public static SaveGameSlot LoadSlot(int index)
    {
        PhysicalFile file = GetSlotFile(index);

        if (!file.SourceFileExists)
            throw new FileNotFoundException();

        Context context = Rom.Context;

        using (context)
            return FileFactory.Read<SaveGameSlot>(context, file.FilePath);
    }

    public static void SaveSlot(int index, SaveGameSlot save)
    {
        PhysicalFile file = GetSlotFile(index);

        Context context = Rom.Context;

        using (context)
            FileFactory.Write<SaveGameSlot>(context, file.FilePath, save);
    }

    public static void DeleteSlot(int index)
    {
        PhysicalFile file = GetSlotFile(index);
        File.Delete(file.AbsolutePath);
    }
}