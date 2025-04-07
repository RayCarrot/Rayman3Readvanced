namespace GbaMonoGame.Rayman3;

public partial class Spider
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    // TODO: Name all actions
    public enum Action
    {
        Action0 = 0,
        Action1 = 1,
        Action2 = 2,
        Action3 = 3,
        Action4 = 4,
        Action5 = 5,
        Action6 = 6,
        Action7 = 7,
        Attack_Down = 8,
        Attack_Up = 9,
        Attack_Right = 10,
        Attack_Left = 11,
        Action12 = 12,
        Idle_Right = 13,
        Idle_Left = 14,
        Action15 = 15,
        Action16 = 16,
        Action17 = 17,
        Action18 = 18,
        Action19 = 19,
        Action20 = 20,
        Action21 = 21,
        Action22 = 22,
        Action23 = 23,
        RotateTo_Attack_Up = 24,
        Action25 = 25,
        Action26 = 26,
        RotateFrom_Attack_Down = 27,
        RotateTo_Attack_Down = 28,
        Action29 = 29,
        Action30 = 30,
        RotateFrom_Attack_Up = 31,
    }
}