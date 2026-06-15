using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace GbaMonoGame.Rayman3.J2me;

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
    }

    private ZipArchive ZipArchive { get; }

    public ZipArchiveEntry GetFile(string filePath)
    {
        return ZipArchive.GetEntry(filePath) ?? 
               throw new FileNotFoundException($"File '{filePath}' not found in archive");
    }

    public IReadOnlyCollection<ZipArchiveEntry> GetEntries()
    {
        return ZipArchive.Entries;
    }

    public JavaManifest ReadManifest()
    {
        ZipArchiveEntry manifestEntry = ZipArchive.GetEntry("META-INF/MANIFEST.MF");
        if (manifestEntry == null)
            throw new Exception("Invalid JAR file");

        using Stream manifestStream = manifestEntry.Open();
        return new JavaManifest(manifestStream);
    }

    public void Dispose()
    {
        ZipArchive.Dispose();
    }
}