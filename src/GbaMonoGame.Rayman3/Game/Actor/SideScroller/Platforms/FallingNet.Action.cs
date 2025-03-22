namespace GbaMonoGame.Rayman3;

public partial class FallingNet
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        Fall = 1,
        Action2 = 2, // Unused
        Shake = 3,
    }
}