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

                    State.MoveTo(Fsm_Spin);
                    return false;
                }

                // Default
                if (Timer == 2)
                {
                    State.MoveTo(Fsm_Follow);
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

                cam.Position = new Vector2(
                    x: LinkedObject.Position.X - directionalVector.X * MainActorDistance,
                    y: LinkedObject.Position.Y + directionalVector.Y * MainActorDistance);
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
                if (Math.Abs(linkedObjDir - cam.Direction.Inverse()) >= 1)
                {
                    if (linkedObjDir + cam.Direction < Angle256.Half)
                    {
                        DirectionDelta += 0.25f + (linkedObjDir + cam.Direction) * 0.0625f;
                        cam.Direction = (DirectionDelta - cam.Direction).Inverse();
                    }
                    else
                    {
                        DirectionDelta += 0.25f + (cam.Direction.Inverse() - linkedObjDir) * 0.0625f;
                        cam.Direction = (DirectionDelta.Inverse() - cam.Direction).Inverse();
                    }

                    DirectionDelta %= 1;
                }

                float speedLength = LinkedObject.Speed.Length();
                float targetLength = MainActorDistance + speedLength;
                Vector2 camDirectionalVector = cam.Direction.Inverse().ToDirectionalVector();

                Vector2 posDelta = new(
                    x: LinkedObject.Position.X - (targetLength * camDirectionalVector.X + camDirectionalVector.X / 2) - cam.Position.X,
                    y: LinkedObject.Position.Y + (targetLength * camDirectionalVector.Y + camDirectionalVector.Y / 2) - cam.Position.Y);

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

                    cam.Position = new Vector2(
                        x: LinkedObject.Position.X - camDirectionalVector.X * MainActorDistance,
                        y: LinkedObject.Position.Y + camDirectionalVector.Y * MainActorDistance);
                }

                cam.Direction = (1 - cam.Direction).Inverse();

                Vector2 posDelta = new(
                    x: LinkedObject.Position.X - (MainActorDistance * camDirectionalVector.X + camDirectionalVector.X / 2) - cam.Position.X,
                    y: LinkedObject.Position.Y + (MainActorDistance * camDirectionalVector.Y + camDirectionalVector.Y / 2) - cam.Position.Y);

                cam.Position += posDelta;
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

                Vector2 camDirectionalVector = cam.Direction.Inverse().ToDirectionalVector();
                Vector2 targetPos = (LinkedObject.Position + sam.Position) / 2;

                Vector2 posDelta = new(
                    x: targetPos.X -
                       (MainActorDistance * camDirectionalVector.X + camDirectionalVector.X / 2) - 
                       cam.Position.X,
                    y: targetPos.Y + 
                       (MainActorDistance * camDirectionalVector.Y + camDirectionalVector.Y / 2) - 
                       cam.Position.Y);

                cam.Position += posDelta;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}