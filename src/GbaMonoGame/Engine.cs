using System;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public static class Engine
{
    #region Properties

    public static ConfigManager Config { get; private set; }

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

    public static RichPresenceManager RichPresenceManager { get; private set; }

    #endregion

    #region Methods

    public static void BeginLoad()
    {
        IsLoading = true;
    }

    public static void LoadConfig()
    {
        Config = new ConfigManager();

        // If the internal resolution is null then we default to the original resolution
        if (Config.Active.Tweaks.InternalGameResolution == null)
            InternalGameResolution = Rom.IsLoaded ? Rom.OriginalResolution : Resolution.Modern;
        else
            InternalGameResolution = Config.Active.Tweaks.InternalGameResolution.Value;
    }

    public static void UpdateInternalGameResolution()
    {
        if (InternalGameResolution != Config.Active.Tweaks.InternalGameResolution)
            SetInternalGameResolution(Config.Active.Tweaks.InternalGameResolution!.Value);
    }

    public static void SetInternalGameResolution(Vector2 resolution)
    {
        InternalGameResolution = resolution;
        GameViewPort.UpdateRenderBox();
    }

    public static void Init(GbaGame gbaGame, GraphicsDevice graphicsDevice, IServiceProvider serviceProvider, GbaGameWindow gameWindow)
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        GbaGame = gbaGame;
        GraphicsDevice = graphicsDevice;
        FixContentManager = new ContentManager(serviceProvider, Paths.AssetsDirectoryName);
        FrameContentManager = new ContentManager(serviceProvider, Paths.AssetsDirectoryName);
        GameWindow = gameWindow;
        GameViewPort = new GbaGameViewPort();
        GameRenderContext = new GameRenderContext();
        RichPresenceManager = new RichPresenceManager();
        RichPresenceManager.Initialize();

        Gfx.Load();
    }

    public static void UnInit()
    {
        FixContentManager?.Dispose();
        FrameContentManager?.Dispose();
        RichPresenceManager?.Dispose();
    }

    public static void Step()
    {
        FrameManager.Step();
    }

    public static void ExitGame()
    {
        GbaGame.Exit();
    }

    #endregion
}