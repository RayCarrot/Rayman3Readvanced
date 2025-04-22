using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class EnergyBall
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // ???
                ScreenPosition = ScreenPosition with { X = 0 };
                Speed = Speed with { X = MathHelpers.FromFixedPoint(1) };
                break;

            case FsmAction.Step:
                bool finished = (ScreenPosition.X >= Scene.Resolution.X && Speed.X > 0) || 
                                (ScreenPosition.X < 0 && Speed.X < 0) ||
                                Speed.X == 0 ||
                                // TODO: Have these match resolution? In the original game they're the same on both
                                //       GBA and N-Gage. Though they seem to be GBA res with a margin of 10.
                                ScreenPosition.Y <= -10 ||
                                ScreenPosition.Y >= 170;

                if (ActionId is Action.Shot1Enemy_Right or Action.Shot1Enemy_Left)
                {
                    InteractableActor hitActor = Scene.IsHitActor(this);
                    if (hitActor != null && hitActor != Scene.MainActor)
                    {
                        hitActor.ReceiveDamage(AttackPoints);
                        hitActor.ProcessMessage(this, Message.Actor_Hurt);
                        Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
                        hitActor.ProcessMessage(this, Message.Actor_Hit);
                        
                        if (explosion != null)
                            explosion.Position = Position;
                    }
                }
                else
                {
                    if (Scene.IsHitMainActor(this) && !Scene.MainActor.IsInvulnerable)
                    {
                        Scene.MainActor.ReceiveDamage(AttackPoints);
                        Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                        
                        if (explosion != null)
                            explosion.Position = Position;
                    }
                }

                if (finished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // ???
                ScreenPosition = ScreenPosition with { X = 0 };
                Speed = Speed with { X = MathHelpers.FromFixedPoint(1) };

                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}