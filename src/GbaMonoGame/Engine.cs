using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

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

    /// <summary>
    /// The full, local, game config. Avoid using this to read the config as the <see cref="ActiveConfig"/>
    /// may be overriden and temporarily contain a different config.
    /// </summary>
    public static LocalGameConfig LocalConfig { get; private set; }

    /// <summary>
    /// The currently active game config. This is either the same as <see cref="LocalConfig"/> or a
    /// temporarily overriden config.
    /// </summary>
    public static ActiveGameConfig ActiveConfig { get; private set; }

    /// <summary>
    /// Indicates if the game config has been overriden.
    /// </summary>
    public static bool IsConfigOverrided { get; private set; }

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

    public static Cache<Texture2D> TextureCache { get; } = new();
    public static Cache<Palette> PaletteCache { get; } = new();

    /// <summary>
    /// Disposable resources to dispose when loading a new frame
    /// </summary>
    public static DisposableResources DisposableResources { get; } = new();

    public static MessageManager MessageManager { get; } = new();

    #endregion

    #region Methods

    public static void BeginLoad()
    {
        IsLoading = true;
    }

    public static void LoadConfig()
    {
        string filePath = FileManager.GetDataFile(ConfigFileName);
        LocalGameConfig config = new();

        try
        {
            config.Serialize(new IniDeserializer(filePath));
        }
        catch (Exception ex)
        {
            // Recreate and serialize without a file source to reset to default values
            config = new LocalGameConfig();
            config.Serialize(new IniDeserializer(null));

            MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: $"An error occurred when reading the saved game options.{Environment.NewLine}All options will be reset to their default values.", 
                header: "Error reading game options");
        }

        LocalConfig = config;

        ActiveConfig = new ActiveGameConfig(LocalConfig.Tweaks, LocalConfig.Difficulty, LocalConfig.Debug);
        IsConfigOverrided = false;

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
        LocalConfig.Serialize(serializer);

        try
        {
            serializer.Save(filePath);
        }
        catch (Exception ex)
        {
            MessageManager.EnqueueExceptionMessage(
                ex: ex, 
                text: "An error occurred when saving the game options.", 
                header: "Error reading game options");
        }
    }

    public static void OverrideActiveConfig(ActiveGameConfig activeGameConfig)
    {
        ActiveConfig = activeGameConfig;
        IsConfigOverrided = true;
        UpdateInternalGameResolution();
    }

    public static void RestoreActiveConfig()
    {
        ActiveConfig = new ActiveGameConfig(LocalConfig.Tweaks, LocalConfig.Difficulty, LocalConfig.Debug);
        IsConfigOverrided = false;
        UpdateInternalGameResolution();
    }

    public static void UpdateInternalGameResolution()
    {
        if (InternalGameResolution != ActiveConfig.Tweaks.InternalGameResolution)
            SetInternalGameResolution(ActiveConfig.Tweaks.InternalGameResolution!.Value);
    }

    public static void SetInternalGameResolution(Vector2 resolution)
    {
        InternalGameResolution = resolution;
        GameViewPort.UpdateRenderBox();
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