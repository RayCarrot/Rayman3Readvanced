using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Grenade
{
    private bool FsmStep_CheckHitMainActor()
    {
        if (Scene.IsHitMainActor(this))
            Scene.MainActor.ReceiveDamage(AttackPoints);

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                TouchingMapTimer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckHitMainActor())
                    return false;

                if (TouchingMapTimer != 0)
                    TouchingMapTimer++;

                if (TouchingMapTimer == 0 && IsTouchingMap)
                {
                    TouchingMapTimer++;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Grenad01_Mix03);
                }

                PhysicalType type = Scene.GetPhysicalType(Position + new Vector2(0, 8));
                if (type == PhysicalTypeValue.InstaKill || 
                    type == PhysicalTypeValue.MoltenLava || 
                    Scene.IsHitMainActor(this) ||
                    TouchingMapTimer >= 30)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
                
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);
                
                if (explosion != null)
                    explosion.Position = Position;

                IsTouchingMap = false;
                ProcessMessage(this, Message.Destroy);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Grenad01_Mix03);
                break;
        }

        return true;
    }
}