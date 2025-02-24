namespace GbaMonoGame.Rayman3;

public partial class ExplosionMode7
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Explode = 0,
    }
}