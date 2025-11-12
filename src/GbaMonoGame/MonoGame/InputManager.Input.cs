using System;

namespace GbaMonoGame;

public static partial class InputManager
{
    public static InputMode InputMode { get; private set; }

    private static void UpdateInput()
    {
        // Switch input mode based on last detected input
        if (InputMode != InputMode.GamePad && IsGamePadConnected && IsAnyButtonPressed())
        {
            InputMode = InputMode.GamePad;
            Logger.Info("Switched input mode to GamePad");
        }
        else if (InputMode != InputMode.Keyboard && IsAnyKeyPressed())
        {
            InputMode = InputMode.Keyboard;
            Logger.Info("Switched input mode to Keyboard");
        }
    }

    public static bool IsInputPressed(Input input) => InputMode switch
    {
        InputMode.Keyboard => IsKeyPressed(GetKey(input)),
        InputMode.GamePad => IsButtonPressed(GetButton(input)),
        _ => throw new InvalidOperationException($"Input mode {InputMode} is not supported")
    };
    public static bool IsInputReleased(Input input) => InputMode switch
    {
        InputMode.Keyboard => IsKeyReleased(GetKey(input)),
        InputMode.GamePad => IsButtonReleased(GetButton(input)),
        _ => throw new InvalidOperationException($"Input mode {InputMode} is not supported")
    };
    public static bool IsInputJustPressed(Input input) => InputMode switch
    {
        InputMode.Keyboard => IsKeyJustPressed(GetKey(input)),
        InputMode.GamePad => IsButtonJustPressed(GetButton(input)),
        _ => throw new InvalidOperationException($"Input mode {InputMode} is not supported")
    };
    public static bool IsInputJustReleased(Input input) => InputMode switch
    {
        InputMode.Keyboard => IsKeyJustReleased(GetKey(input)),
        InputMode.GamePad => IsButtonJustReleased(GetButton(input)),
        _ => throw new InvalidOperationException($"Input mode {InputMode} is not supported")
    };
}