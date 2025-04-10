namespace GbaMonoGame.Rayman3;

public partial class Electricity
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        SingleActivated_Left = 0,
        SingleActivated_Right = 1, 
        DoubleActivated_Left = 2,
        DoubleActivated_Right = 3,
        SingleDeactivated_Left = 4,
        SingleDeactivated_Right = 5,
        DoubleDeactivated_Left = 6,
        DoubleDeactivated_Right = 7,
    }
}