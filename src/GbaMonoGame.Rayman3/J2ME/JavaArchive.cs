using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GbaMonoGame.Rayman3.J2ME;

public sealed class JavaArchive : IDisposable
{
    public JavaArchive(string filePath, bool cache)
    {
        // Open the archive
        if (cache)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            ZipArchive = new ZipArchive(new MemoryStream(fileData), ZipArchiveMode.Read);
        }
        else
        {
            ZipArchive = new ZipArchive(File.OpenRead(filePath), ZipArchiveMode.Read);
        }

        // Get the manifest
        Manifest = new();
        ZipArchiveEntry manifestEntry = ZipArchive.GetEntry("META-INF/MANIFEST.MF");
        if (manifestEntry == null)
            throw new Exception("Invalid JAR file");
        
        // Parse the manifest
        using Stream manifestStream = manifestEntry.Open();
        using StreamReader manifestReader = new(manifestStream, Encoding.UTF8);
        while (manifestReader.ReadLine() is { } line)
        {
            int separatorIndex = line.IndexOf(':');
            if (separatorIndex == -1)
                continue;
            string name = line[..separatorIndex];
            string value = line[(separatorIndex + 2)..];
            Manifest[name] = value;
        }
    }

    private ZipArchive ZipArchive { get; }
    private Dictionary<string, string> Manifest { get; }

    public string GetManifestValue(string name)
    {
        return Manifest[name];
    }

    public ZipArchiveEntry GetFile(string filePath)
    {
        return ZipArchive.GetEntry(filePath) ?? 
               throw new FileNotFoundException($"File '{filePath}' not found in archive");
    }

    public IReadOnlyCollection<ZipArchiveEntry> GetEntries()
    {
        return ZipArchive.Entries;
    }

    public void Dispose()
    {
        ZipArchive.Dispose();
    }
}