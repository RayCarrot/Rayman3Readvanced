using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

// TODO: Handle exceptions for loading/saving config
public static class Engine
{
    #region Paths

    public static string DataDirectoryName => "Data";
    public static string AssetsDirectoryName => "Assets";
    public static string CrashlogFileName => "Crashlog.txt";
    public static string ConfigFileName => "Config.ini";
    public static string ImgGuiConfigFileName => "imgui.ini";
    public static string SerializerLogFileName => "SerializerLog.txt";

    #endregion

    #region Properties

    public static GameConfig Config { get; private set; }

    public static bool IsLoading { get; set; }

    /// <summary>
    /// The game instance.
    /// </summary>
    public static GbaGame GbaGame { get; private set; }

    /// <summary>
    /// The graphics device to use for creating textures.
    /// </summary>
    public static GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// The fixed content manager to load contents which should stay loaded through the entire lifecycle of the game.
    /// </summary>
    public static ContentManager FixContentManager { get; private set; }

    /// <summary>
    /// The frame content manager to load contents which should be unloaded when changing the current <see cref="Frame"/>.
    /// </summary>
    public static ContentManager FrameContentManager { get; private set; }

    /// <summary>
    /// The internal game resolution used for the aspect ratio and scaling.
    /// </summary>
    public static Vector2 InternalGameResolution { get; set; }

    /// <summary>
    /// The primary render context using the internal game resolution.
    /// </summary>
    public static GameRenderContext GameRenderContext { get; private set; }

    public static GbaGameViewPort GameViewPort { get; private set; }

    public static GbaGameWindow GameWindow { get; private set; }

    // TODO: Show cache in debug layout
    public static Cache<Texture2D> TextureCache { get; } = new();
    public static Cache<Palette> PaletteCache { get; } = new();

    /// <summary>
    /// Disposable resources to dispose when loading a new frame
    /// </summary>
    public static DisposableResources DisposableResources { get; } = new();

    #endregion

    #region Methods

    public static void BeginLoad()
    {
        IsLoading = true;
    }

    public static void LoadConfig()
    {
        string filePath = FileManager.GetDataFile(ConfigFileName);
        GameConfig config = new();
        config.Serialize(new IniDeserializer(filePath));
        Config = config;

        // If the internal resolution is null then we default to the original resolution
        if (config.Tweaks.InternalGameResolution == null)
            InternalGameResolution = Rom.IsLoaded ? Rom.OriginalResolution : Resolution.Modern;
        else
            InternalGameResolution = config.Tweaks.InternalGameResolution.Value;
    }

    public static void SaveConfig()
    {
        string filePath = FileManager.GetDataFile(ConfigFileName);
        IniSerializer serializer = new();
        Config.Serialize(serializer);
        serializer.Save(filePath);
    }

    public static void Init(GbaGame gbaGame, GbaGameWindow gameWindow, Frame initialFrame)
    {
        GbaGame = gbaGame;
        GraphicsDevice = gbaGame.GraphicsDevice;
        FixContentManager = new ContentManager(gbaGame.Services, AssetsDirectoryName);
        FrameContentManager = new ContentManager(gbaGame.Services, AssetsDirectoryName);
        GameWindow = gameWindow;
        GameViewPort = new GbaGameViewPort();
        GameRenderContext = new GameRenderContext();

        Gfx.Load();

        FrameManager.SetNextFrame(initialFrame);
    }

    public static void UnInit()
    {
        FixContentManager.Dispose();
        FrameContentManager.Dispose();
    }

    public static void Step()
    {
        FrameManager.Step();
    }

    #endregion
}