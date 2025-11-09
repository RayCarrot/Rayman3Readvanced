using System;
using System.Diagnostics;
using System.IO;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public static class Rom
{
    #region Fields

    private static string _gameDirectory;
    private static string[] _gameFileNames;
    private static Game _game;
    private static Platform _platform;
    private static Region _region;
    private static Vector2 _originalResolution;
    private static RenderContext _originalGameRenderContext;
    private static RenderContext _originalScaledGameRenderContext;
    private static Context _context;
    private static Loader _loader;

    #endregion

    #region Properties

    public static bool IsLoaded { get; private set; }

    public static string GameDirectory => IsLoaded ? _gameDirectory : throw new RomNotInitializedException();
    public static string[] GameFileNames => IsLoaded ? _gameFileNames : throw new RomNotInitializedException();
    public static Game Game => IsLoaded ? _game : throw new RomNotInitializedException();
    public static Platform Platform => IsLoaded ? _platform : throw new RomNotInitializedException();
    public static Region Region => IsLoaded ? _region : throw new RomNotInitializedException();

    public static Vector2 OriginalResolution => IsLoaded ? _originalResolution : throw new RomNotInitializedException();
    public static RenderContext OriginalGameRenderContext => IsLoaded ? _originalGameRenderContext : throw new RomNotInitializedException();
    public static RenderContext OriginalScaledGameRenderContext => IsLoaded ? _originalScaledGameRenderContext : throw new RomNotInitializedException();

    public static Context Context => IsLoaded ? _context : throw new RomNotInitializedException();
    public static Loader Loader => IsLoaded ? _loader : throw new RomNotInitializedException();

    #endregion

    #region Events

    public static event EventHandler Loaded;
    public static event EventHandler Unloaded;

    #endregion

    #region Private Methods

    private static void LoadRom()
    {
        using Context context = Context;
        GbaEngineSettings settings = context.GetRequiredSettings<GbaEngineSettings>();

        if (Platform == Platform.GBA)
        {
            string romFileName = GameFileNames[0];

            GbaLoader loader = new(context);
            loader.LoadFiles(romFileName, cache: true);
            loader.LoadRomHeader(romFileName);

            string gameCode = loader.RomHeader.GameCode;

            context.AddPreDefinedPointers(Game switch
            {
                Game.Rayman3 when gameCode is "AYZP" => DefinedPointers.Rayman3_GBA_EU, // Rayman 3 (Europe)
                Game.Rayman3 when gameCode is "AYZE" => DefinedPointers.Rayman3_GBA_US, // Rayman 3 (USA)
                Game.Rayman3 when gameCode is "BX5P" => DefinedPointers.Rayman3_GBA_10thAnniversary_EU, // Rayman 10th Anniversary (Europe)
                Game.Rayman3 when gameCode is "BX5E" => DefinedPointers.Rayman3_GBA_10thAnniversary_US, // Rayman 10th Anniversary (USA)
                Game.Rayman3 when gameCode is "BWZP" => DefinedPointers.Rayman3_GBA_WinnieThePoohPack_EU, // Winnie the Pooh's Rumbly Tumbly Adventure & Rayman 3 (Europe)
                _ => throw new Exception($"Unsupported game {Game} and/or code {gameCode}")
            });

            settings.SetDefinedResources(DefinedResources.Rayman3_GBA);

            Stopwatch sw = Stopwatch.StartNew();
            loader.LoadData(romFileName);
            sw.Stop();

            Logger.Info("Loaded ROM data in {0} ms", sw.ElapsedMilliseconds);

            _region = gameCode[3] switch
            {
                'P' => Region.Europe,
                'E' => Region.Usa,
                _ => throw new Exception($"Unsupported game code {gameCode}")
            };
            _loader = loader;
        }
        else if (Platform == Platform.NGage)
        {
            string appFileName = GameFileNames[0];
            string dataFileName = GameFileNames[1];

            NGageLoader loader = new(context);
            loader.LoadFiles(appFileName, dataFileName, cache: true);

            context.AddPreDefinedPointers(Game switch
            {
                Game.Rayman3 => DefinedPointers.Rayman3_NGage,
                _ => throw new Exception($"Unsupported game {Game}")
            });

            settings.SetDefinedResources(DefinedResources.Rayman3_NGage);

            loader.LoadData(appFileName, dataFileName);

            _region = Region.Europe;
            _loader = loader;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    #endregion

    #region Public Methods

    public static void Init(string gameDirectory, string[] gameFileNames, Game game, Platform platform)
    {
        if (IsLoaded)
            throw new Exception("The rom is already loaded");

        try
        {
            IsLoaded = true;

            // Set properties
            _gameDirectory = gameDirectory;
            _gameFileNames = gameFileNames;
            _game = game;
            _platform = platform;

            // Create a serializer logger
            ISerializerLogger serializerLogger = Engine.ActiveConfig.Debug.WriteSerializerLog
                ? new FileSerializerLogger(FileManager.GetDataFile(Engine.SerializerLogFileName))
                : null;

            // Create the binary context
            _context = new Context(GameDirectory, serializerLogger: serializerLogger, systemLogger: new BinarySerializerSystemLogger());

            // Create and add the game settings
            GbaEngineSettings settings = new() { Game = Game, Platform = Platform };
            Context.AddSettings(settings);

            // Load the rom
            LoadRom();

            // Set the original game resolution
            _originalResolution = Platform switch
            {
                Platform.GBA => Resolution.Gba,
                Platform.NGage => Resolution.NGage,
                _ => throw new UnsupportedPlatformException(),
            };

            // Set the internal resolution if it's null
            if (Engine.ActiveConfig.Tweaks.InternalGameResolution == null)
                Engine.InternalGameResolution = OriginalResolution;

            _originalGameRenderContext = new FixedResolutionRenderContext(OriginalResolution);
            _originalScaledGameRenderContext = new OriginalScaledGameRenderContext();

            Loaded?.Invoke(null, EventArgs.Empty);
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
        _gameFileNames = default;
        _game = default;
        _region = default;
        _platform = default;
        _originalResolution = default;
        _originalGameRenderContext = default;
        _originalScaledGameRenderContext = default;
        _context = default;
        _loader = default;

        Unloaded?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Loads a resource from the given index. If the context has caching enabled then the resource
    /// will be cached after loading, resulting in future loads returning the same data.
    /// </summary>
    /// <typeparam name="T">The resource type</typeparam>
    /// <param name="index">The resource index</param>
    /// <returns>The loaded resource</returns>
    public static T LoadResource<T>(int index)
        where T : Resource, new()
    {
        using Context context = Context;
        return Loader.GameOffsetTable.ReadResource<T>(Context, index, name: $"Resource_{index}");
    }

    public static T LoadResource<T>(Rayman3DefinedResource definedResource)
        where T : Resource, new()
    {
        using Context context = Context;
        return Loader.GameOffsetTable.ReadResource<T>(Context, definedResource, name: definedResource.ToString());
    }

    public static Stream LoadResourceStream(int index)
    {
        RawResource res = LoadResource<RawResource>(index);
        return new MemoryStream(res.RawData);
    }

    public static Stream LoadResourceStream(Rayman3DefinedResource definedResource)
    {
        RawResource res = LoadResource<RawResource>(definedResource);
        return new MemoryStream(res.RawData);
    }

    public static T CopyResource<T>(T resource)
        where T : BinarySerializable, new()
    {
        using Context context = Context;
        
        SerializerSettings settings = (SerializerSettings)context.Settings;
        bool ignoreCacheOnRead = settings.IgnoreCacheOnRead;
        settings.IgnoreCacheOnRead = true;

        T obj = FileFactory.Read<T>(context, resource.Offset);

        settings.IgnoreCacheOnRead = ignoreCacheOnRead;

        return obj;
    }

    #endregion
}