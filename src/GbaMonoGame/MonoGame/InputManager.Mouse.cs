using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public static partial class InputManager
{
    private static MouseState _previousMouseState;
    private static MouseState _mouseState;

    public static Vector2 MouseOffset { get; set; }

    private static void UpdateMouse()
    {
        _previousMouseState = _mouseState;
        _mouseState = Engine.GbaGame.IsActive ? Mouse.GetState() : new MouseState();
    }

    public static bool IsMouseOnScreen(RenderContext renderContext)
    {
        Vector2 mousePos = GetMousePosition(renderContext);

        if (mousePos.X < 0 || mousePos.Y < 0)
            return false;

        Vector2 resolution = renderContext.Resolution;

        if (mousePos.X >= resolution.X || mousePos.Y >= resolution.Y)
            return false;

        return true;
    }

    public static Vector2 GetMousePosition(RenderContext renderContext) =>
        renderContext.ToWorldPosition(_mouseState.Position.ToVector2() + MouseOffset);
    public static Vector2 GetMousePositionDelta(RenderContext renderContext) =>
        renderContext.ToWorldPosition(_mouseState.Position.ToVector2()) -
        renderContext.ToWorldPosition(_previousMouseState.Position.ToVector2());
    public static int GetMouseWheelDelta() => _mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
    public static MouseState GetMouseState() => _mouseState;
    public static bool IsMouseLeftButtonJustPressed() => _mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
}