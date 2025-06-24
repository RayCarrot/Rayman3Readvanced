using System;
using BinarySerializer.Ubisoft.GbaEngine;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public static class InputManager
{
    private static readonly Dictionary<Input, GbaInput> _gbaInputMapping = new()
    {
        [Input.Gba_A] = GbaInput.A,
        [Input.Gba_B] = GbaInput.B,
        [Input.Gba_Select] = GbaInput.Select,
        [Input.Gba_Start] = GbaInput.Start,
        [Input.Gba_Right] = GbaInput.Right,
        [Input.Gba_Left] = GbaInput.Left,
        [Input.Gba_Up] = GbaInput.Up,
        [Input.Gba_Down] = GbaInput.Down,
        [Input.Gba_R] = GbaInput.R,
        [Input.Gba_L] = GbaInput.L,
    };

    private static KeyboardState _previousKeyboardState;
    private static KeyboardState _keyboardState;
    private static MouseState _previousMouseState;
    private static MouseState _mouseState;

    public static Vector2 MouseOffset { get; set; }

    public static Keys GetDefaultKey(Input input)
    {
        // TODO: Update default button mapping. Start should probably be esc and select should be backspace.
        return input switch
        {
            // GBA
            Input.Gba_A => Keys.Space,
            Input.Gba_B => Keys.S,
            Input.Gba_Select => Keys.C,
            Input.Gba_Start => Keys.V,
            Input.Gba_Right => Keys.Right,
            Input.Gba_Left => Keys.Left,
            Input.Gba_Up => Keys.Up,
            Input.Gba_Down => Keys.Down,
            Input.Gba_R => Keys.W,
            Input.Gba_L => Keys.Q,

            // Debug
            Input.Debug_ToggleBoxes => Keys.B,
            Input.Debug_ToggleCollision => Keys.T,
            Input.Debug_ToggleNoClip => Keys.Z,
            
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }
    public static Keys GetKey(Input input) => Engine.Config.Controls[input];
    public static GbaInput GetGbaInput(Input input) => _gbaInputMapping[input];

    // TODO: Improve
    public static string GetKeyName(Keys key) => key.ToString();

    public static Keys[] GetPressedKeys() => _keyboardState.GetPressedKeys();

    public static bool IsButtonPressed(Keys input) => _keyboardState.IsKeyDown(input);
    public static bool IsButtonReleased(Keys input) => _keyboardState.IsKeyUp(input);
    public static bool IsButtonJustPressed(Keys input) => _keyboardState.IsKeyDown(input) && _previousKeyboardState.IsKeyUp(input);
    public static bool IsButtonJustReleased(Keys input) => _keyboardState.IsKeyUp(input) && _previousKeyboardState.IsKeyDown(input);

    public static bool IsButtonPressed(Input input) => IsButtonPressed(GetKey(input));
    public static bool IsButtonReleased(Input input) => IsButtonReleased(GetKey(input));
    public static bool IsButtonJustPressed(Input input) => IsButtonJustPressed(GetKey(input));
    public static bool IsButtonJustReleased(Input input) => IsButtonJustReleased(GetKey(input));

    public static GbaInput GetPressedGbaInputs()
    {
        GbaInput inputs = GbaInput.Valid;

        foreach (KeyValuePair<Input, GbaInput> input in _gbaInputMapping)
        {
            if (IsButtonPressed(input.Key))
                inputs |= input.Value;
        }

        // Cancel out if opposite directions are pressed
        if ((inputs & (GbaInput.Right | GbaInput.Left)) == (GbaInput.Right | GbaInput.Left))
            inputs &= ~(GbaInput.Right | GbaInput.Left);
        if ((inputs & (GbaInput.Up | GbaInput.Down)) == (GbaInput.Up | GbaInput.Down))
            inputs &= ~(GbaInput.Up | GbaInput.Down);

        return inputs;
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

    public static void Update()
    {
        _previousKeyboardState = _keyboardState;
        _keyboardState = Engine.GbaGame.IsActive ? Keyboard.GetState() : new KeyboardState();

        _previousMouseState = _mouseState;
        _mouseState = Engine.GbaGame.IsActive ? Mouse.GetState() : new MouseState();
    }
}