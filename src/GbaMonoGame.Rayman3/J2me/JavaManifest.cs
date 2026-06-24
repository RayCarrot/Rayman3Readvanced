using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GbaMonoGame.Rayman3.J2me;

public class JavaManifest
{
    public JavaManifest(Stream manifestStream)
    {
        Values = new();
        using StreamReader manifestReader = new(manifestStream, Encoding.UTF8);
        while (manifestReader.ReadLine() is { } line)
        {
            int separatorIndex = line.IndexOf(':');
            if (separatorIndex == -1)
                continue;
            string name = line[..separatorIndex];
            string value = line[(separatorIndex + 2)..];
            Values[name] = value;
        }
    }

    private Dictionary<string, string> Values { get; }

    public string GetValue(string name)
    {
        return Values.GetValueOrDefault(name);
    }
}