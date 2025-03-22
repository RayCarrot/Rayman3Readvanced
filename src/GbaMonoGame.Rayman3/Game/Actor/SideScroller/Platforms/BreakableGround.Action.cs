namespace GbaMonoGame.Rayman3;

public partial class BreakableGround
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Idle_Default = 0,
        Destroyed = 1,
        Idle_QuickFinishBodyShotAttack = 2, // Used when the boulder chases you
        Idle_World = 3, // Used in hub world and gets removed once you progress further in the game
    }
}