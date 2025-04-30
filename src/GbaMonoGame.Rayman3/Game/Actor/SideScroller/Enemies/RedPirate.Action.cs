namespace GbaMonoGame.Rayman3;

public partial class RedPirate
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle_Right = 0,
        Idle_Left = 1,
        Walk_Right = 2,
        Walk_Left = 3,
        Action_4 = 4, // Unused
        Action_5 = 5, // Unused
        Action_6 = 6, // Unused
        Action_7 = 7, // Unused
        Action_8 = 8, // Unused
        Action_9 = 9, // Unused
        Dying_Right = 10,
        Dying_Left = 11,
        DyingBehind_Right = 12,
        DyingBehind_Left = 13,
        Shoot_Right = 14,
        Shoot_Left = 15,
        Action_16 = 16, // Unused
        Action_17 = 17, // Unused
        Fall_Right = 18,
        Fall_Left = 19,
        Land_Right = 20,
        Land_Left = 21,
        Hit_Right = 22,
        Hit_Left = 23,
        HitBehind_Right = 24,
        HitBehind_Left = 25,
        Action_26 = 26, // Unused
        Action_27 = 27, // Unused
        HitKnockBack_Right = 28,
        HitKnockBack_Left = 29,
    }
}