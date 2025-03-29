namespace GbaMonoGame.Rayman3;

public partial class Spinneroo
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle_Left = 0,
        Idle_Right = 1,
        Walk_Left = 2,
        Walk_Right = 3,
        Unused1_Left = 4, // Unused
        Unused1_Right = 5, // Unused
        Attack1_Left = 6,
        Attack1_Right = 7,
        Unused2_Left = 8, // Unused
        Unused2_Right = 9, // Unused
        BeginFall_Left = 10,
        BeginFall_Right = 11,
        Fall_Left = 12,
        Fall_Right = 13,
        Dying_Left = 14,
        Dying_Right = 15,
        BeginAttack_Left = 16,
        BeginAttack_Right = 17,
        Attack2_Left = 18,
        Attack2_Right = 19,
        Taunt_Left = 20,
        Taunt_Right = 21,
    }
}