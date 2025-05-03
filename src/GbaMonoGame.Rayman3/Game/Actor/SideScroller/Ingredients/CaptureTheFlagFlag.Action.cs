namespace GbaMonoGame.Rayman3;

public partial class CaptureTheFlagFlag
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Fall_Right = 0,
        Fall_Left = 1,
        Idle_Right = 2,
        Idle_Left = 3,
    }
}