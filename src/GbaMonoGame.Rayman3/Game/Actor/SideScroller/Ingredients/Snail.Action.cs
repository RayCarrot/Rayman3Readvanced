namespace GbaMonoGame.Rayman3;

public partial class Snail
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Move_Left = 0,
        Move_Right = 1,
        TurnAround_Left = 2,
        TurnAround_Right = 3,
    }
}