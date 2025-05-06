using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class CaptureTheFlagFlag
{
    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AttachedPlayer = null;
                Scene.GetGameObject(BaseActorId).ProcessMessage(this, Message.CaptureTheFlagFlagBase_LinkFlag, this);
                break;
            
            case FsmAction.Step:
                Scene.GetGameObject(BaseActorId).ProcessMessage(this, Message.CaptureTheFlagFlagBase_LinkFlag, this);

                if (AttachedPlayer != null)
                {
                    State.MoveTo(null);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Dropped(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Position = AttachedPlayer.Position;

                // Optionally fix the palette being wrong
                SavedPaletteIndex = AnimatedObject.BasePaletteIndex;
                if (Engine.Config.FixBugs)
                {
                    if (MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Solo)
                    {
                        AnimatedObject.BasePaletteIndex = 0;
                    }
                    else
                    {
                        MessageRefParam<int> param = new();
                        AttachedPlayer.ProcessMessage(this, Message.Rayman_GetPlayerPaletteId, param);
                        AnimatedObject.BasePaletteIndex = param.Value + 1;
                    }
                }
                else
                {
                    AnimatedObject.BasePaletteIndex = 1;
                }

                AttachedPlayer = null;
                ActionId = Action.Fall_Left;
                IsMovingUp = true;
                break;
            
            case FsmAction.Step:
                bool reset = false;

                Box viewBox = GetViewBox();

                for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                {
                    Rayman player = Scene.GetGameObject<Rayman>(i);

                    if (viewBox.Intersects(player.GetDetectionBox()))
                    {
                        MessageRefParam<bool> playerParam = new();
                        player.ProcessMessage(this, Message.Rayman_GetCanPickUpDroppedFlag, playerParam);

                        if (playerParam.Value)
                        {
                            MessageRefParam<BaseActor> baseParam = new() { Value = player };
                            Scene.GetGameObject(BaseActorId).ProcessMessage(this, Message.CaptureTheFlagFlagBase_GetCapturableFlag, baseParam);

                            if (baseParam.Value == null || baseParam.Value != this)
                            {
                                reset = true;
                            }
                            else
                            {
                                player.ProcessMessage(this, Message.Rayman_PickUpFlag, this);
                                AttachedPlayer = player;
                            }

                            break;
                        }
                    }
                }

                if (Speed.Y > 0)
                    IsMovingUp = false;

                if (Speed == Vector2.Zero && !IsMovingUp && ActionId is Action.Fall_Right or Action.Fall_Left)
                    ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;

                if (AttachedPlayer != null)
                {
                    State.MoveTo(null);
                    return false;
                }

                if (reset)
                {
                    State.MoveTo(Fsm_Reset);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AnimatedObject.BasePaletteIndex = SavedPaletteIndex;
                break;
        }

        return true;
    }

    public bool Fsm_Reset(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Scene.GetGameObject(BaseActorId).ProcessMessage(this, Message.CaptureTheFlagFlagBase_ResetFlag);
                break;
            
            case FsmAction.Step:
                State.MoveTo(Fsm_Wait);
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}