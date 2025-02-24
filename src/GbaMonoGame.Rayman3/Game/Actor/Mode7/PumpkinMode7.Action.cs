namespace GbaMonoGame.Rayman3;

public partial class PumpkinMode7
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        Break = 1,
    }
}