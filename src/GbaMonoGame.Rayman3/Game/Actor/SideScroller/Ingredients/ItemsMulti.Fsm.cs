using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class ItemsMulti
{
    // NOTE: For some reason there's no check against the current FSM state action, so the code always runs
    public bool Fsm_Items(FsmAction action)
    {
        // Time out
        if (Timer >= 900)
        {
            // Despawn
            SpawnCountdown = 0xFF;
            Timer = 0;

            // Spawn a new item
            MultiplayerInfo.TagInfo.SpawnNewItem(Scene, false);
        }
        // Spawn
        else if (SpawnCountdown == 0)
        {
            FrameMultiSideScroller frame = (FrameMultiSideScroller)Frame.Current;

            // NOTE: Will be null first time as scene is being initialized - N-Gage has a null check and defaults to 0
            int tagId = frame.UserInfo?.GetTagId() ?? 0;

            if (ActionId != Action.Fist)
                Timer++;

            Box viewBox = GetViewBox();

            int currentFramePlayerId = (int)(MultiplayerManager.GetElapsedTime() % RSMultiplayer.MaxPlayersCount);
            if (currentFramePlayerId < MultiplayerManager.PlayersCount)
            {
                // The fist can only be collected by the player with the tag in tag mode or without the tag in burglar mode
                if (ActionId != Action.Fist || 
                    (MultiplayerInfo.GameType == MultiplayerGameType.RayTag && tagId == currentFramePlayerId) || 
                    (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && tagId != currentFramePlayerId))
                {
                    MovableActor player = Scene.GetGameObject<MovableActor>(currentFramePlayerId);

                    if (player.GetDetectionBox().Intersects(viewBox))
                    {
                        // Collect
                        player.ProcessMessage(this, ActionId switch
                        {
                            Action.Globox => Message.Main_CollectedMultiItemGlobox,
                            Action.Reverse => Message.Main_CollectedMultiItemReverse,
                            Action.Invisibility => Message.Main_CollectedMultiItemInvisibility,
                            Action.Fist => Message.Main_CollectedMultiItemFist,
                            _ => throw new ArgumentOutOfRangeException(nameof(ActionId), ActionId, null)
                        });

                        if (ActionId is Action.Globox or Action.Reverse or Action.Invisibility) 
                        {
                            // Despawn
                            SpawnCountdown = 0xff;
                            Timer = 0;

                            // Spawn a new item
                            MultiplayerInfo.TagInfo.SpawnNewItem(Scene, ActionId == Action.Invisibility);
                        }
                        else
                        {
                            // Respawn after 3 seconds
                            SpawnCountdown = 180;
                            
                            // Show +3 animation
                            if (currentFramePlayerId == MultiplayerManager.MachineId)
                            {
                                AnimatedObject.CurrentAnimation = 7;
                                AnimatedObject.ObjPriority = 0;
                            }
                        }
                    }
                }
            }
        }
        // Wait to spawn
        else if (SpawnCountdown != 0xFF)
        {
            if (SpawnCountdown == 119 && ActionId == Action.Fist && AnimatedObject.ObjPriority == 0)
            {
                AnimatedObject.CurrentAnimation = 3;
                AnimatedObject.ObjPriority = 32;
            }

            SpawnCountdown--;
        }

        return true;
    }
}