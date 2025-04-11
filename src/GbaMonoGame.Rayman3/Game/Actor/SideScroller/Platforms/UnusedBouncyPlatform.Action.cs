namespace GbaMonoGame.Rayman3;

public partial class UnusedBouncyPlatform
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        Action1 = 1, // Unused
        Bounce = 2,
        DealDamage = 3,
    }
}