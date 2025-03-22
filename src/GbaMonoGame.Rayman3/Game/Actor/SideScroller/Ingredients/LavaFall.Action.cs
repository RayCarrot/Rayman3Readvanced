namespace GbaMonoGame.Rayman3;

public partial class LavaFall
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Flow = 0,
        BeginFlow = 1,
        EndFlow = 2,
    }
}