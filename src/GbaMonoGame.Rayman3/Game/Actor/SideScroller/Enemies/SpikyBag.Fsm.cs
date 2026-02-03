using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public partial class SpikyBag
{
    public bool Fsm_Stationary(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Stationary;
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                {
                    Scene.MainActor.ReceiveDamage(AttackPoints);
                    State.MoveTo(_Fsm_BeginSwing);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_BeginSwing(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.BeginSwing;

                if (AnimatedObject.IsFramed)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BallSwng_LumSwing_Mix03);
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Swing);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Swing(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Swing;

                if (AnimatedObject.IsFramed)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BallSwng_LumSwing_Mix03);
                break;

            case FsmAction.Step:
                if (Scene.IsHitMainActor(this))
                    Scene.MainActor.ReceiveDamage(AttackPoints);

                if (!AnimatedObject.IsDelayMode && 
                    AnimatedObject.CurrentFrame == 3 && 
                    AnimatedObject.CurrentAnimation is 0 or 2 && 
                    AnimatedObject.IsFramed)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BallSwng_LumSwing_Mix03);
                }

                if (IsActionFinished)
                {
                    CurrentSwingAnimation++;
                    if (CurrentSwingAnimation > 3)
                        CurrentSwingAnimation = 0;

                    AnimatedObject.CurrentAnimation = CurrentSwingAnimation;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}