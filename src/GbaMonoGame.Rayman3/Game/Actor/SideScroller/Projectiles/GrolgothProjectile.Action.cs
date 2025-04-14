namespace GbaMonoGame.Rayman3;

public partial class GrolgothProjectile
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    // TODO: Name
    public enum Action
    {
        SmallGroundBomb_Right = 0,
        SmallGroundBomb_Left = 1,
        SmallGroundBombReverse_Right = 2,
        SmallGroundBombReverse_Left = 3,
        EnergyBall_Right = 4,
        EnergyBall_Left = 5,
        Laser_Right = 6,
        Laser_Left = 7,
        BigGroundBomb_Right = 8,
        BigGroundBomb_Left = 9,
        FallingBomb = 10,
        Action11 = 11,
        Action12 = 12,
        Action13 = 13,
        Action14 = 14,
        Action15 = 15,
        Action16 = 16,
        Action17 = 17,
    }
}