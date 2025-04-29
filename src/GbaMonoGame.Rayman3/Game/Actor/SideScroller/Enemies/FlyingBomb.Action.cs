namespace GbaMonoGame.Rayman3;

public partial class FlyingBomb
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
        Wait = 4,
        Stationary = 5,
        Shake = 6,
        Attack = 7,
        Action_8 = 8, // Unused - only defined for the helicopter bombs and always the same as action 6
    }
}