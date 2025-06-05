using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.SinglePak;

public class Player : SinglePakActor
{
    public Player(FrameSinglePak singlePak, int machineId) : base(singlePak, machineId)
    {
        field_0x44 = new Box(-12, -6, 12, 6);
        AnimatedObject = new AnimatedObject(SinglePak.LoadResource<AnimatedObjectResource>(0), false)
        {
            AffineMatrix = AffineMatrix.Identity,
            IsDoubleAffine = true,
            CurrentAnimation = 0,
            BgPriority = 0,
            ObjPriority = 16,
            RenderContext = SinglePak.RenderContext,
        };

        ScoreFrame = new AnimatedObject(SinglePak.LoadResource<AnimatedObjectResource>(3), false)
        {
            ScreenPos = MachineId == 0 ? new Vector2(0, 23) : new Vector2(-65, 23),
            HorizontalAnchor = MachineId == 0 ? HorizontalAnchorMode.Left : HorizontalAnchorMode.Right,
            CurrentAnimation = MachineId == 0 ? 10 : 11,
            RenderContext = SinglePak.RenderContext,
        };
        ScoreDigit = new AnimatedObject(SinglePak.LoadResource<AnimatedObjectResource>(3), false)
        {
            ScreenPos = MachineId == 0 ? new Vector2(22, 20) : new Vector2(-43, 20),
            HorizontalAnchor = MachineId == 0 ? HorizontalAnchorMode.Left : HorizontalAnchorMode.Right,
            CurrentAnimation = 0,
            RenderContext = SinglePak.RenderContext,
        };

        Reset(true);

        // TODO: Implement
        //BLDALPHA = 0xf01;
        //BLDCNT = 0x3f40;
    }

    public AnimatedObject ScoreFrame { get; }
    public AnimatedObject ScoreDigit { get; }

    public void Reset(bool param)
    {
        // TODO: Implement
    }
}