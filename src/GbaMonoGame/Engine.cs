using System.Text;

namespace GbaMonoGame;

public static class Engine
{
    #region Properties

    // Services
    public static ConfigManager Config { get; private set; }
    public static ApplicationManager App { get; private set; }
    public static GameWindowManager Window { get; private set; }
    public static AssetManager Assets { get; private set; }

    // State
    public static bool IsLoading { get; set; }



    /// <summary>
    /// The internal game resolution used for the aspect ratio and scaling.
    /// </summary>
    public static Vector2 InternalGameResolution { get; set; }

    /// <summary>
    /// The primary render context using the internal game resolution.
    /// </summary>
    public static GameRenderContext GameRenderContext { get; private set; }

    public static GbaGameViewPort GameViewPort { get; private set; }


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

    public static void Init(
        ConfigManager config, 
        ApplicationManager app, 
        GameWindowManager window, 
        AssetManager assets)
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Create services
        Config = config;
        App = app;
        Window = window;
        Assets = assets;

        // Initialize services
        if (Config.Active.Tweaks.InternalGameResolution == null)
        {
            // If the internal resolution is null then we default to the original resolution
            InternalGameResolution = Rom.IsLoaded ? Rom.OriginalResolution : Resolution.Modern;
        }
        else
        {
            InternalGameResolution = Config.Active.Tweaks.InternalGameResolution.Value;
        }

        // TODO: Refactor
        GameViewPort = new GbaGameViewPort();
        GameRenderContext = new GameRenderContext();
        RichPresenceManager = new RichPresenceManager();
        RichPresenceManager.Initialize();
        Gfx.Load();
    }

    public static void UnInit()
    {
        // Uninitialize services
        Assets?.Dispose();
        RichPresenceManager?.Dispose();

        // Remove services
        Config = null;
        App = null;
        Window = null;
        Assets = null;
    }

    public static void Step()
    {
        FrameManager.Step();
    }

    #endregion
}