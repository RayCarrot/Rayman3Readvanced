using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class KegFire
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Default_Right;
                break;

            case FsmAction.Step:
                InteractableActor hitKeg = Scene.IsHitActorOfType(this, (int)ActorType.Keg);
                hitKeg?.ProcessMessage(this, IsFacingRight ? Message.Actor_LightOnFireRight : Message.Actor_LightOnFireLeft);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}