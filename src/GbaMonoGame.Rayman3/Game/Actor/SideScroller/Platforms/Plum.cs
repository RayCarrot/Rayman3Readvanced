using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class Plum : MovableActor
{
    public Plum(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Resource = actorResource;

        FloatSpeedX = 0;
        DisableMessages = false;
        LavaSplash = null;
        ShouldSetInitialSpeed = true;

        if ((Action)actorResource.FirstActionId == Action.Fall)
            State.SetTo(Fsm_Fall);
        else
            State.SetTo(Fsm_Idle);
    }

    private const uint MinChargePower = 2;
    private const uint MaxChargePower = 180;

    public ActorResource Resource { get; }
    public LavaSplash LavaSplash { get; set; }

    public float FloatSpeedX { get; set; }
    public float BounceSpeedX { get; set; }
    public ushort Timer { get; set; }
    public uint ChargePower { get; set; }
    public bool DisableMessages { get; set; }
    public bool ShouldSetInitialSpeed { get; set; }

    private void SpawnLavaSplash()
    {
        if (LavaSplash != null)
        {
            LavaSplash.ProcessMessage(this, Message.Resurrect);
            LavaSplash.LinkedMovementActor = this;
            LavaSplash.Position = Position;
        }
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        if (DisableMessages)
            return false;

        switch (message)
        {
            case Message.Plum_HitRight:
                ChargePower = Math.Clamp((uint)param, MinChargePower, MaxChargePower);

                FloatSpeedX -= (uint)param / 8f + 1;
                if (FloatSpeedX < -6)
                    FloatSpeedX = -6;
                return false;

            case Message.Plum_HitLeft:
                ChargePower = Math.Clamp((uint)param, MinChargePower, MaxChargePower);

                FloatSpeedX += (uint)param / 8f + 1;
                if (FloatSpeedX > 6)
                    FloatSpeedX = 6;
                return false;

            case Message.Hit:
                if (ActionId == Action.Grow)
                {
                    ActionId = Action.Hit;
                    ChangeAction();
                }

                BounceSpeedX = ((MovableActor)param).Speed.X / 4;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Boing_Mix02);
                return false;

            default:
                return false;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        if (actorResource.Links[0] != null)
            LavaSplash = Scene.GetGameObject<LavaSplash>(actorResource.Links[0].Value);

        base.Init(actorResource);
    }
}