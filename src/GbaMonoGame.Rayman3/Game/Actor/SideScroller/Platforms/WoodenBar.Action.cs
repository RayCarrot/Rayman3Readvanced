namespace GbaMonoGame.Rayman3;

public partial class WoodenBar
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        SpeedUp = 1,
        ShakingFast = 2,
        SlowDown = 3,
    }
}