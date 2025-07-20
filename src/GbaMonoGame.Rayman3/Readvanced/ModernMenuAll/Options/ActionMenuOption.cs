namespace GbaMonoGame.Rayman3.Readvanced;

public class ActionMenuOption : TextMenuOption
{
    public ActionMenuOption(string text, System.Action action, float scale = 1) : base(text, scale)
    {
        Action = action;
    }

    public System.Action Action { get; }

    public void Invoke()
    {
        Action();
    }
}