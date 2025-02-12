namespace GbaMonoGame.Rayman3;

public partial class BoulderMode7
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Stationary = 0,
        Move_Left = 1,
        Move_Right = 2,
        Move_Up = 3,
        Move_Down = 4,
        Bounce = 5,
    }
}