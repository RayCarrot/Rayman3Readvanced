namespace GbaMonoGame.Rayman3;

public partial class LavaSplash
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        PlumLandedSplash = 1,
        Moving = 2,
        MainActorDrownSplash = 3,
    }
}