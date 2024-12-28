namespace GbaMonoGame.Rayman3;

public partial class ItemsMulti
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Globox = 0,
        Reverse = 1,
        Invisibility = 2,
        Fist = 3,
        Random = 4,
    }
}