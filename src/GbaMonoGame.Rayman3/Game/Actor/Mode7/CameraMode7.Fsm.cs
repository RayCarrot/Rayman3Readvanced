using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

// NOTE: There are 3 unused states which haven't been re-implemented here. They are leftovers from the free-cam mode in the prototypes.
public partial class CameraMode7
{
    public bool Fsm_Init(FsmAction action)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                // Waterski
                if (Timer == 2 && IsWaterSki)
                {
                    SamMode7 sam = Scene.GetGameObject<SamMode7>(((RaymanMode7)LinkedObject).SamActorId);
                    cam.Direction = (sam.Direction - Angle256.Half).Inverse();

                    MainActorDistance -= 30;

                    State.MoveTo(_Fsm_Spin);
                    return false;
                }

                // Default
                if (Timer == 2)
                {
                    State.MoveTo(_Fsm_Follow);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Follow(FsmAction action)
    {
        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        switch (action)
        {
            case FsmAction.Init:
                cam.Direction = ((Mode7Actor)LinkedObject).Direction.Inverse();

                Vector2 directionalVector = cam.Direction.Inverse().ToDirectionalVector();
                cam.Position = LinkedObject.Position - (directionalVector * MainActorDistance).FlipY();
                break;

            case FsmAction.Step:
                if (RSMultiplayer.IsActive)
                {
                    if (!SoundEventsManager.IsSongPlaying(GameInfo.GetLevelMusicSoundEvent()) &&
                        !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__win3))
                    {
                        GameInfo.PlayLevelMusic();
                    }
                }

                Angle256 linkedObjDir = ((Mode7Actor)LinkedObject).Direction;

                // NOTE: The game has no tolerance and uses a direct equality comparison. But that's because
                //       it doesn't store a fractional value for the direction.
                if (Math.Abs((linkedObjDir - cam.Direction.Inverse()).SignedValue) >= 1)
                {
                    if (linkedObjDir + cam.Direction < Angle256.Half)
                    {
                        DirectionDelta += (1 / 4f) + (cam.Direction + linkedObjDir) * (1 / 16f);
                        cam.Direction = (cam.Direction.Inverse() + DirectionDelta).Inverse();
                    }
                    else
                    {
                        DirectionDelta += (1 / 4f) + (cam.Direction.Inverse() - linkedObjDir) * (1 / 16f);
                        cam.Direction = (cam.Direction.Inverse() + DirectionDelta.Inverse()).Inverse();
                    }

                    DirectionDelta %= 1;
                }
                else
                {
                    cam.Direction = linkedObjDir.Inverse();
                }

                float speedLength = LinkedObject.Speed.Length();
                float targetLength = MainActorDistance + speedLength;
                Vector2 camDirectionalVector = cam.Direction.Inverse().ToDirectionalVector();

                Vector2 posDelta = LinkedObject.Position - (targetLength * camDirectionalVector + camDirectionalVector / 2).FlipY() - cam.Position;

                // Clamp the movement
                posDelta = new Vector2(Math.Clamp(posDelta.X, -15, 15), Math.Clamp(posDelta.Y, -15, 15));

                cam.Position += posDelta;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Spin(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ResetPosition = true;
                break;

            case FsmAction.Step:
                TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;
                Vector2 camDirectionalVector = cam.Direction.Inverse().ToDirectionalVector();

                if (ResetPosition)
                {
                    ResetPosition = false;
                    cam.Position = LinkedObject.Position - (MainActorDistance * camDirectionalVector).FlipY();
                }

                cam.Direction = (cam.Direction.Inverse() + 1).Inverse();
                cam.Position = LinkedObject.Position - (MainActorDistance * camDirectionalVector + camDirectionalVector / 2).FlipY();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_WaterSkiFollow(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                MainActorDistance = 85;
                break;

            case FsmAction.Step:
                TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;
                SamMode7 sam = Scene.GetGameObject<SamMode7>(((RaymanMode7)LinkedObject).SamActorId);

                Angle256 linkedObjDir = sam.Direction;

                // NOTE: The game has no tolerance and uses a direct equality comparison. But that's because
                //       it doesn't store a fractional value for the direction.
                if (Math.Abs(linkedObjDir - cam.Direction.Inverse()) >= 1)
                {
                    // NOTE: The game changes by 1 every second frame, but we instead to 0.5 every frame
                    if (linkedObjDir + cam.Direction < Angle256.Half)
                        cam.Direction = (cam.Direction.Inverse() + 0.5f).Inverse();
                    else
                        cam.Direction = (cam.Direction.Inverse() - 0.5f).Inverse();
                }
                else
                {
                    cam.Direction = linkedObjDir.Inverse();
                }

                Vector2 camDirectionalVector = cam.Direction.Inverse().ToDirectionalVector();
                Vector2 targetPos = (LinkedObject.Position + sam.Position) / 2;

                cam.Position = targetPos - (MainActorDistance * camDirectionalVector + camDirectionalVector / 2).FlipY();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}