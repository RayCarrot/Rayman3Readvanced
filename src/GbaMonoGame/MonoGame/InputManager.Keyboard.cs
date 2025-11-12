using System;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame;

public static partial class InputManager
{
    private static KeyboardState _previousKeyboardState;
    private static KeyboardState _keyboardState;

    private static void UpdateKeyboard()
    {
        _previousKeyboardState = _keyboardState;
        _keyboardState = Engine.GbaGame.IsActive ? Keyboard.GetState() : new KeyboardState();
    }

    public static Keys GetKey(Input input) => Engine.LocalConfig.Controls.Controls[input];

    public static Keys GetDefaultKey(Input input)
    {
        return input switch
        {
            // GBA
            Input.Gba_A => Keys.Space,
            Input.Gba_B => Keys.S,
            Input.Gba_Select => Keys.Back,
            Input.Gba_Start => Keys.Escape,
            Input.Gba_Right => Keys.Right,
            Input.Gba_Left => Keys.Left,
            Input.Gba_Up => Keys.Up,
            Input.Gba_Down => Keys.Down,
            Input.Gba_R => Keys.W,
            Input.Gba_L => Keys.Q,

            // Debug
            Input.Debug_ToggleDebugMode => Keys.Tab,
            Input.Debug_TogglePause => Keys.P,
            Input.Debug_StepOneFrame => Keys.F,
            Input.Debug_SpeedUp => Keys.LeftShift,
            Input.Debug_ToggleDisplayBoxes => Keys.B,
            Input.Debug_ToggleDisplayCollision => Keys.C,
            Input.Debug_ToggleNoClip => Keys.Z,

            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }

    // TODO: Improve with localized names
    public static string GetKeyName(Keys key) => key.ToString();

    public static Keys[] GetPressedKeys() => _keyboardState.GetPressedKeys();

    public static bool IsKeyPressed(Keys key) => _keyboardState.IsKeyDown(key);
    public static bool IsKeyReleased(Keys key) => _keyboardState.IsKeyUp(key);
    public static bool IsKeyJustPressed(Keys key) => _keyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    public static bool IsKeyJustReleased(Keys key) => _keyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);

    public static bool IsAnyKeyPressed()
    {
        return _keyboardState.GetPressedKeyCount() > 0;
    }
}