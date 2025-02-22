namespace GbaMonoGame.Rayman3;

public partial class WaterSplashMode7
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Splash = 0,
        SurfWaves = 1,
    }
}