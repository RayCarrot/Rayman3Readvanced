namespace GbaMonoGame.Rayman3;

public partial class LumsMode7
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        YellowLum = 0,
        BlueLum = 1,
        RedLum = 2,
    }
}