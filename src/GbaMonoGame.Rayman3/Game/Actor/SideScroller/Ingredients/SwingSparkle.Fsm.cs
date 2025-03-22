using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class SwingSparkle
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (AnimatedObject.CurrentAnimation != 1)
                    AnimatedObject.CurrentFrame = Random.GetNumber(7);
                break;

            case FsmAction.Step:
                Rayman rayman = (Rayman)Scene.MainActor;
                if (rayman.AttachedObject != null)
                {
                    float xPos = rayman.AttachedObject.Position.X + MathHelpers.Cos256(rayman.Timer) * Value;
                    float yPos = rayman.AttachedObject.Position.Y + MathHelpers.Sin256(rayman.Timer) * Value;
                    Position = new Vector2(xPos, yPos);
                }

                bool finished = rayman.AttachedObject == null || 
                                (AnimatedObject.CurrentAnimation != 1 && 
                                 Value > rayman.PreviousXSpeed - 32 && 
                                 rayman.PreviousXSpeed >= 80);

                if (AnimatedObject.CurrentAnimation == 1)
                    Value = rayman.PreviousXSpeed - 30;

                if (finished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ScreenPosition = ScreenPosition with { X = 0 };
                AnimatedObject.CurrentAnimation = 0;
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}