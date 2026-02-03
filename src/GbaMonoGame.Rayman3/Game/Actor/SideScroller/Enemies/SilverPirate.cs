using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class SilverPirate : PirateBaseActor
{
    public SilverPirate(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        SpawnsRedLum = ActionId is Action.Init_HasRedLum_Left or Action.Init_HasRedLum_Right;
        ReInit();
    }

    public uint AttackTimer { get; set; }
    public uint DoubleHitTimer { get; set; }
    public float KnockBackYPosition { get; set; }
    public bool HighShot { get; set; }
    public bool QueueFallSound { get; set; } // Custom to prevent fall sounds from playing on level load when playing with all objects loaded

    private void Shoot()
    {
        EnergyBall energyBall = Scene.CreateProjectile<EnergyBall>(ActorType.EnergyBall);

        if (energyBall == null)
            return;

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser1_Mix02);

        float yOffset = HighShot ? -27 : -9;

        if (IsFacingRight)
        {
            energyBall.Position = Position + new Vector2(36, yOffset);
            energyBall.ActionId = EnergyBall.Action.Shot2_Right;
        }
        else
        {
            energyBall.Position = Position + new Vector2(-36, yOffset);
            energyBall.ActionId = EnergyBall.Action.Shot2_Left;
        }
    }

    protected override void ReInit()
    {
        DoubleHitTimer = 0;
        HighShot = false;
        AttackTimer = 0;
        State.SetTo(_Fsm_Fall);
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