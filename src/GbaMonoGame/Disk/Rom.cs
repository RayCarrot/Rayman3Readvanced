using System;
using System.IO;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

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
    private static Rayman3Loader _loader;

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
    public static Rayman3Loader Loader => IsLoaded ? _loader : throw new RomNotInitializedException();

    #endregion

    #region Events

    public static event EventHandler Loaded;
    public static event EventHandler Unloaded;

    #endregion

    #region Private Methods

    private static void LoadRom()
    {
        using Context context = Context;

        if (Platform == Platform.GBA)
        {
            string romFileName = GameFileNames[0];

            Rayman3Loader loader = new(context);
            loader.LoadRom(romFileName, cache: true);
            loader.DefinePointers();
            loader.DefineResources();
            loader.LoadResourceTable();

            string gameCode = loader.RomHeader.GameCode;
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

            Rayman3Loader loader = new(context);
            loader.LoadNGageRom(appFileName, dataFileName, cache: true);
            loader.DefinePointers();
            loader.DefineResources();
            loader.LoadResourceTable();

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

    public static bool ValidateGbaRom(string filePath, Game game)
    {
        string dir = Path.GetDirectoryName(filePath) ?? String.Empty;
        string fileName = Path.GetFileName(filePath);

        using Context context = new(dir, systemLogger: BinarySerializerSystemLogger.Create());
        context.AddFile(new MemoryMappedFile(context, fileName, Constants.Address_ROM));
        ROMHeader header = FileFactory.Read<ROMHeader>(context, fileName);

        return DefinedPointers.GetPointers(header, Platform.GBA, game, false) != null;
    }

    public static void GetGamePaths(Game game, Platform platform, out string gameDirectory, out string[] gameFileNames)
    {
        string gameName = game switch
        {
            Game.Rayman3 => "rayman3",
            _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
        };

        if (platform == Platform.GBA)
        {
            gameDirectory = Engine.UserData.GetDirectory("Gba");
            gameFileNames =
            [
                $"{gameName}.gba"
            ];
        }
        else if (platform == Platform.NGage)
        {
            gameDirectory = Engine.UserData.GetDirectory("NGage");
            gameFileNames =
            [
                $"{gameName}.app",
                $"{gameName}.dat",
            ];
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    public static void Init(Game game, Platform platform)
    {
        if (IsLoaded)
            throw new Exception("The rom is already loaded");

        try
        {
            IsLoaded = true;

            // Get the paths
            GetGamePaths(game, platform, out string gameDirectory, out string[] gameFileNames);
            
            // Set properties
            _gameDirectory = gameDirectory;
            _gameFileNames = gameFileNames;
            _game = game;
            _platform = platform;

            // Create a serializer logger
            ISerializerLogger serializerLogger = Engine.Settings.Active.Debug.WriteSerializerLog
                ? new FileSerializerLogger(Engine.UserData.GetFile(Paths.SerializerLogFileName))
                : null;

            // Create the binary context
            _context = new Context(GameDirectory, serializerLogger: serializerLogger, systemLogger: BinarySerializerSystemLogger.Create());

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
            if (Engine.Settings.Active.Tweaks.InternalGameResolution == null)
                Engine.ViewPort.SetInternalGameResolution(OriginalResolution);

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