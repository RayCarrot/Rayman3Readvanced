namespace GbaMonoGame.Rayman3;

public partial class Vines
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle_Down = 0,
        Idle_Up = 1,
        Extend_Down = 2,
        Extend_Up = 3,
        Attack_Down = 4,
        Attack_Up = 5,
        Retract_Down = 6,
        Retract_Up = 7,
    }
}