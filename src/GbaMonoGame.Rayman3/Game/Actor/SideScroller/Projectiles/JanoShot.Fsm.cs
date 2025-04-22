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
                    if (ScreenPosition.X > Scene.Resolution.X + 1 && Speed.X > 0 || 
                        ScreenPosition.X < 0 && Speed.X < 0 ||
                        Speed.X == 0 || 
                        ScreenPosition.Y <= -10 || 
                        ScreenPosition.Y >= Scene.Resolution.Y + 10) // TODO: This is incorrectly set to 170 (GBA + 10) on N-Gage - should we keep that?
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