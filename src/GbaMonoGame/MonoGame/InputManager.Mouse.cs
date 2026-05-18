using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public partial class InputManager
{
    private MouseState _previousMouseState;
    private MouseState _mouseState;

    public Vector2 MouseOffset { get; set; }

    private void UpdateMouse()
    {
        _previousMouseState = _mouseState;
        _mouseState = Engine.App.IsActive ? Mouse.GetState() : new MouseState();
    }

    public bool IsMouseOnScreen(RenderContext renderContext)
    {
        Vector2 mousePos = GetMousePosition(renderContext);

        if (mousePos.X < 0 || mousePos.Y < 0)
            return false;

        Vector2 resolution = renderContext.Resolution;

        if (mousePos.X >= resolution.X || mousePos.Y >= resolution.Y)
            return false;

        return true;
    }

    public Vector2 GetMousePosition(RenderContext renderContext) =>
        renderContext.ToWorldPosition(_mouseState.Position.ToVector2() + MouseOffset);
    public Vector2 GetMousePositionDelta(RenderContext renderContext) =>
        renderContext.ToWorldPosition(_mouseState.Position.ToVector2()) -
        renderContext.ToWorldPosition(_previousMouseState.Position.ToVector2());
    public int GetMouseWheelDelta() => _mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
    public MouseState GetMouseState() => _mouseState;
    public bool IsMouseLeftButtonJustPressed() => _mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
}