namespace GbaMonoGame.Rayman3;

public partial class Wall
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Action0 = 0,
        Action1 = 1,
        Action2 = 2,
        Action3 = 3,
        Action4 = 4,
        Action5 = 5,
        Action6 = 6,
    }
}