namespace GbaMonoGame.Rayman3;

public partial class Balloon
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Inflate = 0,
        Idle = 1,
        Pop = 2,
        Init_NotRespawnable = 3,
        Init_TimedNotRespawnable = 4,
    }
}