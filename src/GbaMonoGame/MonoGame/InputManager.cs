using System;
using BinarySerializer.Ubisoft.GbaEngine;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public static class InputManager
{
    private static readonly Dictionary<GbaInput, Input> _gbaInputMapping = new()
    {
        [GbaInput.A] = Input.Gba_A,
        [GbaInput.B] = Input.Gba_B,
        [GbaInput.Select] = Input.Gba_Select,
        [GbaInput.Start] = Input.Gba_Start,
        [GbaInput.Right] = Input.Gba_Right,
        [GbaInput.Left] = Input.Gba_Left,
        [GbaInput.Up] = Input.Gba_Up,
        [GbaInput.Down] = Input.Gba_Down,
        [GbaInput.R] = Input.Gba_R,
        [GbaInput.L] = Input.Gba_L,
    };

    private static KeyboardState _previousKeyboardState;
    private static KeyboardState _keyboardState;
    private static MouseState _previousMouseState;
    private static MouseState _mouseState;

    public static Vector2 MouseOffset { get; set; }

    public static Keys GetDefaultKey(Input input)
    {
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

            // Menu
            Input.Menu_Right => Keys.Right,
            Input.Menu_Left => Keys.Left,
            Input.Menu_Up => Keys.Up,
            Input.Menu_Down => Keys.Down,
            Input.Menu_Confirm => Keys.Space,
            Input.Menu_Back => Keys.Back,

            // Debug
            Input.Debug_ToggleBoxes => Keys.B,
            Input.Debug_ToggleCollision => Keys.T,
            Input.Debug_ToggleNoClip => Keys.Z,
            
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }
    public static Keys GetKey(Input input) => Engine.Config.Controls[input];

    public static bool IsButtonPressed(Keys input) => _keyboardState.IsKeyDown(input);
    public static bool IsButtonReleased(Keys input) => _keyboardState.IsKeyUp(input);
    public static bool IsButtonJustPressed(Keys input) => _keyboardState.IsKeyDown(input) && _previousKeyboardState.IsKeyUp(input);
    public static bool IsButtonJustReleased(Keys input) => _keyboardState.IsKeyUp(input) && _previousKeyboardState.IsKeyDown(input);

    public static bool IsButtonPressed(Input input) => IsButtonPressed(GetKey(input));
    public static bool IsButtonReleased(Input input) => IsButtonReleased(GetKey(input));
    public static bool IsButtonJustPressed(Input input) => IsButtonJustPressed(GetKey(input));
    public static bool IsButtonJustReleased(Input input) => IsButtonJustReleased(GetKey(input));

    public static GbaInput GetGbaInputs()
    {
        GbaInput inputs = GbaInput.Valid;

        foreach (KeyValuePair<GbaInput, Input> input in _gbaInputMapping)
        {
            if (IsButtonPressed(input.Value))
                inputs |= input.Key;
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