using BinarySerializer.Ubisoft.GbaEngine;
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
                    Position = rayman.AttachedObject.Position + MathHelpers.DirectionalVector256(rayman.Timer) * Distance;

                bool finished = rayman.AttachedObject == null || 
                                (AnimatedObject.CurrentAnimation != 1 && 
                                 Distance > rayman.PreviousXSpeed - 32 && 
                                 rayman.PreviousXSpeed >= 80);

                if (AnimatedObject.CurrentAnimation == 1)
                    Distance = rayman.PreviousXSpeed - 30;

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

                if (Rom.Platform == Platform.GBA || Engine.Config.FixBugs)
                {
                    // NOTE: The game doesn't do this, however we have to do this to re-produce a bugged, but desirable, behavior. Basically,
                    //       in the original GBA code it doesn't do a null check on ´rayman.AttachedObject´ when setting the position. This
                    //       means on the very last frame of the sparkles appearing, when the attached object is null, then the position it
                    //       sets becomes some unexpected large out-of-bounds value. In the N-Gage version they added a null check to avoid
                    //       the game crashing (it doesn't crash on a null pointer on the GBA).
                    //       This sounds like undesirable behavior, however on the very first frame when the sparkles appear again they will
                    //       render on the last set position. This means that when hitting a purple lum again you will for a single frame
                    //       see sparkles appear at the wrong position, before going back to normal the next frame. With the bug this however
                    //       isn't visible because the last position will be out-of-bounds and thus not visible.
                    Position = new Vector2(-1000, -1000);
                }
                break;
        }

        return true;
    }
}