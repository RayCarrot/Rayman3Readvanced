using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class RockyFlame
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                ActionId = Action.Smoke;
                AnimatedObject.ObjPriority = 15;
                break;

            case FsmAction.Step:
                if (ActionId == Action.Smoke)
                    Timer++;

                if (Timer >= 2 && IsActionFinished && ActionId == Action.Smoke)
                {
                    ActionId = Action.Flame;
                    AnimatedObject.ObjPriority = 17;
                    ChangeAction();
                }
                else if (Timer <= 1 && ActionId != Action.Smoke)
                {
                    ActionId = Action.Smoke;
                    ChangeAction();
                }

                if (Scene.IsHitMainActor(this) && !Scene.MainActor.IsInvulnerable)
                {
                    Scene.MainActor.ReceiveDamage(1);
                    Timer = 0xFF;
                }

                if (Timer == 0xFF)
                {
                    if (Scene.MainActor.Position.Y <= 100)
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Actor_HurtKnockback);
                        Timer = 2;
                    }
                    else
                    {
                        Scene.MainActor.MechModel.Speed = new Vector2(0, -4);
                    }
                }

                Rayman rayman = (Rayman)Scene.MainActor;
                if (rayman.State == rayman.Fsm_SuperHelico || 
                    (ActionId == Action.Flame && IsActionFinished))
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AnimatedObject.ObjPriority = 15;
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}