using System.Text;

namespace GbaMonoGame;

public static class Engine
{
    // Engine services
    public static IConfigManager Config { get; private set; }
    public static IApplicationManager App { get; private set; }
    public static InputManager Input { get; private set; }
    public static IGameWindowManager Window { get; private set; }
    public static ViewPortManager ViewPort { get; private set; }
    public static AssetManager Assets { get; private set; }
    public static MessageManager Messages { get; private set; }
    public static IRichPresenceManager RichPresence { get; private set; }
    public static FrameManager FrameMngr { get; private set; }

    // Game services
    public static SoundEventsManager Sem { get; private set; }
    public static FontManager Font { get; private set; }

    public static void InitEngine(
        IConfigManager config,
        IApplicationManager app, 
        InputManager input,
        IGameWindowManager window,
        ViewPortManager viewPort,
        AssetManager assets,
        MessageManager messages,
        IRichPresenceManager richPresence,
        FrameManager frameMngr)
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Set services
        Config = config;
        App = app;
        Input = input;
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
    }

    public static void InitGame(
        SoundEventsManager sem,
        FontManager font)
    {
        // Set services
        Sem = sem;
        Font = font;
    }

    public static void UnInitEngine()
    {
        // Uninitialize services
        Assets?.Dispose();
        RichPresence?.Dispose();
        FrameMngr?.Dispose();

        // Remove services
        Config = null;
        App = null;
        Input = null;
        Window = null;
        ViewPort = null;
        Assets = null;
        Messages = null;
        RichPresence = null;
        FrameMngr = null;
    }

    public static void UnInitGame()
    {
        // Uninitialize services
        Sem?.Dispose();
        Font?.Dispose();

        // Remove services
        Sem = null;
        Font = null;
    }

    public static void Step()
    {
        FrameMngr.Step();
    }
}