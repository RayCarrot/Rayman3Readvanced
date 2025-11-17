using System;

namespace GbaMonoGame;

public static partial class InputManager
{
    private static readonly Input[] _allInputs = Enum.GetValues<Input>();

    private static Input _previousInputs;
    private static Input _inputs;

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

        // Update inputs
        _previousInputs = _inputs;
        _inputs = InputMode switch
        {
            InputMode.Keyboard => GetInputsFromKeyboard(),
            InputMode.GamePad => GetInputsFromGamePad(),
            _ => throw new InvalidOperationException($"Input mode {InputMode} is not supported")
        };
    }

    public static bool IsInputPressed(Input input) => (_inputs & input) != 0;
    public static bool IsInputReleased(Input input) => (_inputs & input) == 0;
    public static bool IsInputJustPressed(Input input) => (_inputs & input) != 0 && (_previousInputs & input) == 0;
    public static bool IsInputJustReleased(Input input) => (_inputs & input) == 0 && (_previousInputs & input) != 0;
}