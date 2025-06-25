using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class JanoShot
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // ???
                ScreenPosition = ScreenPosition with { X = 0 };
                Speed = Speed with { X = MathHelpers.FromFixedPoint(1) };

                ProcessMessage(this, Message.Destroy);
                break;

            case FsmAction.Step:
                bool finished = false;

                if (ActionId == Action.Hit)
                {
                    finished = IsActionFinished;
                }
                else if (ActionId == Action.Move_Down)
                {
                    if (ScreenPosition.Y > Scene.Resolution.Y + 1 || Speed.X == 0)
                    {
                        if (Speed.X == 0)
                        {
                            ActionId = Action.Hit;
                        }
                        else
                        {
                            finished = true;
                        }
                    }

                    if (Scene.IsHitMainActor(this) && !Scene.MainActor.IsInvulnerable)
                    {
                        Scene.MainActor.ReceiveDamage(AttackPoints);
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                        ActionId = Action.Hit;
                    }

                    if (Scene.MainActor.Position.X > 1700)
                        ActionId = Action.Hit;
                }
                else
                {
                    // In the original game the max y value is hard-coded to 160, even on N-Gage. But it should be the vertical screen resolution + margin.
                    const float marginY = 10;
                    const float minY = 0 - marginY;
                    float maxY;
                    if (Engine.Config.Tweaks.FixBugs)
                        maxY = Scene.Resolution.Y + marginY;
                    else
                        maxY = 160 + marginY;

                    if (ScreenPosition.X > Scene.Resolution.X + 1 && Speed.X > 0 || 
                        ScreenPosition.X < 0 && Speed.X < 0 ||
                        Speed.X == 0 || 
                        ScreenPosition.Y <= minY || 
                        ScreenPosition.Y >= maxY)
                    {
                        if (Speed.X == 0)
                        {
                            ActionId = Action.Hit;
                        }
                        else
                        {
                            finished = true;
                        }
                    }

                    if (Scene.IsHitMainActor(this) && !Scene.MainActor.IsInvulnerable)
                    {
                        Scene.MainActor.ReceiveDamage(AttackPoints);
                        Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt);
                        ActionId = Action.Hit;
                    }
                }

                if (finished)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // ???
                ScreenPosition = ScreenPosition with { X = 0 };
                Speed = Speed with { X = MathHelpers.FromFixedPoint(1) };

                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}