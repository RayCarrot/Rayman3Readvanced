namespace GbaMonoGame.Rayman3;

public partial class CaptureTheFlagItems
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Invincibility = 0,
        MagicShoes = 1,
        PlayerArrows = 2, // Unused
    }
}