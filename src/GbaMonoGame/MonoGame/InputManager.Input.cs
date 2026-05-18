using System;

namespace GbaMonoGame;

public partial class InputManager
{
    private static readonly Input[] _allInputs = Enum.GetValues<Input>();

    private Input _previousInputs;
    private Input _inputs;

    public InputMode InputMode { get; private set; }

    private void UpdateInput()
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

    public bool IsInputPressed(Input input) => (_inputs & input) != 0;
    public bool IsInputReleased(Input input) => (_inputs & input) == 0;
    public bool IsInputJustPressed(Input input) => (_inputs & input) != 0 && (_previousInputs & input) == 0;
    public bool IsInputJustReleased(Input input) => (_inputs & input) == 0 && (_previousInputs & input) != 0;
}