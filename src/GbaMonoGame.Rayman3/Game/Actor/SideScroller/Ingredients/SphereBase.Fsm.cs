using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class SphereBase
{
    public bool Fsm_Deactivated(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                // Check for collision with sphere
                foreach (BaseActor actor in new EnabledAlwaysActorIterator(Scene))
                {
                    // Verify type
                    if (actor.Type != (int)ActorType.Sphere)
                        continue;

                    Sphere sphere = (Sphere)actor;

                    // Verify matching color
                    if (sphere.Color != Color)
                        continue;

                    Box sphereBox = sphere.GetDetectionBox();
                    Box box = GetActionBox();

                    // Check collision
                    if (!sphereBox.Intersects(box))
                        continue;

                    // Verify sphere movement
                    if (sphere.Speed.Y <= 0)
                        continue;
                    
                    sphere.ProcessMessage(this, Message.Destroy);
                    State.MoveTo(_Fsm_Activating);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Activating(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SocleFX1_Mix01);
                ActionId = Action.Activating;

                // Send message to linked actors
                Message message = (Action)Resource.FirstActionId is Action.Init_Yellow_Resurrect or Action.Init_Purple_Resurrect 
                    ? Message.ResurrectWakeUp 
                    : Message.Gate_Open;
                foreach (byte? id in Resource.Links)
                {
                    if (id != null)
                        Scene.GetGameObject(id.Value).ProcessMessage(this, message, this);
                }
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    State.MoveTo(_Fsm_Activated);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Activated(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Activated;
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}