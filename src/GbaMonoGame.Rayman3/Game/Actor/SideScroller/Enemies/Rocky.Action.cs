namespace GbaMonoGame.Rayman3;

public partial class Rocky
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    // TODO: Name remaining actions
    public enum Action
    {
        IdlePhase1_Right = 0,
        IdlePhase1_Left = 1,
        Slam_Right = 2,
        Slam_Left = 3,
        Action4 = 4,
        Action5 = 5,
        Action6 = 6,
        Action7 = 7,
        Action8 = 8,
        Action9 = 9,
        Action10 = 10,
        Action11 = 11,
        Action12 = 12,
        Action13 = 13,
        IdlePhase2_Right = 14,
        IdlePhase2_Left = 15,
        IdlePhase3_Right = 16,
        IdlePhase3_Left = 17,
        Action18 = 18,
        Action19 = 19,
        Action20 = 20,
        Action21 = 21,
        Action22 = 22,
        Action23 = 23,
        Sleep_Right = 24,
        Sleep_Left = 25,
        PreparePunch_Right = 26,
        PreparePunch_Left = 27,
    }
}