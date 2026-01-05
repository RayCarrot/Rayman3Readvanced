namespace GbaMonoGame.Rayman3;

public partial class CheatDialog
{
    public bool Fsm_NavigateItem(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
                {
                    int selectedIndex = SelectedIndex + 1;
                    if (selectedIndex >= CheatItems.Length)
                        selectedIndex = 0;

                    SetSelectedIndex(selectedIndex);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
                {
                    int selectedIndex = SelectedIndex - 1;
                    if (selectedIndex < 0)
                        selectedIndex = CheatItems.Length - 1;

                    SetSelectedIndex(selectedIndex);
                }
                else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
                {
                    CheatItems[SelectedIndex].Invoke();
                    PendingClose = true;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}