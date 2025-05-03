namespace GbaMonoGame.Rayman3;

public partial class CaptureTheFlagFlagBase
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        Shine = 1,
    }
}