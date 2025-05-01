namespace GbaMonoGame.Rayman3;

public partial class SpikyFlyingBomb
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Move_Left = 0,
        Move_Right = 1,
        Move_Up = 2,
        Move_Down = 3,
        WaitToAttack = 4,
        Stationary = 5,
        Attack = 6,
        Action_7 = 7, // Unused
    }
}