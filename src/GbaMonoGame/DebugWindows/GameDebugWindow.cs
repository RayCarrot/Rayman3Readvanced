using System;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

/// <summary>
/// Debug window which renders the game.
/// </summary>
public class GameDebugWindow : DebugWindow
{
    public GameDebugWindow(GameRenderTarget gameRenderTarget)
    {
        GameRenderTarget = gameRenderTarget;
    }

    private Point _previousWindowSize;

    public override string Name => "Game";
    public override bool CanClose => false;
    public GameRenderTarget GameRenderTarget { get; }

    public void RefreshSize()
    {
        // Reset previous size to force it to refresh
        _previousWindowSize = Point.Zero;
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        Point newSize = new((int)ImGui.GetContentRegionAvail().X, (int)ImGui.GetContentRegionAvail().Y);

        if (newSize != _previousWindowSize)
        {
            _previousWindowSize = newSize;
            GameRenderTarget.ResizeGame(newSize);
        }

        if (GameRenderTarget.RenderTarget != null)
        {
            IntPtr texPtr = textureManager.BindTexture(GameRenderTarget.RenderTarget);
            InputManager.MouseOffset = -ImGui.GetCursorScreenPos();
            ImGui.Image(texPtr, new System.Numerics.Vector2(GameRenderTarget.RenderTarget.Width, GameRenderTarget.RenderTarget.Height));
        }
    }

    public override void OnWindowOpened()
    {
        RefreshSize();
    }

    public override void OnWindowClosed()
    {
        InputManager.MouseOffset = Vector2.Zero;
    }
}