using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.SinglePak;

public abstract class SinglePakActor
{
    protected SinglePakActor(FrameSinglePak singlePak, int machineId)
    {
        SinglePak = singlePak;
        MachineId = machineId;
        field_0x44 = Box.Empty;
        IsActive = true;
    }

    public FrameSinglePak SinglePak { get; }
    public FiniteStateMachine State { get; } = new();
    public AnimatedObject AnimatedObject { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Speed { get; set; }
    public Box field_0x44 { get; set; }
    public byte field_0x4c { get; set; }
    public int MachineId { get; set; }
    public bool IsActive { get; set; }

    public void Step()
    {
        State.Step();
    }

    public virtual void Draw(AnimationPlayer animationPlayer)
    {

    }
}