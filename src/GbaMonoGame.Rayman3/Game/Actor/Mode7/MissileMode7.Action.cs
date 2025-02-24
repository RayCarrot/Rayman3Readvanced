namespace GbaMonoGame.Rayman3;

public partial class MissileMode7
{
    private const int ActionRotationSize = 6;

    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Default = 0,
    }
}