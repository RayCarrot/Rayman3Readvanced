using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class KegDebris
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                Timer++;

                Position += new Vector2(0, ActionId switch
                {
                    0 => 3.25f,
                    1 => 3,
                    2 => 3.5f,
                    3 => 2.75f,
                    _ => 0,
                });

                if (Timer > 60 && !Scene.Camera.IsActorFramed(this))
                {
                    State.MoveTo(_Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ScreenPosition = Vector2.Zero;
                Timer = 0;
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}