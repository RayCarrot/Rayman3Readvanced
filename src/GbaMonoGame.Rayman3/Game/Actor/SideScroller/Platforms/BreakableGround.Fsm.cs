using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class BreakableGround
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                IsSolid = true;
                break;

            case FsmAction.Step:
                // Disable Rayman being near an edge if the breakable ground is there
                if (Scene.IsDetectedMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_DisableNearEdge);

                if (Scene.IsDetectedMainActor(this) && Scene.MainActor.LinkedMovementActor != this && Scene.MainActor.Position.Y <= Position.Y)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
                }
                else if (Scene.MainActor.LinkedMovementActor == this && 
                         (!Scene.IsDetectedMainActor(this) || Scene.MainActor.Position.Y > Position.Y))
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                }

                if (ActionId == Action.Destroyed)
                {
                    State.MoveTo(_Fsm_Destroyed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (Scene.IsDetectedMainActor(this))
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                break;
        }

        return true;
    }

    public bool Fsm_Destroyed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                IsSolid = false;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__WoodBrk1_Mix04);
                break;

            case FsmAction.Step:
                bool multiplayerFinished = false;

                if (IsActionFinished)
                {
                    if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive)
                        multiplayerFinished = true;
                    else
                        ProcessMessage(this, Message.Destroy);
                }

                if (multiplayerFinished)
                {
                    IsDestroyed = true;
                    State.MoveTo(_Fsm_MultiplayerRespawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MultiplayerRespawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                bool outOfView = true;

                Box viewBox = GetViewBox();
                viewBox.Left -= Scene.Resolution.X;
                viewBox.Top -= Scene.Resolution.Y;
                viewBox.Right += Scene.Resolution.X;
                viewBox.Bottom += Scene.Resolution.Y;
                
                for (int playerId = 0; playerId < RSMultiplayer.PlayersCount; playerId++)
                {
                    Box playerDetectionBox = Scene.GetGameObject<ActionActor>(playerId).GetDetectionBox();

                    if (viewBox.Intersects(playerDetectionBox))
                        outOfView = false;
                }

                if (outOfView)
                {
                    IsDestroyed = false;
                    ActionId = Action.Idle_Default;
                    State.MoveTo(_Fsm_Idle);
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