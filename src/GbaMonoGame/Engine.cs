using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public static class Engine
{
    #region Paths

    public static string AssetsDirectoryName => "Content";
    public static string ConfigFileName => "Config.ini";
    public static string ImgGuiConfigFileName => "imgui.ini";
    public static string SerializerLogFileName => "SerializerLog.txt";

    #endregion

    #region Properties

    public static GameConfig Config { get; private set; }
    public static Version Version => new(0, 0, 0);

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
    /// The content manager to load contents.
    /// </summary>
    public static ContentManager ContentManager { get; private set; }

    /// <summary>
    /// The primary render context using the current game resolution.
    /// </summary>
    public static GameRenderContext GameRenderContext { get; private set; }

    public static GbaGameViewPort GameViewPort { get; private set; }

    public static GbaGameWindow GameWindow { get; private set; }

    // TODO: Show cache in debug layout
    public static Cache<Texture2D> TextureCache { get; } = new();
    public static Cache<Palette> PaletteCache { get; } = new();

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
        ContentManager = gbaGame.Content;
        GameWindow = gameWindow;
        GameViewPort = new GbaGameViewPort(Config.InternalGameResolution.ToVector2());
        GameRenderContext = new GameRenderContext();

        Gfx.Load();

        FrameManager.SetNextFrame(initialFrame);
    }

    public static void Step()
    {
        FrameManager.Step();
    }

    #endregion
}