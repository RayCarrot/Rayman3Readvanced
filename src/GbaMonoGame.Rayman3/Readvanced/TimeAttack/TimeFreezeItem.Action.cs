namespace GbaMonoGame.Rayman3.Readvanced;

public partial class TimeFreezeItem
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Init_Blue = 0,
        Init_Orange = 1,
        Idle = 2,
        Dying = 3,
    }
}