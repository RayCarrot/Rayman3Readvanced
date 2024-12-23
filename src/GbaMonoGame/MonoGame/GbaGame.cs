using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Game = BinarySerializer.Ubisoft.GbaEngine.Game;

namespace GbaMonoGame;

// TODO: Create crash screen in case of exception, during update or load
public abstract class GbaGame : Microsoft.Xna.Framework.Game
{
    #region Constructor

    protected GbaGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _updateTimeStopWatch = new Stopwatch();
        _gameInstallations = new List<GameInstallation>();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        SetFramerate(Framerate);
    }

    #endregion

    #region Private Fields

    // The GBA framerate is actually 59.727500569606, but we do 60
    private const float Framerate = 60;

    private readonly GraphicsDeviceManager _graphics;
    private readonly Stopwatch _updateTimeStopWatch;
    private readonly List<GameInstallation> _gameInstallations;

    private SpriteBatch _spriteBatch;
    private Texture2D _gbaIcon;
    private Texture2D _nGageIcon;
    private SpriteFont _font;
    private Effect _paletteShader;
    private GfxRenderer _gfxRenderer;
    private MenuManager _menu;
    private DebugLayout _debugLayout;
    private GameRenderTarget _debugGameRenderTarget;
    private PerformanceDebugWindow _performanceWindow;
    private int _selectedGameInstallationIndex;
    private GameInstallation _selectedGameInstallation;
    private int _skippedDraws = -1;
    private float _fps = 60;
    private bool _showMenu;
    private bool _isChangingResolution;
    private Task _loadingGameInstallationTask;

    #endregion

    #region Protected Properties

    protected abstract Game Game { get; }
    protected abstract string Title { get; }
    protected abstract Dictionary<int, string> GbaSongTable { get; }
    protected abstract Dictionary<int, string> NGageSongTable { get; }

    #endregion

    #region Public Properties

    public abstract Dictionary<SoundType, string> SampleSongs { get; }
    
    public abstract bool CanSkipCutscene { get; }

    public bool HasLoadedGameInstallation => _selectedGameInstallation != null && _loadingGameInstallationTask == null;
    public bool RunSingleFrame { get; set; }
    public bool IsPaused { get; set; }
    public bool DebugMode { get; set; }

    #endregion

    #region Event Handlers

    private void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        if (!DebugMode)
            SizeGameToWindow();
    }

    private void Menu_Closed(object sender, EventArgs e)
    {
        _showMenu = false;
        Resume();
    }

    private void GbaGame_Exiting(object sender, EventArgs e)
    {
        if (!HasLoadedGameInstallation)
            return;

        // Save window state
        SaveWindowState();

        // Save config
        Engine.SaveConfig();

        // Unload the engine
        Engine.Unload();
    }

    #endregion

    #region Private Methods

    private void SetFramerate(float fps)
    {
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1 / fps);
    }

    private void SetResolution(Point size, bool isFullscreen)
    {
        // A bit convoluted code for setting if it's fullscreen or not. But this seems to reduce flickering.

        _isChangingResolution = true;

        if (!isFullscreen && _graphics.IsFullScreen)
            _graphics.IsFullScreen = false;

        _graphics.PreferredBackBufferWidth = size.X;
        _graphics.PreferredBackBufferHeight = size.Y;
        _graphics.ApplyChanges();

        if (isFullscreen && !_graphics.IsFullScreen)
        {
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        if (HasLoadedGameInstallation)
            Engine.GameViewPort.Resize(size.ToVector2());

        _isChangingResolution = false;
    }

    private Point GetResolution()
    {
        return new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
    }

    private void SizeGameToWindow()
    {
        if (!HasLoadedGameInstallation)
            return;

        Engine.GameViewPort.Resize(
            newScreenSize: GetResolution().ToVector2(), 
            maintainScreenRatio: InputManager.IsButtonPressed(Keys.LeftShift) && !_isChangingResolution, 
            changeScreenSizeCallback: x => SetResolution(x.ToRoundedPoint(), false));
        SaveWindowState();
        Engine.SaveConfig();
    }

    private void LoadEngine(GameInstallation gameInstallation)
    {
        if (_loadingGameInstallationTask == null)
        {
            _selectedGameInstallation = gameInstallation;

            // TODO: If this throws then the exception might get swallowed
            // Load the selected game installation
            _loadingGameInstallationTask = Task.Run(() => Engine.LoadGameInstallation(_selectedGameInstallation));
        }
        else if (_loadingGameInstallationTask.IsCompleted)
        {
            _loadingGameInstallationTask = null;

            // Load the MonoGame part of the engine
            GameViewPort gameViewPort = new(Engine.Settings);
            gameViewPort.SetRequestedResolution(Engine.Config.InternalGameResolution?.ToVector2());
            Engine.LoadMonoGame(GraphicsDevice, Content, gameViewPort);
            Gfx.Load(_paletteShader);

            // Load engine sounds and fonts
            SoundEventsManager.Load(Engine.Settings.Platform switch
            {
                Platform.GBA => new GbaSoundEventsManager(GbaSongTable, Engine.Loader.SoundBank),
                Platform.NGage => new NGageSoundEventsManager(NGageSongTable, Engine.Loader.NGage_SoundEvents),
                _ => throw new UnsupportedPlatformException()
            });
            FontManager.Load(Engine.Loader.Font8, Engine.Loader.Font16, Engine.Loader.Font32);

            // Load window
            ApplyDisplayConfig();

            // Load the initial engine frame
            FrameManager.SetNextFrame(CreateInitialFrame());

            // Load the game
            LoadGame();

            // Load the renderer
            _gfxRenderer = new GfxRenderer(_spriteBatch, Engine.GameViewPort);
            _debugGameRenderTarget = new GameRenderTarget(GraphicsDevice, Engine.GameViewPort);

            // Load the menu
            _menu = new MenuManager();
            _menu.Closed += Menu_Closed;

            // Load the debug layout
            _debugLayout = new DebugLayout();
            _debugLayout.AddWindow(new GameDebugWindow(_debugGameRenderTarget));
            _debugLayout.AddWindow(_performanceWindow = new PerformanceDebugWindow());
            _debugLayout.AddWindow(new LoggerDebugWindow());
            _debugLayout.AddWindow(new GfxDebugWindow());
            _debugLayout.AddWindow(new SoundDebugWindow());
            _debugLayout.AddWindow(new MultiplayerDebugWindow());
            _debugLayout.AddMenu(new WindowsDebugMenu());
            AddDebugWindowsAndMenus(_debugLayout);
            _debugLayout.LoadContent(this);

            // Check launch arguments
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                // Check if it's a BizHawk TAS file
                if (arg.EndsWith(".bk2") && File.Exists(arg))
                    LoadBizHawkTas(arg);
            }
        }
    }

    public void LoadBizHawkTas(string filePath)
    {
        using FileStream fileStream = File.OpenRead(filePath);
        using ZipArchive bk2 = new(fileStream, ZipArchiveMode.Read);

        ZipArchiveEntry entry = bk2.GetEntry("Input Log.txt");

        if (entry == null)
            return;

        using Stream logFileStream = entry.Open();
        using StreamReader reader = new(logFileStream);

        List<GbaInput> inputs = new();
        const string prefix = "|    0,    0,    0,    0,";
        GbaInput[] inputSeq =
        [
            GbaInput.Up,
            GbaInput.Down,
            GbaInput.Left,
            GbaInput.Right,
            GbaInput.Start,
            GbaInput.Select,
            GbaInput.B,
            GbaInput.A,
            GbaInput.L,
            GbaInput.R
        ];

        while (reader.ReadLine() is { } line)
        {
            GbaInput input = GbaInput.Valid;

            if (line.StartsWith(prefix))
            {
                line = line[prefix.Length..];

                for (int i = 0; i < inputSeq.Length; i++)
                {
                    if (line[i] != '.')
                        input |= inputSeq[i];
                }
            }

            inputs.Add(input);
        }

        inputs.Add(GbaInput.None);

        JoyPad.SetReplayData(inputs.ToArray());
    }

    private void StepEngine()
    {
        if (IsPaused)
            return;

        try
        {
            if (DebugMode)
                _updateTimeStopWatch.Restart();

            FrameManager.Step();

            // If this frame did a load, and thus might have taken longer than 1/60th of a second, then
            // we disable fixed time step to avoid MonoGame repeatedly calling Update() to make up for
            // the lost time, and thus drop frames
            if (Engine.IsLoading)
                IsFixedTimeStep = false;

            if (DebugMode)
                _updateTimeStopWatch.Stop();
        }
        catch
        {
            Engine.Unload();
            throw;
        }
    }

    #endregion

    #region Protected Methods

    protected abstract Frame CreateInitialFrame();
    protected virtual void LoadGame() { }
    protected virtual void AddDebugWindowsAndMenus(DebugLayout debugLayout) { }

    protected override void Initialize()
    {
        Window.Title = Title;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += Window_ClientSizeChanged;

        Exiting += GbaGame_Exiting;

        Engine.LoadConfig();

        ApplyDisplayConfig();

        // Find all installed games
        foreach (string gameDir in Directory.EnumerateDirectories(FileManager.GetDataDirectory(Engine.InstalledGamesDirName)))
        {
            foreach (string gameFile in Directory.EnumerateFiles(gameDir))
            {
                if (gameFile.EndsWith(".gba", StringComparison.InvariantCultureIgnoreCase))
                {
                    _gameInstallations.Add(new GameInstallation(gameDir, gameFile, Path.ChangeExtension(gameFile, ".sav"), Game, Platform.GBA));
                    break;
                }
                else if (gameFile.EndsWith(".app", StringComparison.InvariantCultureIgnoreCase))
                {
                    _gameInstallations.Add(new GameInstallation(gameDir, gameFile, Path.Combine(gameDir, "save.dat"), Game, Platform.NGage));
                    break;
                }
            }
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _gbaIcon = Content.Load<Texture2D>("GBA");
        _nGageIcon = Content.Load<Texture2D>("N-Gage");
        _font = Content.Load<SpriteFont>("Font");
        _paletteShader = Content.Load<Effect>("PaletteShader");

        // If there's only one game installation we load that directly
        if (_gameInstallations.Count == 1)
            LoadEngine(_gameInstallations[0]);

        base.LoadContent();
    }

    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
        base.Update(gameTime);

        InputManager.Update();

        // Toggle full-screen
        if (InputManager.IsButtonPressed(Keys.LeftAlt) && InputManager.IsButtonJustPressed(Keys.Enter))
        {
            Engine.Config.IsFullscreen = !_graphics.IsFullScreen;
            SaveWindowState();
            Engine.SaveConfig();
            ApplyDisplayConfig();
        }

        // Update mouse visibility
        IsMouseVisible = !_graphics.IsFullScreen || DebugMode;

        // Select game installation if not loaded yet
        if (!HasLoadedGameInstallation)
        {
            if (_gameInstallations.Count == 0)
                return;

            // If loading, keep calling LoadEngine until done
            if (_loadingGameInstallationTask != null)
            {
                LoadEngine(_selectedGameInstallation);
            }
            else
            {
                // Use arrow keys to select game
                if (InputManager.IsButtonJustPressed(Keys.Up))
                {
                    _selectedGameInstallationIndex--;
                    if (_selectedGameInstallationIndex < 0)
                        _selectedGameInstallationIndex = _gameInstallations.Count - 1;
                }
                else if (InputManager.IsButtonJustPressed(Keys.Down))
                {
                    _selectedGameInstallationIndex++;
                    if (_selectedGameInstallationIndex > _gameInstallations.Count - 1)
                        _selectedGameInstallationIndex = 0;
                }

                // Select with space or enter (but not when alt is pressed due to fullscreen toggle)
                if (InputManager.IsButtonReleased(Keys.LeftAlt) && (InputManager.IsButtonJustPressed(Keys.Space) || InputManager.IsButtonJustPressed(Keys.Enter)))
                    LoadEngine(_gameInstallations[_selectedGameInstallationIndex]);
            }
            
            return;
        }

        // If the previous frame was loading, then we disable it now
        if (Engine.IsLoading)
        {
            Engine.IsLoading = false;
            IsFixedTimeStep = true;
        }

        _skippedDraws++;

        if (_skippedDraws > 0)
            _fps = 0;

        if (DebugMode)
        {
            if (!IsPaused)
            {
                _performanceWindow.AddFps(_fps);
                _performanceWindow.AddSkippedDraws(_skippedDraws);
            }

            using Process p = Process.GetCurrentProcess();
            _performanceWindow.AddMemoryUsage(p.PrivateMemorySize64);
        }

        if (RunSingleFrame)
        {
            RunSingleFrame = false;
            Pause();
        }

        // Toggle debug mode
        if (InputManager.IsButtonJustPressed(Keys.Tab))
        {
            DebugMode = !DebugMode;

            if (DebugMode)
            {
                foreach (DebugWindow window in _debugLayout.GetWindows())
                    window.OnWindowOpened();
            }
            else
            {
                foreach (DebugWindow window in _debugLayout.GetWindows())
                    window.OnWindowClosed();
                
                SizeGameToWindow();
            }
        }

        // Toggle pause
        if (!_showMenu && InputManager.IsButtonPressed(Keys.LeftControl) && InputManager.IsButtonJustPressed(Keys.P))
        {
            if (!IsPaused)
                Pause();
            else
                Resume();
        }

        // Speed up game
        if (!_showMenu && InputManager.IsButtonPressed(Keys.LeftShift))
            SetFramerate(Framerate * 4);
        else if (InputManager.IsButtonJustReleased(Keys.LeftShift))
            SetFramerate(Framerate);

        // Toggle menu
        if (!_menu.IsTransitioningOut && InputManager.IsButtonJustPressed(Keys.Escape))
        {
            if (!_showMenu)
            {
                Pause();
                _menu.Open(new MainMenu(this));
                _showMenu = true;
            }
            else
            {
                _menu.Close();
            }
        }

        // Run one frame
        if (!_showMenu && InputManager.IsButtonPressed(Keys.LeftControl) && InputManager.IsButtonJustPressed(Keys.F))
        {
            IsPaused = false;
            RunSingleFrame = true;
        }

        StepEngine();

        if (_showMenu)
            _menu.Update();

        if (DebugMode && !IsPaused)
            _performanceWindow.AddUpdateTime(_updateTimeStopWatch.ElapsedMilliseconds);
    }

    protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
    {
        // Draw game selection if not loaded yet
        if (!HasLoadedGameInstallation)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            const int xPos = 80;
            const int yPos = 60;

            // TODO: Make this look a bit nicer? Have it scale better at different resolution by creating a matrix when rendering based on the screen size.
            if (_gameInstallations.Count == 0)
            {
                _spriteBatch.DrawString(_font, "No games were found", new Vector2(xPos, yPos), Color.White);
            }
            else if (_loadingGameInstallationTask != null)
            {
                _spriteBatch.DrawString(_font, "Loading...", new Vector2(xPos, yPos), Color.White);
            }
            else
            {
                _spriteBatch.DrawString(_font, "Select game to play", new Vector2(xPos, yPos), Color.White);

                for (int i = 0; i < _gameInstallations.Count; i++)
                {
                    if (_gameInstallations[i].Platform == Platform.GBA)
                        _spriteBatch.Draw(_gbaIcon, new Vector2(xPos, yPos + 60 + i * 30), Color.White);
                    else if (_gameInstallations[i].Platform == Platform.NGage) 
                        _spriteBatch.Draw(_nGageIcon, new Vector2(xPos, yPos + 60 + i * 30), Color.White);

                    _spriteBatch.DrawString(
                        spriteFont: _font, 
                        text: Path.GetFileName(_gameInstallations[i].Directory), 
                        position: new Vector2(xPos + 80, yPos + 60 + i * 30), 
                        color: i == _selectedGameInstallationIndex ? Color.Yellow : Color.White);
                }
            }

            _spriteBatch.End();
            return;
        }

        _fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

        _skippedDraws = -1;

        if (DebugMode)
            _debugGameRenderTarget.BeginRender();

        // Clear screen
        GraphicsDevice.Clear(Color.Black);

        // Draw screen
        Gfx.Draw(_gfxRenderer);
        if (DebugMode && !IsPaused)
            _performanceWindow.AddDrawCalls(GraphicsDevice.Metrics.DrawCount);
        if (_showMenu)
            _menu.Draw(_gfxRenderer);
        if (DebugMode)
            _debugLayout.DrawGame(_gfxRenderer);
        _gfxRenderer.EndRender();

        if (DebugMode)
        {
            _debugGameRenderTarget.EndRender();

            // Draw debug layout
            _debugLayout.Draw(gameTime);
        }

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            Engine.Unload();
    }

    #endregion

    #region Public Methods

    public abstract void SkipCutscene();

    public void SaveWindowState()
    {
        if (!HasLoadedGameInstallation || _graphics.IsFullScreen)
            return;

        switch (Engine.Settings.Platform)
        {
            case Platform.GBA:
                Engine.Config.GbaWindowBounds = Window.ClientBounds;
                break;

            case Platform.NGage:
                Engine.Config.NGageWindowBounds = Window.ClientBounds;
                break;

            default:
                throw new UnsupportedPlatformException();
        }
    }

    public void ApplyDisplayConfig()
    {
        if (Engine.Config.IsFullscreen)
        {
            SetResolution(Engine.Config.FullscreenResolution, true);
        }
        else
        {
            Rectangle bounds;

            if (HasLoadedGameInstallation)
            {
                bounds = Engine.Settings.Platform switch
                {
                    Platform.GBA => Engine.Config.GbaWindowBounds,
                    Platform.NGage => Engine.Config.NGageWindowBounds,
                    _ => throw new UnsupportedPlatformException()
                };
            }
            else
            {
                // We don't know the platform yet, so default to the gba bounds since it'll be the most common one
                bounds = Engine.Config.GbaWindowBounds;
            }

            if (bounds.Location != Point.Zero)
                Window.Position = bounds.Location;

            SetResolution(bounds.Size, false);
        }
    }

    public void Pause()
    {
        if (IsPaused)
            return;

        IsPaused = true;
        SoundEventsManager.ForcePauseAllSongs();
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        SoundEventsManager.ForceResumeAllSongs();
    }

    #endregion
}