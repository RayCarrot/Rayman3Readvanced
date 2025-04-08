namespace GbaMonoGame.Rayman3;

public partial class Scaleman
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    // TODO: Name
    public enum Action
    {
        Idle_Right = 0,
        Idle_Left = 1,
        Action2 = 2,
        Action3 = 3,
        Hit_Right = 4,
        HitBehind_Left = 5,
        HitBehind_Right = 6,
        Hit_Left = 7,
        Submerge_Right = 8,
        Submerge_Left = 9,
        Shrink_Right = 10,
        Shrink_Left = 11,
        Ball_Bounce_Right = 12,
        Ball_Bounce_Left = 13,
        Ball_Roll_Right = 14,
        Ball_Roll_Left = 15,
        Ball_RollFast_Right = 16,
        Ball_RollFast_Left = 17,
        Action18 = 18,
        Action19 = 19,
        Action20 = 20,
        Action21 = 21,
        Action22 = 22,
        Action23 = 23,
        Emerge_Right = 24,
        Emerge_Left = 25,
        Small_Idle_Right = 26,
        Small_Idle_Left = 27,
        Small_Run_Right = 28,
        Small_Run_Left = 29,
        Small_RunFast_Right = 30,
        Small_RunFast_Left = 31,
        Action32 = 32,
        Action33 = 33,
        Action34 = 34,
        Action35 = 35,
        Action36 = 36,
        Action37 = 37,
        Action38 = 38,
        Action39 = 39,
        Action40 = 40,
        Action41 = 41,
        Action42 = 42,
        Action43 = 43,
    }
}