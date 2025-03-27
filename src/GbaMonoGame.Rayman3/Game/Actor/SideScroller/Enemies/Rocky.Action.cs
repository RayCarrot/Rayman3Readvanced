namespace GbaMonoGame.Rayman3;

public partial class Rocky
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        IdlePhase1_Right = 0,
        IdlePhase1_Left = 1,
        Slam_Right = 2,
        Slam_Left = 3,
        Charge_Right = 4,
        Charge_Left = 5,
        Fall_Right = 6,
        Fall_Left = 7,
        Land_Right = 8,
        Land_Left = 9,
        PrepareCharge_Right = 10,
        PrepareCharge_Left = 11,
        Punch_Right = 12,
        Punch_Left = 13,
        IdlePhase2_Right = 14,
        IdlePhase2_Left = 15,
        IdlePhase3_Right = 16,
        IdlePhase3_Left = 17,
        Hit_Right = 18,
        Hit_Left = 19,
        UnusedHit_Right = 20, // Unused
        UnusedHit_Left = 21, // Unused
        Dying_Right = 22,
        Dying_Left = 23,
        Sleep_Right = 24,
        Sleep_Left = 25,
        PreparePunch_Right = 26,
        PreparePunch_Left = 27,
    }
}