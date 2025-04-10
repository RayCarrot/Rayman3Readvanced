namespace GbaMonoGame.Rayman3;

public partial class BluePirate
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
        AttackExtend_Left = 2,
        AttackExtend_Right = 3,
        AttackWait_Left = 4,
        AttackWait_Right = 5,
        AttackRetract_Left = 6,
        AttackRetract_Right = 7,
        Hit_Left = 8,
        Hit_Right = 9,
        HitBehind_Left = 10,
        HitBehind_Right = 11,
        Dying_Left = 12,
        Dying_Right = 13,
        Init_HasRedLum_Left = 14,
        Init_HasRedLum_Right = 15,
        Fall_Left = 16,
        Fall_Right = 17,
        Land_Left = 18,
        Land_Right = 19,
    }
}