namespace GbaMonoGame.Rayman3;

public partial class Barrel
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        IdleFloat = 1,
        LandInWater = 2,
        BeginHitImpact = 3,
        Break = 4, // Broken in final game
        FallRight = 5,
        FallLeft = 6, // Broken in final game
        FloatRight = 7,
        FloatLeft = 8,
        HitImpact = 9,
        FallIntoWater1 = 10,
        FallIntoWater2 = 11,
        FallIntoWater3 = 12,
    }
}