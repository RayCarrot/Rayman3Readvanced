namespace GbaMonoGame.Rayman3;

public partial class GreenPirate
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
        ShootHigh_Left = 2,
        ShootHigh_Right = 3,
        ShootLow_Left = 4,
        ShootLow_Right = 5,
        Hit_Left = 6,
        Hit_Right = 7,
        HitBehind_Left = 8,
        HitBehind_Right = 9,
        Dying_Left = 10,
        Dying_Right = 11,
        HitKnockBack_Left = 12,
        HitKnockBack_Right = 13,
        Init_HasRedLum_Left = 14,
        Init_HasRedLum_Right = 15,
        Fall_Left = 16,
        Fall_Right = 17,
        Land_Left = 18,
        Land_Right = 19,
    }
}