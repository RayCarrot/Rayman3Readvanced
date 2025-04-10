namespace GbaMonoGame.Rayman3;

public partial class Scaleman
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
        CreateRedLum_Right = 2,
        CreateRedLum_Left = 3,
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
        Ball_FlyUp_Right = 18,
        Ball_FlyUp_Left = 19,
        Ball_FlyDown_Right = 20,
        Ball_FlyDown_Left = 21,
        Ball_Land_Right = 22,
        Ball_Land_Left = 23,
        Emerge_Right = 24,
        Emerge_Left = 25,
        Small_Idle_Right = 26,
        Small_Idle_Left = 27,
        Small_Run_Right = 28,
        Small_Run_Left = 29,
        Small_RunFast_Right = 30,
        Small_RunFast_Left = 31,
        Small_ChangeDirection_Right = 32,
        Small_ChangeDirection_Left = 33,
        Small_Hop_Right = 34,
        Small_Hop_Left = 35,
        Small_Hit_Right = 36,
        Small_Hit_Left = 37,
        Grow_Right = 38,
        Grow_Left = 39,
        Hop_Right = 40,
        Hop_Left = 41,
        Dying_Right = 42,
        Dying_Left = 43,
    }
}