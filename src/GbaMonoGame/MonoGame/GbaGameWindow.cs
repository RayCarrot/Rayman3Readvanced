using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;

namespace GbaMonoGame;

public class GbaGameWindow
{
    public GbaGameWindow(GameWindow window, GraphicsDeviceManager graphics)
    {
        _window = window;
        _graphics = graphics;
        _form = (Form)Control.FromHandle(_window.Handle);
    }

    private GameWindow _window { get; }
    private GraphicsDeviceManager _graphics { get; }
    private Form _form { get; }

    public Point WindowResolution
    {
        get
        {
            if (DisplayMode == DisplayMode.Windowed)
                return GetResolution();
            else
                return Engine.Config.WindowResolution;
        }
        set
        {
            Engine.Config.WindowResolution = value;
            Engine.Config.WindowIsMaximized = false;

            if (DisplayMode == DisplayMode.Windowed)
            {
                if (_form.WindowState != FormWindowState.Normal)
                    _form.WindowState = FormWindowState.Normal;

                SetResolution(value, DisplayMode.Windowed);
            }
        }
    }

    public Point FullscreenResolution
    {
        get
        {
            if (DisplayMode == DisplayMode.Fullscreen)
                return GetResolution();
            else
                return Engine.Config.FullscreenResolution;
        }
        set
        {
            Engine.Config.FullscreenResolution = value;

            if (DisplayMode == DisplayMode.Fullscreen)
                ApplyState();
        }
    }

    public DisplayMode DisplayMode
    {
        get
        {
            if (_graphics.IsFullScreen)
            {
                if (_graphics.HardwareModeSwitch)
                    return DisplayMode.Fullscreen;
                else
                    return DisplayMode.Borderless;
            }
            else
            {
                return DisplayMode.Windowed;
            }
        }
        set
        {
            SaveState();
            Engine.Config.DisplayMode = value;
            ApplyState();
        }
    }

    private void SetResolution(Point size, DisplayMode displayMode)
    {
        switch (displayMode)
        {
            case DisplayMode.Windowed:
                _graphics.IsFullScreen = false;
                break;

            case DisplayMode.Fullscreen:
                _graphics.IsFullScreen = true;
                _graphics.HardwareModeSwitch = true;
                break;

            case DisplayMode.Borderless:
                _graphics.IsFullScreen = true;
                _graphics.HardwareModeSwitch = false;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(displayMode), displayMode, null);
        }

        _graphics.PreferredBackBufferWidth = size.X;
        _graphics.PreferredBackBufferHeight = size.Y;
        _graphics.ApplyChanges();
    }

    public Point GetResolution()
    {
        return new Point(_graphics.GraphicsDevice.PresentationParameters.BackBufferWidth, _graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);
    }

    public void SetTitle(string title)
    {
        _window.Title = title;
    }

    public bool IsResizable()
    {
        return _form.WindowState == FormWindowState.Normal && !_graphics.IsFullScreen;
    }

    public void SetResizeMode(bool allowResize, Point minSize, Point maxSize)
    {
        _window.AllowUserResizing = allowResize;
        _form.MinimumSize = new Size(minSize.X, minSize.Y);
        _form.MaximumSize = new Size(maxSize.X, maxSize.Y);
    }

    public void SaveState()
    {
        if (_graphics.IsFullScreen)
        {
            if (_graphics.HardwareModeSwitch)
            {
                Engine.Config.DisplayMode = DisplayMode.Fullscreen;
                Engine.Config.FullscreenResolution = GetResolution();
            }
            else
            {
                Engine.Config.DisplayMode = DisplayMode.Borderless;
            }
        }
        else
        {
            Engine.Config.DisplayMode = DisplayMode.Windowed;
            Engine.Config.WindowPosition = _window.Position;
            Engine.Config.WindowResolution = GetResolution();
            Engine.Config.WindowIsMaximized = _form.WindowState == FormWindowState.Maximized;
        }
    }

    public void ApplyState()
    {
        switch (Engine.Config.DisplayMode)
        {
            case DisplayMode.Windowed:
                SetResolution(Engine.Config.WindowResolution, DisplayMode.Windowed);

                if (Engine.Config.WindowPosition != Point.Zero)
                    _window.Position = Engine.Config.WindowPosition;

                _form.WindowState = Engine.Config.WindowIsMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
                break;

            case DisplayMode.Fullscreen:
                SetResolution(Engine.Config.FullscreenResolution, DisplayMode.Fullscreen);
                break;

            case DisplayMode.Borderless:
                GraphicsAdapter adapter = _graphics.GraphicsDevice.Adapter;
                SetResolution(new Point(adapter.CurrentDisplayMode.Width, adapter.CurrentDisplayMode.Height), DisplayMode.Borderless);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}