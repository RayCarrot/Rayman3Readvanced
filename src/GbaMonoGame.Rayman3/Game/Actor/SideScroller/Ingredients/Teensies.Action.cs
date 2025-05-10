namespace GbaMonoGame.Rayman3;

public partial class Teensies
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Init_Victory_Left = 0,
        Init_Victory_Right = 1,
        Victory1_Left = 2,
        Victory1_Right = 3,
        Victory2_Left = 4,
        Victory2_Right = 5,

        Idle1_Right = 6,
        Idle1_Left = 7,
        Idle2_Right = 8,
        Idle2_Left = 9,
        Idle3_Right = 10,
        Idle3_Left = 11,
        Idle4_Right = 12,
        Idle4_Left = 13,
        Idle5_Right = 14,
        Idle5_Left = 15,
        Idle6_Right = 16,
        Idle6_Left = 17,

        Init_Master_Left = 18,
        Init_Master_Right = 19,
        Master1_Left = 20,
        Master1_Right = 21,
        Master2_Left = 22,
        Master2_Right = 23,
        Master3_Left = 24,
        Master3_Right = 25,
        Master4_Left = 26,
        Master4_Right = 27,
        Master5_Left = 28,
        Master5_Right = 29,

        Init_World1_Left = 30,
        Init_World1_Right = 31,
        Init_World2_Left = 32,
        Init_World2_Right = 33,
        Init_World3_Left = 34,
        Init_World3_Right = 35,
        Init_World4_Left = 36,
        Init_World4_Right = 37,

        ShowRequirementMet_Left = 38,
        ShowRequirementMet_Right = 39,
        ShowRequirementNotMet_Right = 40,
        ShowRequirementNotMet_Left = 41,
    }
}