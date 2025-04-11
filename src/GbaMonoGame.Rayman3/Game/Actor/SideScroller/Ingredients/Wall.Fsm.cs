using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Wall
{
    public bool Fsm_Variant1State1(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Action0;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(Fsm_Variant1State2);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Variant1State2(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Action3;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Variant1State3);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Variant1State3(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Action1;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this) && !Scene.IsHitMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Main_PreventWallJumps, this);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Variant1State4);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // NOTE: For some reason there's no check against the current FSM state action, so the code always runs
    public bool Fsm_Variant1State4(FsmAction action)
    {
        if (Scene.IsDetectedMainActor(this))
            Scene.MainActor.ProcessMessage(this, Message.Main_PreventWallJumps, this);

        return true;
    }

    public bool Fsm_Variant2State1(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = GameTime.ElapsedFrames;
                ActionId = Action.Action2;
                break;

            case FsmAction.Step:
                if (GameTime.ElapsedFrames - Timer >= 240)
                {
                    State.MoveTo(Fsm_Variant2State3);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_Variant2State2(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Action3;
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Variant2State3);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Variant2State3(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Action4;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Main_PreventWallJumps, this);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Variant2State4);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Variant2State4(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Action5;
                Timer = GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Main_PreventWallJumps, this);

                if (GameTime.ElapsedFrames - Timer >= 240)
                {
                    State.MoveTo(Fsm_Variant2State5);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Variant2State5(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Action6;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Main_PreventWallJumps, this);

                if (IsActionFinished)
                {
                    State.MoveTo(Fsm_Variant2State1);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}