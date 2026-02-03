using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Piranha
{
    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Position = InitPos;
                ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;
                Timer = 0;
                ShouldDraw = false;
                break;

            case FsmAction.Step:
                Timer++;
                
                if (Scene.IsDetectedMainActor(this) && Timer > 120)
                {
                    State.MoveTo(_Fsm_Move);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ShouldDraw = true;
                ActionId = IsFacingRight ? Action.Move_Right : Action.Move_Left;
                SpawnSplash();
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    SpawnSplash();
                }
                else
                {
                    if (Scene.IsHitMainActor(this))
                        Scene.MainActor.ReceiveDamage(AttackPoints);
                }

                if (HitPoints == 0)
                {
                    State.MoveTo(_Fsm_Dying);
                    return false;
                }
                else if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Wait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;
                break;

            case FsmAction.Step:
                if (IsActionFinished && ActionId is Action.Dying_Right or Action.Dying_Left)
                    ActionId = IsFacingRight ? Action.Dead_Right : Action.Dead_Left;

                PhysicalType type = Scene.GetPhysicalType(Position);

                if (type == PhysicalTypeValue.Water)
                {
                    State.MoveTo(_Fsm_Wait);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SpawnSplash();
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}