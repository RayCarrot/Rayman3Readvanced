using System.IO;

namespace GbaMonoGame;

public static class FileManager
{
    public static string GetDataDirectory()
    {
        return Engine.DataDirectoryName;
    }

    public static string GetDataDirectory(string dir)
    {
        return Path.Combine(GetDataDirectory(), dir);
    }

    public static string GetDataFile(string file)
    {
        return Path.Combine(GetDataDirectory(), file);
    }
}