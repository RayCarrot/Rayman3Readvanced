namespace GbaMonoGame.Rayman3;

public partial class Cage
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        GroundedIdle = 0,
        GroundedBlink = 1,
        GroundedHitRight = 2,
        GroundedBlinkDamaged = 3,
        GroundedBreak = 4,
        GroundedHitLeft = 5,

        HangingIdle = 6,
        HangingBlink = 7,
        HangingHitRight = 8,
        HangingBlinkDamaged = 9,
        HangingBreak = 10,
        HangingHitLeft = 11,
    }
}