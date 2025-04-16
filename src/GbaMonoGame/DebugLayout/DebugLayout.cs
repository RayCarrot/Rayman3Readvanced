using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class DebugLayout
{
    private readonly List<DebugWindow> _windows = [];
    private readonly List<DebugMenu> _menus = [];
    private DebugLayoutTextureManager _textureManager;
    private ImGuiRenderer _guiRenderer;

    public void AddWindow(DebugWindow window) => _windows.Add(window);
    public T GetWindow<T>() where T : DebugWindow => _windows.FirstOrDefault(x => x is T) as T;
    public IReadOnlyCollection<DebugWindow> GetWindows() => _windows;

    public void AddMenu(DebugMenu menu) => _menus.Add(menu);
    public T GetMenu<T>() where T : DebugMenu => _menus.FirstOrDefault(x => x is T) as T;
    public IReadOnlyCollection<DebugMenu> GetMenus() => _menus;

    public void Clear()
    {
        foreach (DebugWindow debugWindow in _windows)
            debugWindow.Unload();
        foreach (DebugMenu debugMenu in _menus)
            debugMenu.Unload();

        _windows.Clear();
        _menus.Clear();
    }

    public unsafe void LoadContent(Game game)
    {
        _guiRenderer = new ImGuiRenderer(game);
        _guiRenderer.RebuildFontAtlas();

        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        
        // Get the config file path
        string iniFilePath = FileManager.GetDataFile(Engine.ImgGuiConfigFileName);
        
        // Convert to ASCII bytes
        byte[] iniFilePathBytes = Encoding.ASCII.GetBytes(iniFilePath);
        
        // Allocate to unmanaged memory
        IntPtr iniFilePathBytesPointer = Marshal.AllocHGlobal(iniFilePathBytes.Length + 1);
        
        // Copy over the bytes
        Marshal.Copy(iniFilePathBytes, 0, iniFilePathBytesPointer, iniFilePathBytes.Length);
        
        // Null terminate
        *((byte*)iniFilePathBytesPointer + iniFilePathBytes.Length) = 0;
        
        // Set the path
        io.NativePtr->IniFilename = (byte*)iniFilePathBytesPointer;

        _textureManager = new DebugLayoutTextureManager(_guiRenderer);
    }

    public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
    {
        _guiRenderer.BeforeLayout(gameTime);

        ImGui.DockSpaceOverViewport();

        // Draw menus
        if (ImGui.BeginMainMenuBar())
        {
            foreach (DebugMenu menu in _menus)
            {
                if (ImGui.BeginMenu(menu.Name))
                {
                    menu.Draw(this, _textureManager);
                    ImGui.EndMenu();
                }
            }

            ImGui.EndMainMenuBar();
        }

        // Draw windows
        foreach (DebugWindow window in _windows)
        {
            if (window.IsOpen || !window.CanClose)
            {
                if (window.CanClose)
                {
                    bool open = window.IsOpen;

                    if (ImGui.Begin(window.Name, ref open))
                        window.Draw(this, _textureManager);

                    window.IsOpen = open;
                }
                else
                {
                    if (ImGui.Begin(window.Name))
                        window.Draw(this, _textureManager);
                }

                ImGui.End();
            }
        }

        _guiRenderer.AfterLayout();

        _textureManager.UnbindTextures();
    }

    public void DrawGame(GfxRenderer renderer)
    {
        foreach (DebugWindow window in _windows)
        {
            if (window.IsOpen || !window.CanClose)
                window.DrawGame(renderer);
        }
    }
}