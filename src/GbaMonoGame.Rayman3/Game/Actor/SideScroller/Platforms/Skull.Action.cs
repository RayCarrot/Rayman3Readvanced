namespace GbaMonoGame.Rayman3;

public partial class Skull
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        SolidMove_Stationary = 0,

        Move_Left = 1,
        Move_Right = 2,
        Move_Up = 3,
        Move_Down = 4,

        // Unused - broken animations in final build
        SpinStart = 5,
        Spin_6 = 6,
        Spin_7 = 7,
        
        Stationary = 8,
        Spawn = 9,
        StationaryShake = 10,
        FallDown = 11, // Unused
        Despawn = 12,

        SolidMove_Left = 13,
        SolidMove_Right = 14,
        SolidMove_Wait = 15,
    }
}