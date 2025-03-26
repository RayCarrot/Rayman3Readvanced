namespace GbaMonoGame.Rayman3;

public partial class FallingBridge
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle_Right = 0,
        Shake_Right = 1,
        Fall_Right = 2,
        Break_Right = 3,
        Idle_Left = 4,
        Shake_Left = 5,
        Fall_Left = 6,
        Break_Left = 7,
    }
}