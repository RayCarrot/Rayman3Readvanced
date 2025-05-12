namespace GbaMonoGame.Rayman3;

public partial class Urchin
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle1 = 0,
        Idle2 = 1,
        Idle3 = 2,
    }
}