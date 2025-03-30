namespace GbaMonoGame.Rayman3;

public partial class Boulder
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Roll_Right = 0,
        Roll_Left = 1,
        Fall = 2,
    }
}