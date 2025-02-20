using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class RedPirate : PirateBaseActor
{
    public RedPirate(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        SpawnsRedLum = true;
        ReInit();
    }

    public uint AttackTimer { get; set; }
    public uint DoubleHitTimer { get; set; }
    public Vector2 KnockBackPosition { get; set; }
    public int Ammo { get; set; } // Ammo functionality appears unused
    public bool HasFiredShot { get; set; }

    private void Walk()
    {
        if (IsFacingRight)
        {
            if (Scene.GetPhysicalType(Position - new Vector2(0, Tile.Size)) == PhysicalTypeValue.Enemy_Left)
                ActionId = Action.Walk_Left;
        }
        else
        {
            if (Scene.GetPhysicalType(Position - new Vector2(0, Tile.Size)) == PhysicalTypeValue.Enemy_Right)
                ActionId = Action.Walk_Right;
        }
    }

    private void Shoot()
    {
        Ammo--;

        EnergyBall energyBall = Scene.CreateProjectile<EnergyBall>(ActorType.EnergyBall);

        if (energyBall == null) 
            return;
        
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser3_Mix03);

        if (IsFacingRight)
        {
            energyBall.Position = Position + new Vector2(28, -32);
            energyBall.ActionId = EnergyBall.Action.Shot1_Right;
        }
        else
        {
            energyBall.Position = Position + new Vector2(-28, -32);
            energyBall.ActionId = EnergyBall.Action.Shot1_Left;
        }

        energyBall.ChangeAction();
    }

    protected override void ReInit()
    {
        Ammo = 0;
        AttackTimer = 0;
        DoubleHitTimer = 0;
        State.SetTo(Fsm_Fall);
    }
}