using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Boulder : MovableActor
{
    public Boulder(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        if ((Action)actorResource.FirstActionId == Action.Roll_Right)
        {
            IsMovingRight = true;
            MoveSpeed = MathHelpers.FromFixedPoint(0x3333);
        }
        else if ((Action)actorResource.FirstActionId == Action.Roll_Left)
        {
            IsMovingRight = false;
            MoveSpeed = -MathHelpers.FromFixedPoint(0x3333);
        }

        MechModel.Speed = MechModel.Speed with { X = 0 };
        
        Timer = 0xFF;
        PendingShake = true;

        State.SetTo(Fsm_Wait);
    }

    private float MoveSpeed { get; set; }
    private Angle256 Rotation { get; set; }
    private byte Timer { get; set; }
    private bool IsMovingRight { get; set; }
    private bool PendingShake { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_Start:
                if (State == Fsm_Wait)
                {
                    Timer = 0;

                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__rockchase))
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__rockchase, 3);
                }
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (State != Fsm_Wait)
            base.Draw(animationPlayer, forceDraw);
    }
}