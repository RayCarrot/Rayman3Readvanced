namespace GbaMonoGame.Rayman3;

public partial class WalkingShell
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle = 0,
        PrepareWalk = 1, // Unused
        Walk = 2,
        ShortBoost = 3,
        Loop = 4,
        Jump = 5,
        Action6 = 6, // Unused
        Action7 = 7, // Unused
        Action8 = 8, // Unused
        BeginBoost = 9,
        EndBoost = 10,
        Mounting = 11,
        LongBoost = 12,
    }
}