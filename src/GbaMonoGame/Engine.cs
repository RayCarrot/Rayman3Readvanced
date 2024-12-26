using System;
using System.IO;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public static class Engine
{
    #region Paths

    public const string InstalledGamesDirName = "Games";
    public static string ConfigFileName => "Config.json";
    public static string ImgGuiConfigFileName => "imgui.ini";
    public static string SerializerLogFileName => "SerializerLog.txt";

    #endregion

    #region Engine

    public static GameConfig Config { get; private set; }
    public static Version Version => new(0, 0, 0);

    #endregion

    #region Rom

    public static GameInstallation GameInstallation { get; private set; }
    public static Context Context { get; private set; }
    public static Loader Loader { get; private set; }
    public static SaveGame SaveGame { get; private set; }
    public static GbaEngineSettings Settings { get; private set; }

    #endregion

    #region MonoGame

    internal static bool IsLoading { get; set; }

    /// <summary>
    /// The graphics device to use for creating textures.
    /// </summary>
    public static GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// The content manager to load contents.
    /// </summary>
    public static ContentManager ContentManager { get; private set; }

    /// <summary>
    /// The primary render context using the current game resolution.
    /// </summary>
    public static GameRenderContext GameRenderContext { get; private set; }

    /// <summary>
    /// The render context for the original game resolution with the screen aspect ratio.
    /// </summary>
    public static OriginalGameRenderContext OriginalGameRenderContext { get; private set; }

    public static GbaGameViewPort GameViewPort { get; private set; }

    public static GbaGameWindow GameWindow { get; private set; }

    #endregion

    #region Cache

    // TODO: Show cache in debug layout
    public static Cache<Texture2D> TextureCache { get; } = new();
    public static Cache<Palette> PaletteCache { get; } = new();

    #endregion

    #region Private Static Methods

    private static void LoadRom()
    {
        using Context context = Context;

        if (GameInstallation.Platform == Platform.GBA)
        {
            GbaLoader loader = new(context);
            loader.LoadFiles(GameInstallation.GameFilePath, cache: true);
            loader.LoadRomHeader(GameInstallation.GameFilePath);

            string gameCode = loader.RomHeader.GameCode;

            context.AddPreDefinedPointers(Settings.Game switch
            {
                Game.Rayman3 when gameCode is "AYZP" => DefinedPointers.Rayman3_GBA_EU,
                //Game.Rayman3 when gameCode is "AYZE" => DefinedPointers.Rayman3_GBA_US, // TODO: Support US version
                _ => throw new Exception($"Unsupported game {Settings.Game} and/or code {gameCode}")
            });

            loader.LoadData(GameInstallation.GameFilePath);
            Loader = loader;
        }
        else if (GameInstallation.Platform == Platform.NGage)
        {
            string dataFileName = Path.ChangeExtension(GameInstallation.GameFilePath, ".dat");

            NGageLoader loader = new(context);
            loader.LoadFiles(GameInstallation.GameFilePath, dataFileName, cache: true);

            context.AddPreDefinedPointers(Settings.Game switch
            {
                Game.Rayman3 => DefinedPointers.Rayman3_NGage,
                _ => throw new Exception($"Unsupported game {Settings.Game}")
            });

            loader.LoadData(GameInstallation.GameFilePath, dataFileName);
            Loader = loader;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private static void LoadSaveGame()
    {
        string saveFile = GameInstallation.SaveFilePath;
        using Context context = Context;

        PhysicalFile file;
        if (Settings.Platform == Platform.GBA)
        {
            EEPROMEncoder encoder = new(0x200);
            file = context.AddFile(new EncodedLinearFile(context, saveFile, encoder)
            {
                IgnoreCacheOnRead = true
            });
        }
        else
        {
            file = context.AddFile(new LinearFile(context, saveFile)
            {
                IgnoreCacheOnRead = true
            });
        }

        if (file.SourceFileExists)
        {
            // TODO: Try/catch?
            SaveGame = FileFactory.Read<SaveGame>(context, saveFile);
        }
        else
        {
            SaveGame = new SaveGame
            {
                ValidSlots = new bool[3],
                Slots =
                [
                    new SaveGameSlot
                    {
                        Lums = new byte[125],
                        Cages = new byte[7],
                    },
                    new SaveGameSlot
                    {
                        Lums = new byte[125],
                        Cages = new byte[7],
                    },
                    new SaveGameSlot
                    {
                        Lums = new byte[125],
                        Cages = new byte[7],
                    },
                ],
                MusicVolume = (int)SoundEngineInterface.MaxVolume,
                SfxVolume = (int)SoundEngineInterface.MaxVolume,
                Language = 0,
                MultiplayerName = "Rayman", // TODO: How is this set?
            };
        }
    }

    #endregion

    #region Internal Static Methods

    internal static void LoadConfig()
    {
        Config = GameConfig.Load(FileManager.GetDataFile(ConfigFileName));
    }

    internal static void SaveConfig()
    {
        Config.Save(FileManager.GetDataFile(ConfigFileName));
    }

    internal static void LoadGameInstallation(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        ISerializerLogger serializerLogger = Config.WriteSerializerLog
            ? new FileSerializerLogger(FileManager.GetDataFile(SerializerLogFileName))
            : null;

        Context = new Context(String.Empty, serializerLogger: serializerLogger, systemLogger: new BinarySerializerSystemLogger());
        Settings = new GbaEngineSettings { Game = GameInstallation.Game, Platform = GameInstallation.Platform };
        Context.AddSettings(Settings);

        LoadRom();
        LoadSaveGame();
    }

    internal static void LoadMonoGame(GraphicsDevice graphicsDevice, ContentManager contentManager, GbaGameViewPort gameViewPort, GbaGameWindow gameWindow)
    {
        GraphicsDevice = graphicsDevice;
        ContentManager = contentManager;
        GameViewPort = gameViewPort;
        GameWindow = gameWindow;
        GameRenderContext = new GameRenderContext();
        OriginalGameRenderContext = new OriginalGameRenderContext();
    }

    internal static void Unload()
    {
        Context?.Dispose();
    }

    #endregion

    #region Public Static Methods

    public static void BeginLoad()
    {
        IsLoading = true;
    }

    #endregion
}