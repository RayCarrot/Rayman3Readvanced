namespace GbaMonoGame.Rayman3;

public partial class Plum
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        BounceGround = 1,
        Hit = 2,
        Fall = 3,
        Float = 4,
        Grow = 5,
        BounceAir = 6,
        FloatAngle = 7,
    }
}