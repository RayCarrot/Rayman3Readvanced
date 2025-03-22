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
                    ActionId = (Action)(AnimationSet * 3 + Random.GetNumber(9) / 3);
                    Delay = Random.GetNumber(41) + 20;
                }

                float mainActorDistX = Math.Abs(Position.X - Scene.MainActor.Position.X);

                if (mainActorDistX > 200 || 
                    ScreenPosition.X < 0 || 
                    ScreenPosition.X > Scene.Resolution.X || 
                    ScreenPosition.Y > Scene.Resolution.Y)
                {
                    State.MoveTo(Fsm_Default);
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