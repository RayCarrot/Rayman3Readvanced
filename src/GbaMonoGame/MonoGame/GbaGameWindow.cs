using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWSDX
using Point = Microsoft.Xna.Framework.Point;
using System.Drawing;
using System.Windows.Forms;
#elif DESKTOPGL
using System.Runtime.InteropServices;
#endif

namespace GbaMonoGame;

// TODO: None of the P/Invoke calls for SDL2 work
public class GbaGameWindow
{
    public GbaGameWindow(GameWindow window, GraphicsDeviceManager graphics)
    {
        _window = window;
        _graphics = graphics;

#if WINDOWSDX
        _form = (Form)Control.FromHandle(_window.Handle);
#elif DESKTOPGL
        _sdlWindowHandle = window.Handle;
#endif
    }

#if DESKTOPGL
    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint SDL_GetWindowFlags(IntPtr window);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetWindowMinimumSize(IntPtr window, int min_w, int min_h);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_SetWindowMaximumSize(IntPtr window, int max_w, int max_h);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_RestoreWindow(IntPtr window);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_MaximizeWindow(IntPtr window);
#endif

    private GameWindow _window { get; }
    private GraphicsDeviceManager _graphics { get; }

#if WINDOWSDX
    private Form _form { get; }
#elif DESKTOPGL
    private IntPtr _sdlWindowHandle { get; }
#endif

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
#if WINDOWSDX
                if (_form.WindowState != FormWindowState.Normal)
                    _form.WindowState = FormWindowState.Normal;
#elif DESKTOPGL
                SDL_WindowFlags flags = GetWindowFlags();
                if ((flags & SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) != 0 ||
                    (flags & SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0)
                    SDL_RestoreWindow(_sdlWindowHandle);
#endif

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

#if DESKTOPGL
    private SDL_WindowFlags GetWindowFlags()
    {
        return (SDL_WindowFlags)SDL_GetWindowFlags(_sdlWindowHandle);
    }
#endif

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
#if WINDOWSDX
        if (_form.WindowState != FormWindowState.Normal)
            return false;
#elif DESKTOPGL
        SDL_WindowFlags flags = GetWindowFlags();
        if ((flags & SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) != 0 ||
            (flags & SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0)
            return false;
#endif

        return !_graphics.IsFullScreen;
    }

    public void SetResizeMode(bool allowResize, Point minSize, Point maxSize)
    {
        _window.AllowUserResizing = allowResize;

#if WINDOWSDX
        _form.MinimumSize = new Size(minSize.X, minSize.Y);
        _form.MaximumSize = new Size(maxSize.X, maxSize.Y);
#elif DESKTOPGL
        SDL_SetWindowMinimumSize(_sdlWindowHandle, minSize.X, minSize.Y);
        SDL_SetWindowMaximumSize(_sdlWindowHandle, maxSize.X, maxSize.Y);
#endif
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

#if WINDOWSDX
            Engine.Config.WindowIsMaximized = _form.WindowState == FormWindowState.Maximized;
#elif DESKTOPGL
            SDL_WindowFlags flags = GetWindowFlags();
            Engine.Config.WindowIsMaximized = (flags & SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) != 0;
#endif
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

#if WINDOWSDX
                _form.WindowState = Engine.Config.WindowIsMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
#elif DESKTOPGL
                if (Engine.Config.WindowIsMaximized)
                    SDL_MaximizeWindow(_sdlWindowHandle);
                else
                    SDL_RestoreWindow(_sdlWindowHandle);
#endif
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

#if DESKTOPGL
    [Flags]
    private enum SDL_WindowFlags
    {
        SDL_WINDOW_FULLSCREEN = 0x00000001,
        SDL_WINDOW_OPENGL = 0x00000002,
        SDL_WINDOW_SHOWN = 0x00000004,
        SDL_WINDOW_HIDDEN = 0x00000008,
        SDL_WINDOW_BORDERLESS = 0x00000010,
        SDL_WINDOW_RESIZABLE = 0x00000020,
        SDL_WINDOW_MINIMIZED = 0x00000040,
        SDL_WINDOW_MAXIMIZED = 0x00000080,
        SDL_WINDOW_MOUSE_GRABBED = 0x00000100,
        SDL_WINDOW_INPUT_FOCUS = 0x00000200,
        SDL_WINDOW_MOUSE_FOCUS = 0x00000400,
        SDL_WINDOW_FULLSCREEN_DESKTOP = (SDL_WINDOW_FULLSCREEN | 0x00001000),
        SDL_WINDOW_FOREIGN = 0x00000800,
        SDL_WINDOW_ALLOW_HIGHDPI = 0x00002000,
        SDL_WINDOW_MOUSE_CAPTURE = 0x00004000,
        SDL_WINDOW_ALWAYS_ON_TOP = 0x00008000,
        SDL_WINDOW_SKIP_TASKBAR = 0x00010000,
        SDL_WINDOW_UTILITY = 0x00020000,
        SDL_WINDOW_TOOLTIP = 0x00040000,
        SDL_WINDOW_POPUP_MENU = 0x00080000,
        SDL_WINDOW_KEYBOARD_GRABBED = 0x00100000,
        SDL_WINDOW_VULKAN = 0x10000000,
        SDL_WINDOW_METAL = 0x20000000,
        SDL_WINDOW_INPUT_GRABBED = SDL_WINDOW_MOUSE_GRABBED,
    }
#endif
}