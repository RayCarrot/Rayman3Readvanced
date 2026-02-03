namespace GbaMonoGame.Rayman3;

public partial class Bats
{
    public bool Fsm_FlyWait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Fly_Wait;
                break;
            
            case FsmAction.Step:
                bool detected = false;

                if (Scene.IsDetectedMainActor(this))
                {
                    HorizontalFlip = Position.X > Scene.MainActor.Position.X;
                    detected = true;
                }

                if (detected)
                {
                    State.MoveTo(_Fsm_FlyStart);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_FlyStart(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = InitialAction;
                ChangeAction();
                AnimatedObject.FlipX = HorizontalFlip;
                break;
            
            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_FlyAway);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                FlyPosition = Position;
                break;
        }

        return true;
    }

    public bool Fsm_FlyAway(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                switch (InitialAction)
                {
                    case Action.Fly_HorizontallyStart1:
                        ActionId = Action.Fly_Horizontally;

                        if (HorizontalFlip)
                            Position = FlyPosition + new Vector2(185, 18);
                        else
                            Position = FlyPosition + new Vector2(-185, 18);
                        break;

                    case Action.Fly_VerticallyStart:
                        ActionId = Action.Fly_Vertically;

                        if (HorizontalFlip)
                            Position = FlyPosition + new Vector2(33, -101);
                        else
                            Position = FlyPosition + new Vector2(-33, -101);
                        break;

                    case Action.Fly_HorizontallyStart2:
                        ActionId = Action.Fly_Horizontally;

                        if (HorizontalFlip)
                            Position = FlyPosition + new Vector2(188, -27);
                        else
                            Position = FlyPosition + new Vector2(-188, -27);
                        break;
                }
                break;
            
            case FsmAction.Step:
                switch (InitialAction)
                {
                    case Action.Fly_HorizontallyStart1:
                    case Action.Fly_HorizontallyStart2:
                        if (HorizontalFlip)
                            Position += new Vector2(4, 0);
                        else
                            Position -= new Vector2(4, 0);
                        break;

                    case Action.Fly_VerticallyStart:
                        Position -= new Vector2(0, 4);
                        break;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_StationaryWait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Stationary_Wait;
                break;
            
            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    State.MoveTo(_Fsm_StationaryFlap);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_StationaryFlap(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = InitialAction;
                break;
            
            case FsmAction.Step:
                if (!AnimatedObject.IsFramed)
                {
                    State.MoveTo(_Fsm_StationaryWait);
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