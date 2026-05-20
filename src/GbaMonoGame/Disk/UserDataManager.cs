using System.IO;

namespace GbaMonoGame;

public class UserDataManager
{
    public string GetBasePath()
    {
        return Paths.UserDataDirectoryName;
    }

    public string GetDirectory(string dir)
    {
        return Path.Combine(GetBasePath(), dir);
    }

    public string GetFile(string file)
    {
        return Path.Combine(GetBasePath(), file);
    }
}