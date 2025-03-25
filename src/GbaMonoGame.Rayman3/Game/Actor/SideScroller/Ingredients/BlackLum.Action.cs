namespace GbaMonoGame.Rayman3;

public partial class BlackLum
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
        PrepareAttack_Left = 2,
        PrepareAttack_Right = 3,
        Fly_Left = 4,
        Fly_Right = 5,
        Dying_Left = 6,
        Dying_Right = 7,
    }
}