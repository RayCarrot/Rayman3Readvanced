namespace GbaMonoGame.Rayman3;

public partial class Leaf
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Leaf1_1 = 0,
        Leaf1_2 = 1,
        Leaf1_3 = 2,
        Leaf2_1 = 3,
        Leaf2_2 = 4,
        Leaf2_3 = 5,
        Leaf3_1 = 6,
        Leaf3_2 = 7,
        Leaf3_3 = 8,
    }
}