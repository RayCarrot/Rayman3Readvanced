namespace GbaMonoGame.Rayman3;

public partial class Spider
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Chase_Right = 0,
        Chase_Left = 1,
        Action2 = 2, // Unused
        Action3 = 3, // Unused
        Action4 = 4, // Unused
        Action5 = 5, // Unused
        Action6 = 6, // Unused
        Action7 = 7, // Unused
        Attack_Down = 8,
        Attack_Up = 9,
        Attack_Right = 10,
        Attack_Left = 11,
        Stop_Down = 12,
        Idle_Right = 13,
        Idle_Left = 14,
        Action15 = 15, // Unused
        Climb_Right = 16,
        Climb_Left = 17,
        Climb_Down = 18,
        Climb_Up = 19,
        Stop_Left = 20,
        Stop_Right = 21,
        Stop_Up = 22,
        Action23 = 23, // Unused
        RotateTo_Attack_Up = 24,
        Action25 = 25, // Unused
        Action26 = 26, // Unused
        RotateFrom_Attack_Down = 27,
        RotateTo_Attack_Down = 28,
        Action29 = 29, // Unused
        Action30 = 30, // Unused
        RotateFrom_Attack_Up = 31,
    }
}