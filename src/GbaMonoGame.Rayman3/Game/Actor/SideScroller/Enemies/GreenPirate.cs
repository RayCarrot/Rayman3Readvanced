using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public sealed partial class GreenPirate : PirateBaseActor
{
    public GreenPirate(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        SpawnsRedLum = ActionId is Action.Init_HasRedLum_Right or Action.Init_HasRedLum_Left;
        ReInit();
    }

    public float KnockBackYPosition { get; set; }
    public bool QueueFallSound { get; set; } // Custom to prevent fall sounds from playing on level load when playing with all objects loaded

    private void Shoot(bool highShot)
    {
        EnergyBall energyBall = Scene.CreateProjectile<EnergyBall>(ActorType.EnergyBall);

        if (energyBall == null)
            return;

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser2_Mix02);

        float yOffset = highShot ? -32 : -16;

        if (IsFacingRight)
        {
            energyBall.Position = Position + new Vector2(16, yOffset);
            energyBall.ActionId = EnergyBall.Action.Shot3_Right;
        }
        else
        {
            energyBall.Position = Position + new Vector2(-16, yOffset);
            energyBall.ActionId = EnergyBall.Action.Shot3_Left;
        }

        energyBall.ChangeAction();
    }

    protected override void ReInit()
    {
        State.SetTo(Fsm_Fall);
    }

    public override void Step()
    {
        base.Step();

        // Custom to prevent fall sounds from playing on level load when playing with all objects loaded
        if (QueueFallSound && AnimatedObject.IsFramed)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PiraJump_BigFoot1_Mix02);
            QueueFallSound = false;
        }
    }
}