using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

// Original name: CagoulardVolant
public sealed partial class Hoodstormer : MovableActor
{
    public Hoodstormer(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene,
        actorResource)
    {
        State.SetTo(Fsm_Wait);
    }

    private void ShootMissile()
    {
        EnergyBall energyBall = Scene.CreateProjectile<EnergyBall>(ActorType.EnergyBall);

        Debug.Assert(energyBall != null, "Cannot create EnergyBall projectile");

        if (energyBall != null)
        {
            if (IsFacingRight)
                energyBall.Position = Position + new Vector2(58, -8);
            else
                energyBall.Position = Position + new Vector2(-58, -8);

            energyBall.ActionId = IsFacingRight ? EnergyBall.Action.DownShot_Right : EnergyBall.Action.DownShot_Left;
            energyBall.ChangeAction();

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Laser3_Mix03);
        }
    }
}