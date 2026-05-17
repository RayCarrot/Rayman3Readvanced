using System.Text;

namespace GbaMonoGame;

public static class Engine
{
    #region Properties

    // Services
    public static ConfigManager Config { get; private set; }
    public static ApplicationManager App { get; private set; }
    public static GameWindowManager Window { get; private set; }
    public static ViewPortManager ViewPort { get; private set; }
    public static AssetManager Assets { get; private set; }
    public static MessageManager Messages { get; private set; }
    public static RichPresenceManager RichPresence { get; private set; }
    public static FrameManager FrameMngr { get; private set; }

    // TODO: Refactor


    /// <summary>
    /// Disposable resources to dispose when loading a new frame
    /// </summary>
    public static DisposableResources DisposableResources { get; } = new();

    #endregion

    #region Methods

    public static void Init(
        ConfigManager config, 
        ApplicationManager app, 
        GameWindowManager window,
        ViewPortManager viewPort,
        AssetManager assets,
        MessageManager messages,
        RichPresenceManager richPresence,
        FrameManager frameMngr)
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Create services
        Config = config;
        App = app;
        Window = window;
        ViewPort = viewPort;
        Assets = assets;
        Messages = messages;
        RichPresence = richPresence;
        FrameMngr = frameMngr;

        // Initialize services
        if (Config.Active.Tweaks.InternalGameResolution == null)
        {
            // If the internal resolution is null then we default to the original resolution
            ViewPort.SetInternalGameResolution(Rom.IsLoaded ? Rom.OriginalResolution : Resolution.Modern);
        }
        else
        {
            ViewPort.SetInternalGameResolution(Config.Active.Tweaks.InternalGameResolution.Value);
        }
        RichPresence.Initialize();

        // TODO: Refactor
        Gfx.Load();
    }

    public static void UnInit()
    {
        // Uninitialize services
        Assets?.Dispose();
        RichPresence?.Dispose();

        // Remove services
        Config = null;
        App = null;
        Window = null;
        ViewPort = null;
        Assets = null;
        Messages = null;
        RichPresence = null;
        FrameMngr = null;
    }

    public static void Step()
    {
        FrameMngr.Step();
    }

    #endregion
}