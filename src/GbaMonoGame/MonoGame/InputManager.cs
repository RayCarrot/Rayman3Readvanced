namespace GbaMonoGame;

public static partial class InputManager
{
    public static void Update()
    {
        UpdateKeyboard();
        UpdateMouse();
        UpdateGamePad();
        UpdateInput();
    }
}