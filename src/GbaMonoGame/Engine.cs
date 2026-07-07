using System.Text;

namespace GbaMonoGame;

public static class Engine
{
    // Engine services
    public static ISettingsManager Settings { get; private set; }
    public static IApplicationManager App { get; private set; }
    public static ISoundManager Sound { get; private set; }
    public static InputManager Input { get; private set; }
    public static BufferedJoyPad JoyPad { get; private set; }
    public static MultiJoyPad MultiJoyPad { get; private set; }
    public static IGameWindowManager Window { get; private set; }
    public static ViewPortManager ViewPort { get; private set; }
    public static AssetManager Assets { get; private set; }
    public static UserDataManager UserData { get; private set; }
    public static GameConfigManager Config { get; private set; }
    public static MessageManager Messages { get; private set; }
    public static FileDialogManager FileDialog { get; private set; }
    public static IRichPresenceManager RichPresence { get; private set; }
    public static FrameManager FrameMngr { get; private set; }

    // Game services
    public static SoundEngineInterface SoundEngineInterface { get; private set; }
    public static SoundEventsManager Sem { get; private set; }
    public static MultiplayerManager Multiplayer { get; private set; }
    public static FontManager Font { get; private set; }

    public static void InitEngine(
        ISettingsManager settings,
        IApplicationManager app, 
        ISoundManager sound,
        InputManager input,
        BufferedJoyPad joyPad,
        MultiJoyPad multiJoyPad,
        IGameWindowManager window,
        ViewPortManager viewPort,
        AssetManager assets,
        UserDataManager userData,
        GameConfigManager config,
        MessageManager messages,
        FileDialogManager fileDialog,
        IRichPresenceManager richPresence,
        FrameManager frameMngr)
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Set services
        Settings = settings;
        Sound = sound;
        App = app;
        Input = input;
        JoyPad = joyPad;
        MultiJoyPad = multiJoyPad;
        Window = window;
        ViewPort = viewPort;
        Assets = assets;
        UserData = userData;
        Config = config;
        Messages = messages;
        FileDialog = fileDialog;
        RichPresence = richPresence;
        FrameMngr = frameMngr;

        // Initialize services
        Settings.Load();
        Sound.Init();
        if (Settings.Active.Tweaks.InternalGameResolution == null)
        {
            // If the internal resolution is null then we default to the original resolution
            ViewPort.SetInternalGameResolution(Rom.IsLoaded ? Rom.OriginalResolution : Resolution.Modern);
        }
        else
        {
            ViewPort.SetInternalGameResolution(Settings.Active.Tweaks.InternalGameResolution.Value);
        }
    }

    public static void InitGame(
        SoundEngineInterface soundEngineInterface,
        SoundEventsManager sem,
        MultiplayerManager multiplayer,
        FontManager font)
    {
        // Set services
        SoundEngineInterface = soundEngineInterface;
        Sem = sem;
        Multiplayer = multiplayer;
        Font = font;
    }

    public static void UnInitEngine()
    {
        // Uninitialize services
        FrameMngr?.DisposeResources(); // NOTE: This needs to happen before disposing the sound manager
        Sound?.Dispose();
        Assets?.Dispose();
        RichPresence?.Dispose();
        FrameMngr?.Dispose();

        // Remove services
        Settings = null;
        App = null;
        Sound = null;
        Input = null;
        JoyPad = null;
        MultiJoyPad = null;
        Window = null;
        ViewPort = null;
        Assets = null;
        UserData = null;
        Config = null;
        Messages = null;
        FileDialog = null;
        RichPresence = null;
        FrameMngr = null;
    }

    public static void UnInitGame()
    {
        // Uninitialize services
        Sem?.Dispose();
        Font?.Dispose();

        // Remove services
        SoundEngineInterface = null;
        Sem = null;
        Multiplayer = null;
        Font = null;
    }

    public static void Step()
    {
        FrameMngr.Step();
    }
}