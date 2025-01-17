using System;
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

    public static Vector2 OriginalResolution => IsLoaded ? _originalResolution : throw new RomNotInitializedException();
    public static RenderContext OriginalGameRenderContext => IsLoaded ? _originalGameRenderContext : throw new RomNotInitializedException();
    public static RenderContext OriginalScaledGameRenderContext => IsLoaded ? _originalScaledGameRenderContext : throw new RomNotInitializedException();

    public static Context Context => IsLoaded ? _context : throw new RomNotInitializedException();
    public static Loader Loader => IsLoaded ? _loader : throw new RomNotInitializedException();

    #endregion

    #region Events

    public static event EventHandler Loaded;

    #endregion

    #region Private Methods

    private static void LoadRom()
    {
        using Context context = Context;

        if (Platform == Platform.GBA)
        {
            string romFileName = GameFileNames[0];

            GbaLoader loader = new(context);
            loader.LoadFiles(romFileName, cache: true);
            loader.LoadRomHeader(romFileName);

            string gameCode = loader.RomHeader.GameCode;

            context.AddPreDefinedPointers(Game switch
            {
                Game.Rayman3 when gameCode is "AYZP" => DefinedPointers.Rayman3_GBA_EU,
                //Game.Rayman3 when gameCode is "AYZE" => DefinedPointers.Rayman3_GBA_US, // TODO: Support US version
                _ => throw new Exception($"Unsupported game {Game} and/or code {gameCode}")
            });

            loader.LoadData(romFileName);
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

            loader.LoadData(appFileName, dataFileName);
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
            ISerializerLogger serializerLogger = Engine.Config.WriteSerializerLog
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
                Platform.GBA => new Vector2(240, 160),
                Platform.NGage => new Vector2(176, 208),
                _ => throw new UnsupportedPlatformException(),
            };

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

    public static T LoadResource<T>(GameResource gameResource)
        where T : Resource, new()
    {
        using Context context = Context;
        return Loader.GameOffsetTable.ReadResource<T>(Context, gameResource, name: gameResource.ToString());
    }

    public static Stream LoadResourceStream(int index)
    {
        RawResource res = LoadResource<RawResource>(index);
        return new MemoryStream(res.RawData);
    }

    public static Stream LoadResourceStream(GameResource gameResource)
    {
        RawResource res = LoadResource<RawResource>(gameResource);
        return new MemoryStream(res.RawData);
    }

    #endregion
}