﻿using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

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
                    // TODO: Set direction based on Sam actor
                    //cam.Direction = 

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
                cam.Direction = MathHelpers.Mod(-((Mode7Actor)LinkedObject).Direction, 256);

                Vector2 directionalVector = MathHelpers.DirectionalVector256(0x100 - cam.Direction);

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

                float linkedObjDir = ((Mode7Actor)LinkedObject).Direction;

                // NOTE: The game has no tolerance and uses a direct equality comparison. But that's because
                //       it doesn't store a fractional value for the direction.
                if (MathHelpers.Mod(Math.Abs(linkedObjDir - -cam.Direction), 256) >= 1)
                {
                    if (MathHelpers.Mod(linkedObjDir + cam.Direction, 256) < 128)
                    {
                        DirectionDelta += 0.25f + MathHelpers.Mod(linkedObjDir + cam.Direction, 256) * 0.0625f;
                        cam.Direction = MathHelpers.Mod(-(MathHelpers.Mod(DirectionDelta, 256) - cam.Direction), 256);
                        DirectionDelta %= 1;
                    }
                    else
                    {
                        DirectionDelta += 0.25f + MathHelpers.Mod(-cam.Direction - linkedObjDir, 256) * 0.0625f;
                        cam.Direction = MathHelpers.Mod(-(-MathHelpers.Mod(DirectionDelta, 256) - cam.Direction), 256);
                        DirectionDelta %= 1;
                    }
                }

                float speedLength = LinkedObject.Speed.Length();
                float targetLength = MainActorDistance + speedLength;
                Vector2 camDirectionalVector = MathHelpers.DirectionalVector256(0x100 - cam.Direction);

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
                Vector2 camDirectionalVector = MathHelpers.DirectionalVector256(0x100 - cam.Direction);

                if (ResetPosition)
                {
                    ResetPosition = false;

                    cam.Position = new Vector2(
                        x: LinkedObject.Position.X - camDirectionalVector.X * MainActorDistance,
                        y: LinkedObject.Position.Y + camDirectionalVector.Y * MainActorDistance);
                }

                cam.Direction = MathHelpers.Mod(-(1 - cam.Direction), 256);

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
}