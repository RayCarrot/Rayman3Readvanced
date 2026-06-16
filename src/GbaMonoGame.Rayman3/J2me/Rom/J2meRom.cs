using System;
using System.IO;
using System.IO.Compression;
using BinarySerializer;
using BinarySerializer.Gameloft.J2me;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.J2me;

public static class J2meRom
{
    #region Fields

    private static string _gameDirectory;
    private static string _gameFileName;
    private static Rayman3J2meVersion _version;
    private static Vector2 _originalResolution;
    private static Context _context;
    private static JavaManifest _manifest;
    private static ArchiveDefine[] _archiveDefines;
    private static ArchiveHeader[] _archiveHeaders;

    #endregion

    #region Properties

    public static bool IsLoaded { get; private set; }

    public static string GameDirectory => IsLoaded ? _gameDirectory : throw new RomNotInitializedException();
    public static string GameFileName => IsLoaded ? _gameFileName : throw new RomNotInitializedException();
    public static Rayman3J2meVersion Version => IsLoaded ? _version : throw new RomNotInitializedException();

    public static Vector2 OriginalResolution => IsLoaded ? _originalResolution : throw new RomNotInitializedException();
    public static Point OriginalIntegerResolution => IsLoaded ? _originalResolution.ToPoint() : throw new RomNotInitializedException();

    public static Context Context => IsLoaded ? _context : throw new RomNotInitializedException();
    public static JavaManifest Manifest => IsLoaded ? _manifest : throw new RomNotInitializedException();
    public static ArchiveDefine[] ArchiveDefines => IsLoaded ? _archiveDefines : throw new RomNotInitializedException();
    public static ArchiveHeader[] ArchiveHeaders => IsLoaded ? _archiveHeaders : throw new RomNotInitializedException();

    #endregion

    #region Private Methods

    private static void LoadRom()
    {
        using Context context = Context;

        // Load the Java archive
        using JavaArchive javaArchive = new(Path.Combine(GameDirectory, GameFileName), cache: false);

        _manifest = javaArchive.ReadManifest();
        // TODO: Validate the manifest values (version etc.)

        // Load the resource archive files from the rom
        _archiveHeaders = new ArchiveHeader[ArchiveDefines.Length];
        for (int archiveIndex = 0; archiveIndex < ArchiveDefines.Length; archiveIndex++)
        {
            ArchiveDefine archiveDefine = ArchiveDefines[archiveIndex];

            // Read and add the file
            ZipArchiveEntry archiveEntry = javaArchive.GetFile(archiveDefine.FileName);
            using (Stream archiveStream = archiveEntry.Open())
            {
                byte[] archiveBuffer = new byte[archiveEntry.Length];
                archiveStream.ReadExactly(archiveBuffer);
                context.AddFile(new StreamFile(
                    context: context,
                    name: archiveDefine.FileName,
                    stream: new MemoryStream(archiveBuffer),
                    endianness: Endian.Big,
                    allowLocalPointers: true,
                    mode: VirtualFileMode.Maintain));
            }

            // Read the archive header
            _archiveHeaders[archiveIndex] = FileFactory.Read<ArchiveHeader>(context, archiveDefine.FileName,
                onPreSerialize: (_, obj) => obj.Pre_EntriesCount = archiveDefine.ImageResourcesCount + archiveDefine.DataResourcesCount);
        }
    }

    #endregion

    #region Public Methods

    public static void GetGamePaths(Rayman3J2meVersion version, out string gameDirectory, out string gameFileName, out string logName)
    {
        string versionName = version switch
        {
            Rayman3J2meVersion.Rayman3_1_0_3_SonyEricssonS700_240x320 => "Sony Ericsson S700 - 240x320 (1.0.3)",
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        gameDirectory = Engine.UserData.GetDirectory(Path.Combine("J2me", versionName));
        gameFileName = "rayman3.jar";
        logName = "j2me";
    }

    public static ArchiveDefine[] GetArchiveDefines(Rayman3J2meVersion version)
    {
        return version switch
        {
            Rayman3J2meVersion.Rayman3_1_0_3_SonyEricssonS700_240x320 =>
            [
                new ArchiveDefine("wbw", 0, 49), // Resource_Archive_animation
                new ArchiveDefine("mdg", 21, 0), // Resource_Archive_image
                new ArchiveDefine("04d", 0, 21), // Resource_Archive_map
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };
    }

    public static void Init(Rayman3J2meVersion version)
    {
        if (IsLoaded)
            throw new Exception("The rom is already loaded");

        try
        {
            IsLoaded = true;

            // Get the paths
            GetGamePaths(version, out string gameDirectory, out string gameFileName, out string logName);

            // Set properties
            _gameDirectory = gameDirectory;
            _gameFileName = gameFileName;
            _version = version;

            // Create the serializer settings
            SerializerSettings settings = new()
            {
                // Don't cache on read
                IgnoreCacheOnRead = true,
            };

            // Create a serializer logger
            ISerializerLogger serializerLogger = Engine.Settings.Active.Debug.WriteSerializerLog
                ? new FileSerializerLogger(Engine.UserData.GetFile(Paths.GetSerializeLogFileName(logName)))
                : null;

            // Create the binary context
            _context = new Context(GameDirectory, settings: settings, serializerLogger: serializerLogger, systemLogger: BinarySerializerSystemLogger.Create());

            // Get the archive defines
            _archiveDefines = GetArchiveDefines(Version);

            // Load the rom
            LoadRom();

            // Set the original game resolution
            _originalResolution = version switch
            {
                Rayman3J2meVersion.Rayman3_1_0_3_SonyEricssonS700_240x320 => new Vector2(240, 320),
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
            };
        }
        catch
        {
            IsLoaded = false;
            throw;
        }

    }

    public static void UnInit()
    {
        IsLoaded = false;

        _context?.Dispose();

        _gameDirectory = default;
        _gameFileName = default;
        _version = default;
        _originalResolution = default;
        _context = default;
        _archiveDefines = default;
        _archiveHeaders = default;
    }

    public static ArchiveResource ReadResource(int archiveIndex, int entryIndex, ArchiveResource resource)
    {
        ArchiveHeader header = ArchiveHeaders[archiveIndex];
        resource.Pre_HeaderEntry = header.Entries[entryIndex];
        return (ArchiveResource)FileFactory.Read(Context, resource, header.EntryPointers[entryIndex], name: $"{archiveIndex}_{entryIndex}");
    }

    public static T ReadResource<T>(int archiveIndex, int entryIndex)
        where T : ArchiveResource, new()
    {
        ArchiveHeader header = ArchiveHeaders[archiveIndex];
        return FileFactory.Read<T>(Context, header.EntryPointers[entryIndex], 
            onPreSerialize: (_, obj) => obj.Pre_HeaderEntry = header.Entries[entryIndex], 
            name: $"{archiveIndex}_{entryIndex}");
    }

    #endregion
}