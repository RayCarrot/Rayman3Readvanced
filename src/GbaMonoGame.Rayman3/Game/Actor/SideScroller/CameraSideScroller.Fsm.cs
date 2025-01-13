using System;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class CameraSideScroller
{
    public bool Fsm_Follow(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (LinkedObject != null)
                {
                    PreviousLinkedObjectPosition = LinkedObject.Position;
                    IsFacingRight = LinkedObject.IsFacingRight;

                    UpdateTargetX();
                    Speed = Speed with { X = TargetX < LinkedObject.ScreenPosition.X ? 1 : -1 };
                }

                Timer = 0;
                break;

            case FsmAction.Step:
                // Reset speed y
                Speed = Speed with { Y = 0 };

                UpdateTargetX();

                float linkedObjDeltaX = LinkedObject.Position.X - PreviousLinkedObjectPosition.X;

                // If we're within 4 pixels of the target...
                if (Math.Abs(LinkedObject.ScreenPosition.X - TargetX) <= 4)
                {
                    // Follow the linked object's movement
                    Speed = Speed with { X = linkedObjDeltaX };
                }
                // If far away from the target...
                else
                {
                    Timer++;

                    // Reset speed x if we're switching direction to move
                    if ((LinkedObject.ScreenPosition.X < TargetX && Speed.X > 0) ||
                        (LinkedObject.ScreenPosition.X > TargetX && Speed.X < 0))
                    {
                        Speed = Speed with { X = 0 };
                    }

                    float dir = LinkedObject.ScreenPosition.X > TargetX ? 1 : -1;

                    // If the linked object is moving faster than 2...
                    if (Math.Abs(linkedObjDeltaX) > 2)
                    {
                        // Move the camera alongside the linked object with a speed of 6
                        Speed = Speed with { X = dir * 6 };
                    }
                    // If the linked object is moving and
                    // the timer is greater than or equal to 3 and
                    // the absolute camera speed is less than 4...
                    else if (LinkedObject.Speed.X != 0 && Timer >= 3 && Math.Abs(Speed.X) < 4)
                    {
                        // Move the camera with a speed of 0.5 and reset the timer
                        Speed += new Vector2(dir * 0.5f, 0);
                        Timer = 0;
                    }
                    // If the linked object is not moving...
                    else if (LinkedObject.Speed.X == 0)
                    {
                        // If the linked object is within 40 pixels of the horizontal offset...
                        if ((LinkedObject.IsFacingRight &&
                             ScaledHorizontalOffset + 40 > LinkedObject.ScreenPosition.X &&
                             ScaledHorizontalOffset <= LinkedObject.ScreenPosition.X) ||
                          (LinkedObject.IsFacingLeft &&
                           Scene.Resolution.X - 40 - ScaledHorizontalOffset < LinkedObject.ScreenPosition.X &&
                           Scene.Resolution.X - ScaledHorizontalOffset > LinkedObject.ScreenPosition.X))
                        {
                            // If timer is greater than 2, slow down the speed if it's absolute greater than 1
                            if (Timer > 2 && Math.Abs(Speed.X) > 1)
                            {
                                Speed -= new Vector2(dir * 0.5f, 0);
                                Timer = 0;
                            }
                        }
                        else
                        {
                            // If the timer is greater than 5, increase the speed if it's absolute less than 4
                            if (Timer > 5 && Math.Abs(Speed.X) < 4)
                            {
                                Speed += new Vector2(dir * 0.5f, 0);
                                Timer = 0;
                            }
                        }
                    }
                }

                float linkedObjDeltaY = LinkedObject.Position.Y - PreviousLinkedObjectPosition.Y;

                // Do not follow Y (unless near the edge). Used when jumping for example.
                if (FollowYMode == FollowMode.DoNotFollow)
                {
                    float yOff = Scene.Resolution.Y - Rom.OriginalResolution.Y;

                    if ((LinkedObject.ScreenPosition.Y < 70 + yOff / 2 && linkedObjDeltaY < 0) ||
                        (LinkedObject.ScreenPosition.Y > 130 + yOff && linkedObjDeltaY > 0))
                    {
                        Speed = Speed with { Y = linkedObjDeltaY };
                    }

                }
                // Follow Y, the default
                else
                {
                    if (Math.Abs(LinkedObject.ScreenPosition.Y - ScaledTargetY) <= 4)
                    {
                        Speed = Speed with { Y = linkedObjDeltaY };

                        if (FollowYMode == FollowMode.FollowUntilNearby)
                            FollowYMode = FollowMode.DoNotFollow;
                    }
                    else
                    {
                        if (ScaledTargetY < LinkedObject.ScreenPosition.Y)
                        {
                            if (linkedObjDeltaY >= 2)
                            {
                                Speed = Speed with { Y = 5 };
                            }
                            else if (LinkedObject.ScreenPosition.Y - ScaledTargetY >= 21)
                            {
                                Speed = Speed with { Y = 3 };
                            }
                            else if (linkedObjDeltaY >= 1)
                            {
                                Speed = Speed with { Y = 2 };
                            }
                            else
                            {
                                Speed = Speed with { Y = 1 };
                            }
                        }
                        else
                        {
                            if (linkedObjDeltaY <= -2)
                            {
                                Speed = Speed with { Y = -5 };
                            }
                            else if (LinkedObject.ScreenPosition.Y - ScaledTargetY <= -21)
                            {
                                Speed = Speed with { Y = -3 };
                            }
                            else if (linkedObjDeltaY <= -1)
                            {
                                Speed = Speed with { Y = -2 };
                            }
                            else
                            {
                                Speed = Speed with { Y = -1 };
                            }
                        }
                    }
                }

                Speed = VerticalShake(Speed);

                // Clamp speed
                Speed = new Vector2(Math.Clamp(Speed.X, -7, 7), Math.Clamp(Speed.Y, -7, 7));

                TgxCamera2D tgxCam = ((TgxPlayfield2D)Scene.Playfield).Camera;
                TgxCluster mainCluster = tgxCam.GetMainCluster();

                // Move camera
                tgxCam.Position += Speed;

                PreviousLinkedObjectPosition = LinkedObject.Position;

                // Reset if changed direction
                if (!mainCluster.IsOnLimit(Edge.Left) &&
                    !mainCluster.IsOnLimit(Edge.Right) &&
                    LinkedObject.IsFacingRight != IsFacingRight)
                {
                    State.MoveTo(Fsm_Follow);
                    return false;
                }

                if (field16_0x2e == 4)
                {
                    HorizontalOffset = CameraOffset.Default;
                    field16_0x2e = 1;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveToTarget(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                {
                    TgxCamera2D tgxCam = ((TgxPlayfield2D)Scene.Playfield).Camera;
                    Vector2 pos = tgxCam.Position;
                    float dist = Vector2.Distance(MoveTargetPos, pos);
                    Speed = (MoveTargetPos - pos) / dist;
                    Speed *= 4;
                }
                break;

            case FsmAction.Step:
                {
                    TgxCamera2D tgxCam = ((TgxPlayfield2D)Scene.Playfield).Camera;
                    TgxCluster mainCluster = tgxCam.GetMainCluster();
                    Vector2 pos = tgxCam.Position;

                    // Reset X
                    if (Speed.X > 0)
                    {
                        if (pos.X + Speed.X > MoveTargetPos.X || mainCluster.IsOnLimit(Edge.Right))
                            Speed = new Vector2(0, RSMultiplayer.IsActive ? Speed.Y : 0);
                    }
                    else if (Speed.X < 0)
                    {
                        if (pos.X + Speed.X < MoveTargetPos.X || mainCluster.IsOnLimit(Edge.Left))
                            Speed = new Vector2(0, RSMultiplayer.IsActive ? Speed.Y : 0);
                    }

                    if (RSMultiplayer.IsActive)
                    {
                        // Reset Y
                        if (Speed.Y > 0)
                        {
                            if (pos.Y + Speed.Y > MoveTargetPos.Y || mainCluster.IsOnLimit(Edge.Bottom))
                                Speed = new Vector2(Speed.X, 0);
                        }
                        else if (Speed.Y < 0)
                        {
                            if (pos.Y + Speed.Y < MoveTargetPos.Y || mainCluster.IsOnLimit(Edge.Top))
                                Speed = new Vector2(Speed.X, 0);
                        }
                    }

                    Speed = VerticalShake(Speed);

                    // Clamp speed - weird that it's 8 and not 7, a typo in the original code?
                    Speed = new Vector2(Math.Clamp(Speed.X, -8, 8), Math.Clamp(Speed.Y, -8, 8));

                    // Move camera
                    tgxCam.Position += Speed;

                    // Reached target
                    if ((Timer == 6 && Speed.X == 0) ||
                        (RSMultiplayer.IsActive && Speed == Vector2.Zero))
                    {
                        Scene.MainActor.ProcessMessage(this, Message.Main_ExitStopOrCutscene);
                        State.MoveTo(Fsm_Follow);
                        return false;
                    }

                    if (Timer == 7)
                        Timer = 6;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}