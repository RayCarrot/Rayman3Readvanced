namespace GbaMonoGame.Rayman3;

public partial class FlyingBombMode7
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Stationary = 0,
        MoveVertical_0 = 1,
        MoveVertical_32 = 2,
        MoveVertical_64 = 3,
        MoveVertical_96 = 4,
        MoveVertical_128 = 5,
        Move_0 = 6,
        Move_1 = 7,
        Drop = 8,
    }
}