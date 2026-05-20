using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public abstract class GbaGame : Game
{
    #region Constructor

    protected GbaGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _updateTimeStopWatch = new Stopwatch();
    }

    #endregion

    #region Private Fields

    // The GBA framerate is actually 59.727500569606, but we do 60
    public const float Framerate = 60;

    private readonly GraphicsDeviceManager _graphics;
    private readonly Stopwatch _updateTimeStopWatch;

    private ApplicationManager _applicationManager;
    private GameWindowManager _gameWindowManager;
    private GfxRenderer _gfxRenderer;
    private DebugLayout _debugLayout;
    private GbaRenderTarget _debugGameRenderTarget;
    private LoggerDebugWindow _loggerWindow;
    private PerformanceDebugWindow _performanceWindow;
    private int _skippedDraws = -1;
    private float _fps = 60;
    private Point _prevWindowResolution;
    private Vector2 _prevInternalResolution;
    private bool _prevLockWindowAspectRatio;
    private bool _speedUp;

    #endregion

    #region Protected Properties

    protected abstract string Title { get; }

    #endregion

    #region Public Properties

    public bool RunSingleFrame { get; set; }
    public bool IsPaused { get; set; }
    public bool DebugMode { get; set; }

    #endregion

    #region Event Handlers

    private void GbaGame_Exiting(object sender, EventArgs e)
    {
        // Save window state
        Engine.Window.SaveState();

        // Save settings
        Engine.Settings.Save();
    }

    private void Rom_Loaded(object sender, EventArgs e)
    {
        // Load the game
        InitGame();

        if (Engine.Settings.Active.Debug.DebugModeEnabled)
        {
            // Load the debug layout
            if (_debugLayout == null)
            {
                _debugLayout = new DebugLayout();
                _debugLayout.LoadContent(this);
            }

            _debugLayout.AddWindow(new GameDebugWindow(_debugGameRenderTarget, Engine.ViewPort));
            _debugLayout.AddWindow(_performanceWindow = new PerformanceDebugWindow());
            _debugLayout.AddWindow(_loggerWindow);
            _debugLayout.AddWindow(new GfxDebugWindow());
            _debugLayout.AddWindow(new SoundDebugWindow());
            _debugLayout.AddWindow(new MultiplayerDebugWindow());
            _debugLayout.AddWindow(new JoyPadDebugWindow());
            _debugLayout.AddMenu(new WindowsDebugMenu());
            AddDebugWindowsAndMenus(_debugLayout);
        }
    }

    private void Rom_Unloaded(object sender, EventArgs e)
    {
        // Unload the game
        UnInitGame();

        // Clear debug windows and menus
        _debugLayout?.Clear();

        // Clear graphics
        Gfx.Clear();
    }

    #endregion

    #region Private Methods

    private void SetFramerate(float fps)
    {
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1 / fps);
    }

    private void StepEngine()
    {
        if (Engine.Messages.IsShowingMessage())
            return;

        if (IsPaused)
            return;

        if (DebugMode)
            _updateTimeStopWatch.Restart();

        try
        {
            if (!_speedUp)
            {
                Engine.Step();
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    Engine.Step();
            }
        }
        catch (Exception ex)
        {
            Engine.FrameMngr.SetNextFrame(CreateFatalErrorFrame(ex));
        }

        // If this frame did a load, and thus might have taken longer than 1/60th of a
        // second, then we reset the timers to avoid MonoGame repeatedly calling Update()
        // to make up for the lost time, and thus dropping frames
        if (_applicationManager.IsLoading)
        {
            ResetElapsedTime();
            _applicationManager.IsLoading = false;
        }

        if (DebugMode)
            _updateTimeStopWatch.Stop();

        Engine.Messages.ShowQueuedMessage();
    }

    #endregion

    #region Protected Methods

    protected abstract Frame CreateInitialFrame();
    protected abstract Frame CreateFatalErrorFrame(Exception exception);
    protected virtual void InitEngine() { }
    protected virtual void InitGame() { }
    protected virtual void UnInitEngine() { }
    protected virtual void UnInitGame() { }
    protected virtual void AddDebugWindowsAndMenus(DebugLayout debugLayout) { }

    protected override void Initialize()
    {
        base.Initialize();

        // Set the game's framerate
        SetFramerate(Framerate);

        // Initialize the window
        _gameWindowManager = new GameWindowManager(Window, _graphics);
        _gameWindowManager.SetTitle(Title);
        _gameWindowManager.SetResizeMode(
            allowResize: true, 
            minSize: new Point(100, 100), 
            maxSize: new Point(GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height));

        // Subscribe to events
        Exiting += GbaGame_Exiting;
        Rom.Loaded += Rom_Loaded;
        Rom.Unloaded += Rom_Unloaded;

        // Load the settings
        SettingsManager settings = new();

        // Create the logger window now so we can start receiving logs during initialization
        if (settings.Active.Debug.DebugModeEnabled)
            _loggerWindow = new LoggerDebugWindow();

        _applicationManager = new ApplicationManager(this);

        // Initialize the engine
        Engine.InitEngine(
            settings: settings, 
            app: _applicationManager, 
            input: new InputManager(),
            window: _gameWindowManager, 
            viewPort: new ViewPortManager(), 
            assets: new AssetManager(Services),
            config: new GameConfigManager(),
            messages: new MessageManager(),
            fileDialog: new FileDialogManager(),
            richPresence: new DiscordRichPresenceManager(),
            frameMngr: new FrameManager());

        // Load the graphics management
        Gfx.Load();

        // Init the engine from the game implementation
        InitEngine();

        // Set the initial frame
        Engine.FrameMngr.SetNextFrame(CreateInitialFrame());

        // Apply the window state
        Engine.Window.VSync = Engine.Settings.Local.Display.VSync;
        Engine.Window.ApplyState();

        // Create the renderer
        _gfxRenderer = new GfxRenderer(GraphicsDevice);

        if (Engine.Settings.Active.Debug.DebugModeEnabled)
            _debugGameRenderTarget = new GbaRenderTarget(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
    }

    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
        base.Update(gameTime);

        Engine.Input.Update();

        // Toggle full-screen
        if (Engine.Settings.Local.Display.AltEnterToggle is { } altEnterToggle && 
            Engine.Input.IsKeyPressed(Keys.LeftAlt) && Engine.Input.IsKeyJustPressed(Keys.Enter))
        {
            Engine.Window.DisplayMode = Engine.Window.DisplayMode switch
            {
                DisplayMode.Windowed => altEnterToggle,
                DisplayMode.Fullscreen => DisplayMode.Windowed,
                DisplayMode.Borderless => DisplayMode.Windowed,
                _ => throw new ArgumentOutOfRangeException()
            };

            // Exit early since we don't want the enter key to be read by the game as just pressed
            return;
        }

        // Update mouse visibility
        IsMouseVisible = !_graphics.IsFullScreen || DebugMode;

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

        if (Engine.Settings.Active.Debug.DebugModeEnabled)
        {
            // Toggle debug mode
            if (Engine.Input.IsInputJustPressed(Input.Debug_ToggleDebugMode) && _debugLayout != null)
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
                }

                // Reset
                _prevWindowResolution = default;
            }

            // Toggle pause
            if (Engine.Input.IsInputJustPressed(Input.Debug_TogglePause))
            {
                if (!IsPaused)
                    Pause();
                else
                    Resume();
            }

            // Speed up game
            if (Engine.Input.IsInputPressed(Input.Debug_SpeedUp))
                _speedUp = true;
            else if (Engine.Input.IsInputJustReleased(Input.Debug_SpeedUp))
                _speedUp = false;

            // Run one frame
            if (Engine.Input.IsInputJustPressed(Input.Debug_StepOneFrame))
            {
                IsPaused = false;
                RunSingleFrame = true;
            }
        }

        StepEngine();

        if (DebugMode && !IsPaused)
            _performanceWindow.AddUpdateTime(_updateTimeStopWatch.ElapsedMilliseconds);
    }

    protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
    {
        _fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

        _skippedDraws = -1;

        if (DebugMode)
            _debugGameRenderTarget.BeginRender();

        if (!DebugMode && 
            (Engine.Window.GetResolution() != _prevWindowResolution || 
             Engine.Settings.Active.Tweaks.InternalGameResolution != _prevInternalResolution || 
             Engine.Settings.Local.Display.LockWindowAspectRatio != _prevLockWindowAspectRatio))
        {
            Point newRes = Engine.Window.GetResolution();
            
            if (Engine.Settings.Local.Display.LockWindowAspectRatio && Engine.Window.IsResizable())
            {
                Vector2 resolution = Engine.ViewPort.InternalGameResolution;

                float screenScale = newRes.X / resolution.X;

                newRes = new Vector2(resolution.X * screenScale, resolution.Y * screenScale).ToRoundedPoint();

                Engine.Window.WindowResolution = newRes;
            }

            _prevWindowResolution = newRes;
            _prevInternalResolution = Engine.ViewPort.InternalGameResolution;
            _prevLockWindowAspectRatio = Engine.Settings.Local.Display.LockWindowAspectRatio;

            Engine.ViewPort.Resize(Engine.Window.GetResolution().ToVector2());
        }

        // Clear screen
        GraphicsDevice.Clear(Color.Black);

        // Draw screen
        Gfx.Draw(_gfxRenderer);
        if (DebugMode && !IsPaused)
            _performanceWindow.AddDrawCalls(GraphicsDevice.Metrics.DrawCount);
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
        Rom.UnInit();
        Engine.UnInitEngine();
        UnInitEngine();
        base.Dispose(disposing);
    }

    #endregion

    #region Public Methods

    public void Pause()
    {
        if (IsPaused)
            return;

        IsPaused = true;
        Engine.Sem?.ForcePauseAllSongs();
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        Engine.Sem?.ForceResumeAllSongs();
    }

    #endregion
}