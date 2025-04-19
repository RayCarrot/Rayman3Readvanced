﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

// TODO: Create crash screen in case of exception, during update or load
public abstract class GbaGame : Microsoft.Xna.Framework.Game
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

    private GbaGameWindow _gameWindow;
    private GfxRenderer _gfxRenderer;
    private DebugLayout _debugLayout;
    private GameRenderTarget _debugGameRenderTarget;
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
        Engine.GameWindow.SaveState();

        // Save config
        Engine.SaveConfig();
    }

    private void Rom_Loaded(object sender, EventArgs e)
    {
        // Load the game
        LoadGame();

        if (Engine.Config.DebugModeEnabled)
        {
            // Load the debug layout
            if (_debugLayout == null)
            {
                _debugLayout = new DebugLayout();
                _debugLayout.LoadContent(this);
            }

            _debugLayout.AddWindow(new GameDebugWindow(_debugGameRenderTarget));
            _debugLayout.AddWindow(_performanceWindow = new PerformanceDebugWindow());
            _debugLayout.AddWindow(_loggerWindow);
            _debugLayout.AddWindow(new GfxDebugWindow());
            _debugLayout.AddWindow(new SoundDebugWindow());
            _debugLayout.AddWindow(new MultiplayerDebugWindow());
            _debugLayout.AddMenu(new WindowsDebugMenu());
            AddDebugWindowsAndMenus(_debugLayout);
        }

        // Check launch arguments
        string[] args = Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            // Check if it's a BizHawk TAS file
            if (arg.EndsWith(".bk2") && File.Exists(arg))
                LoadBizHawkTas(arg);
        }
    }

    private void Rom_Unloaded(object sender, EventArgs e)
    {
        // Unload the game
        UnloadGame();

        // Clear debug windows and menus
        _debugLayout?.Clear();

        // Clear the cache
        Engine.TextureCache.Clear();
        Engine.PaletteCache.Clear();
    }

    #endregion

    #region Private Methods

    private void SetFramerate(float fps)
    {
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1 / fps);
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

        if (DebugMode)
            _updateTimeStopWatch.Restart();

        if (!_speedUp)
        {
            Engine.Step();
        }
        else
        {
            for (int i = 0; i < 4; i++)
                Engine.Step();
        }

        // If this frame did a load, and thus might have taken longer than 1/60th of a second, then
        // we disable fixed time step to avoid MonoGame repeatedly calling Update() to make up for
        // the lost time, and thus drop frames
        if (Engine.IsLoading)
            IsFixedTimeStep = false;

        if (DebugMode)
            _updateTimeStopWatch.Stop();
    }

    #endregion

    #region Protected Methods

    protected abstract Frame CreateInitialFrame();
    protected virtual void LoadGame() { }
    protected virtual void UnloadGame() { }
    protected virtual void AddDebugWindowsAndMenus(DebugLayout debugLayout) { }

    protected override void Initialize()
    {
        base.Initialize();

        // Set the game's framerate
        SetFramerate(Framerate);

        // Initialize the window
        _gameWindow = new GbaGameWindow(Window, _graphics);
        _gameWindow.SetTitle(Title);
        _gameWindow.SetResizeMode(
            allowResize: true, 
            minSize: new Point(100, 100), 
            maxSize: new Point(GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height));

        // Subscribe to events
        Exiting += GbaGame_Exiting;
        Rom.Loaded += Rom_Loaded;
        Rom.Unloaded += Rom_Unloaded;

        // Load the config
        Engine.LoadConfig();

        // Create the logger window now so we can start receiving logs during initialization
        if (Engine.Config.DebugModeEnabled)
            _loggerWindow = new LoggerDebugWindow();

        // Load the engine
        Engine.Init(this, _gameWindow, CreateInitialFrame());

        // Apply the window state
        Engine.GameWindow.ApplyState();

        // Load the renderer
        _gfxRenderer = new GfxRenderer(GraphicsDevice);

        if (Engine.Config.DebugModeEnabled)
            _debugGameRenderTarget = new GameRenderTarget(GraphicsDevice, Engine.GameViewPort);
    }

    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
        base.Update(gameTime);

        InputManager.Update();

        // Toggle full-screen
        if (InputManager.IsButtonPressed(Keys.LeftAlt) && InputManager.IsButtonJustPressed(Keys.Enter))
            Engine.GameWindow.DisplayMode = Engine.GameWindow.DisplayMode switch
            {
                DisplayMode.Windowed => DisplayMode.Fullscreen,
                DisplayMode.Fullscreen => DisplayMode.Windowed,
                DisplayMode.Borderless => DisplayMode.Windowed,
                _ => throw new ArgumentOutOfRangeException()
            };

        // Update mouse visibility
        IsMouseVisible = !_graphics.IsFullScreen || DebugMode;

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

        if (Engine.Config.DebugModeEnabled)
        {
            // Toggle debug mode
            if (InputManager.IsButtonJustPressed(Keys.Tab) && _debugLayout != null)
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
            if (InputManager.IsButtonPressed(Keys.LeftControl) && InputManager.IsButtonJustPressed(Keys.P))
            {
                if (!IsPaused)
                    Pause();
                else
                    Resume();
            }

            // Speed up game
            if (InputManager.IsButtonPressed(Keys.LeftShift))
                _speedUp = true;
            else if (InputManager.IsButtonJustReleased(Keys.LeftShift))
                _speedUp = false;

            // Run one frame
            if (InputManager.IsButtonPressed(Keys.LeftControl) && InputManager.IsButtonJustPressed(Keys.F))
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
            (Engine.GameWindow.GetResolution() != _prevWindowResolution || 
             Engine.Config.InternalGameResolution != _prevInternalResolution || 
             Engine.Config.LockWindowAspectRatio != _prevLockWindowAspectRatio))
        {
            Point newRes = Engine.GameWindow.GetResolution();
            
            if (Engine.Config.LockWindowAspectRatio && Engine.GameWindow.IsResizable())
            {
                Vector2 resolution = Engine.InternalGameResolution;

                float screenScale = newRes.X / resolution.X;

                newRes = new Vector2(resolution.X * screenScale, resolution.Y * screenScale).ToRoundedPoint();

                Engine.GameWindow.WindowResolution = newRes;
            }

            _prevWindowResolution = newRes;
            _prevInternalResolution = Engine.InternalGameResolution;
            _prevLockWindowAspectRatio = Engine.Config.LockWindowAspectRatio;

            Engine.GameViewPort.Resize(Engine.GameWindow.GetResolution().ToVector2());
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
        Engine.UnInit();
        base.Dispose(disposing);
    }

    #endregion

    #region Public Methods

    public void Pause()
    {
        if (IsPaused)
            return;

        IsPaused = true;

        if (SoundEventsManager.IsLoaded)
            SoundEventsManager.ForcePauseAllSongs();
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        
        if (SoundEventsManager.IsLoaded)
            SoundEventsManager.ForceResumeAllSongs();
    }

    #endregion
}