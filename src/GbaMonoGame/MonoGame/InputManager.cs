namespace GbaMonoGame;

public partial class InputManager
{
    public void Update()
    {
        UpdateKeyboard();
        UpdateMouse();
        UpdateGamePad();
        UpdateInput();
    }
}