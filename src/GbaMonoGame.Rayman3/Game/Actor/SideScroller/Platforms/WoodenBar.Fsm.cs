using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class WoodenBar
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (ActionId != Action.Idle && IsActionFinished)
                {
                    if (ActionId is Action.ShakingFast or Action.SpeedUp)
                        ActionId = Action.SlowDown;
                    else
                        ActionId = Action.Idle;
                }

                if (Scene.IsDetectedMainActor(this) &&
                    Position.Y + 40 < Scene.MainActor.Position.Y)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginHang, this);
                    State.MoveTo(Fsm_Grabbed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Grabbed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PreviousFrame = AnimatedObject.CurrentFrame;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this))
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginHang, this);

                    if (PreviousFrame != AnimatedObject.CurrentFrame)
                    {
                        float mainOffsetX = ActionId switch
                        {
                            Action.Idle => AnimatedObject.CurrentFrame switch
                            {
                                0 => 0,
                                1 => -1,
                                2 => -1,
                                3 => 0,
                                4 => 1,
                                5 => 0,
                                6 => 1,
                                7 => 1,
                                8 => 0,
                                9 => -1,
                                10 => 0,
                                11 => 0,
                                12 => 0,
                                13 => 0,
                                _ => throw new Exception("Invalid frame index")
                            },
                            Action.SpeedUp => AnimatedObject.CurrentFrame switch
                            {
                                0 => 0,
                                1 => 3,
                                2 => 1,
                                3 => 0,
                                4 => 0,
                                5 => 0,
                                6 => 0,
                                7 => 0,
                                8 => 0,
                                9 => 0,
                                10 => 0,
                                11 => 0,
                                12 => 0,
                                13 => 0,
                                _ => throw new Exception("Invalid frame index")
                            },
                            Action.ShakingFast => AnimatedObject.CurrentFrame switch
                            {
                                0 => 1,
                                1 => 1,
                                2 => -2,
                                3 => -2,
                                4 => -4,
                                5 => -1,
                                6 => -1,
                                7 => 1,
                                8 => 2,
                                9 => 2,
                                10 => 2,
                                11 => 1,
                                12 => 0,
                                13 => 0,
                                _ => throw new Exception("Invalid frame index")
                            },
                            Action.SlowDown => AnimatedObject.CurrentFrame switch
                            {
                                0 => 1,
                                1 => 1,
                                2 => -2,
                                3 => -2,
                                4 => -4,
                                5 => -1,
                                6 => -1,
                                7 => 1,
                                8 => 1,
                                9 => 3,
                                10 => 1,
                                11 => 1,
                                12 => -1,
                                13 => 0,
                                _ => throw new Exception("Invalid frame index")
                            },
                            _ => throw new Exception("Invalid action")
                        };

                        Scene.MainActor.Position += new Vector2(mainOffsetX, 0);

                        PreviousFrame = AnimatedObject.CurrentFrame;
                    }

                    if (Scene.MainActor.Speed.X == 0 || ActionId == Action.ShakingFast)
                    {
                        if (Scene.MainActor.Speed.X == 0 && ActionId != Action.Idle && IsActionFinished)
                        {
                            if (ActionId is Action.ShakingFast or Action.SpeedUp)
                                ActionId = Action.SlowDown;
                            else
                                ActionId = Action.Idle;
                        }
                    }
                    else
                    {
                        if (IsActionFinished && ActionId == Action.SpeedUp)
                        {
                            ActionId = Action.ShakingFast;
                        }
                        else if (ActionId == Action.Idle)
                        {
                            ActionId = Action.SpeedUp;
                        }
                    }
                }

                if (!Scene.IsDetectedMainActor(this))
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_EndHang, this);
                    State.MoveTo(Fsm_Idle);
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