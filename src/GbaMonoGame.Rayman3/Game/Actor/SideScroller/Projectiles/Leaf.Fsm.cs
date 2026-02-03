using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Leaf
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;
            
            case FsmAction.Step:
                if (Delay != 0)
                {
                    Delay--;
                }
                else if (IsActionFinished)
                {
                    int rand = Random.GetNumber(9) / 3; // 0-2
                    ActionId = AnimationSet switch
                    {
                        // Set 1
                        0 when rand == 0 => Action.Leaf1_1,
                        0 when rand == 1 => Action.Leaf1_2,
                        0 when rand == 2 => Action.Leaf1_3,

                        // Set 2
                        1 when rand == 0 => Action.Leaf2_1,
                        1 when rand == 1 => Action.Leaf2_2,
                        1 when rand == 2 => Action.Leaf2_3,
                        
                        // Set 3
                        2 when rand == 0 => Action.Leaf3_1,
                        2 when rand == 1 => Action.Leaf3_2,
                        2 when rand == 2 => Action.Leaf3_3,
                        _ => throw new Exception()
                    };
                    Delay = Random.GetNumber(41) + 20;
                }

                float mainActorDistX = Math.Abs(Position.X - Scene.MainActor.Position.X);

                if (mainActorDistX > 200 || 
                    ScreenPosition.X < 0 || 
                    ScreenPosition.X > Scene.Resolution.X || 
                    ScreenPosition.Y > Scene.Resolution.Y)
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ScreenPosition = Vector2.Zero;
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}