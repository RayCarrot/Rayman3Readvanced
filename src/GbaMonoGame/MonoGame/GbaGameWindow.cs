using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
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
            if (_graphics.IsFullScreen)
                return Engine.Config.WindowResolution;
            else
                return GetResolution();
        }
        set
        {
            Engine.Config.WindowResolution = value;
            Engine.Config.WindowIsMaximized = false;

            if (!_graphics.IsFullScreen)
                ApplyState();
        }
    }

    public Point FullscreenResolution
    {
        get
        {
            if (_graphics.IsFullScreen)
                return Engine.Config.FullscreenResolution;
            else
                return GetResolution();
        }
        set
        {
            Engine.Config.FullscreenResolution = value;

            if (_graphics.IsFullScreen)
                ApplyState();
        }
    }

    public bool IsFullscreen
    {
        get => _graphics.IsFullScreen;
        set
        {
            SaveState();
            Engine.Config.IsFullscreen = value;
            ApplyState();
        }
    }

    private void SetResolution(Point size, bool isFullscreen)
    {
        _graphics.IsFullScreen = isFullscreen;
        _graphics.PreferredBackBufferWidth = size.X;
        _graphics.PreferredBackBufferHeight = size.Y;
        _graphics.ApplyChanges();
    }

    public Point GetResolution()
    {
        return new Point(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
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
            Engine.Config.IsFullscreen = true;
            Engine.Config.FullscreenResolution = GetResolution();
        }
        else
        {
            Engine.Config.IsFullscreen = false;
            Engine.Config.WindowPosition = _window.Position;
            Engine.Config.WindowResolution = GetResolution();
            Engine.Config.WindowIsMaximized = _form.WindowState == FormWindowState.Maximized;
        }
    }

    public void ApplyState()
    {
        if (Engine.Config.IsFullscreen)
        {
            SetResolution(Engine.Config.FullscreenResolution, true);
        }
        else
        {
            SetResolution(Engine.Config.WindowResolution, false);

            if (Engine.Config.WindowPosition != Point.Zero)
                _window.Position = Engine.Config.WindowPosition;

            _form.WindowState = Engine.Config.WindowIsMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
        }
    }
}