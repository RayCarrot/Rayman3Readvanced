namespace GbaMonoGame.Rayman3;

public partial class RockyFlame
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Smoke = 0,
        Flame = 1,
    }
}