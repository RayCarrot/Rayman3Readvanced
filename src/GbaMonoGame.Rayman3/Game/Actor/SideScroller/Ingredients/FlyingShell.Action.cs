namespace GbaMonoGame.Rayman3;

public partial class FlyingShell
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle_Right = 0,
        Idle_Left = 1,
        Fly_Right = 2,
        Fly_Left = 3,
        Crash_Right = 4,
        Crash_Left = 5,
        ChangeDirection_Right = 6,
        ChangeDirection_Left = 7,
        FlyUp_Right = 8,
        FlyUp_Left = 9,
        FlyDown_Left = 10,
        FlyDown_Right = 11,
    }
}