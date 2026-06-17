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
                    Speed = Speed with { X = TargetX < LinkedObjectScreenPosition.X ? 1 : -1 };
                }

                Timer = 0;
                break;

            case FsmAction.Step:
                // Reset speed y
                Speed = Speed with { Y = 0 };

                UpdateTargetX();

                float linkedObjDeltaX = LinkedObject.Position.X - PreviousLinkedObjectPosition.X;

                // If we're within 4 pixels of the target...
                if (Math.Abs(LinkedObjectScreenPosition.X - TargetX) <= 4)
                {
                    // Follow the linked object's movement
                    Speed = Speed with { X = linkedObjDeltaX };
                }
                // If far away from the target...
                else
                {
                    Timer++;

                    // Reset speed x if we're switching direction to move
                    if ((LinkedObjectScreenPosition.X < TargetX && Speed.X > 0) ||
                        (LinkedObjectScreenPosition.X > TargetX && Speed.X < 0))
                    {
                        Speed = Speed with { X = 0 };
                    }

                    float dir = LinkedObjectScreenPosition.X > TargetX ? 1 : -1;

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
                             ScaledHorizontalOffset + 40 > LinkedObjectScreenPosition.X &&
                             ScaledHorizontalOffset <= LinkedObjectScreenPosition.X) ||
                          (LinkedObject.IsFacingLeft &&
                           Resolution.X - 40 - ScaledHorizontalOffset < LinkedObjectScreenPosition.X &&
                           Resolution.X - ScaledHorizontalOffset > LinkedObjectScreenPosition.X))
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
                    float yOff = Resolution.Y - Rom.OriginalResolution.Y;

                    if ((LinkedObjectScreenPosition.Y < 70 + yOff / 2 && linkedObjDeltaY < 0) ||
                        (LinkedObjectScreenPosition.Y > 130 + yOff && linkedObjDeltaY > 0))
                    {
                        Speed = Speed with { Y = linkedObjDeltaY };
                    }

                }
                // Follow Y, the default
                else
                {
                    if (Math.Abs(LinkedObjectScreenPosition.Y - ScaledTargetY) <= 4)
                    {
                        Speed = Speed with { Y = linkedObjDeltaY };

                        if (FollowYMode == FollowMode.FollowUntilNearby)
                            FollowYMode = FollowMode.DoNotFollow;
                    }
                    else
                    {
                        if (ScaledTargetY < LinkedObjectScreenPosition.Y)
                        {
                            if (linkedObjDeltaY >= 2)
                            {
                                Speed = Speed with { Y = 5 };
                            }
                            else if (LinkedObjectScreenPosition.Y - ScaledTargetY >= 21)
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
                            else if (LinkedObjectScreenPosition.Y - ScaledTargetY <= -21)
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

                Vector2 camDelta = Speed;
                camDelta = VerticalShake(camDelta);

                // Clamp speed
                camDelta = new Vector2(Math.Clamp(camDelta.X, -7, 7), Math.Clamp(camDelta.Y, -7, 7));

                // Move camera
                Position += camDelta;

                if (FollowYMode == FollowMode.DoNotFollow)
                    Speed = Speed with { Y = 0 };

                PreviousLinkedObjectPosition = LinkedObject.Position;

                // Reset if changed direction
                if (!IsOnLimit(Edge.Left) &&
                    !IsOnLimit(Edge.Right) &&
                    LinkedObject.IsFacingRight != IsFacingRight)
                {
                    State.MoveTo(_Fsm_Follow);
                    return false;
                }

                if (Unknown == UnknownMode.PendingReset)
                {
                    HorizontalOffset = CameraOffset.Default;
                    Unknown = UnknownMode.Default;
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
                    Vector2 pos = Position.Truncate();
                    Vector2 target = MoveTargetPos.Truncate();

                    float dist = MathF.Truncate(Vector2.Distance(target, pos));

                    if (dist != 0)
                    {
                        Speed = (target - pos) / dist;
                        Speed *= 4;
                    }
                }
                break;

            case FsmAction.Step:
                {
                    Vector2 pos = Position;

                    // Reset X
                    if (Speed.X > 0)
                    {
                        if (pos.X + Speed.X > MoveTargetPos.X || IsOnLimit(Edge.Right))
                            Speed = new Vector2(0, RSMultiplayer.IsActive ? Speed.Y : 0);
                    }
                    else if (Speed.X < 0)
                    {
                        if (pos.X + Speed.X < MoveTargetPos.X || IsOnLimit(Edge.Left))
                            Speed = new Vector2(0, RSMultiplayer.IsActive ? Speed.Y : 0);
                    }

                    if (RSMultiplayer.IsActive)
                    {
                        // Reset Y
                        if (Speed.Y > 0)
                        {
                            if (pos.Y + Speed.Y > MoveTargetPos.Y || IsOnLimit(Edge.Bottom))
                                Speed = new Vector2(Speed.X, 0);
                        }
                        else if (Speed.Y < 0)
                        {
                            if (pos.Y + Speed.Y < MoveTargetPos.Y || IsOnLimit(Edge.Top))
                                Speed = new Vector2(Speed.X, 0);
                        }
                    }

                    Vector2 camDelta = Speed;
                    camDelta = VerticalShake(camDelta);

                    // Clamp speed - weird that it's 8 and not 7, a typo in the original code?
                    camDelta = new Vector2(Math.Clamp(camDelta.X, -8, 8), Math.Clamp(camDelta.Y, -8, 8));

                    // Move camera
                    Position += camDelta;

                    // Reached target
                    if ((Timer == 6 && Speed.X == 0) ||
                        (RSMultiplayer.IsActive && Speed == Vector2.Zero))
                    {
                        if (!IsKnotsSource)
                            Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
                        State.MoveTo(_Fsm_Follow);
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

    // Unused
    public bool Fsm_Unused(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                PreviousLinkedObjectPosition = LinkedObject.Position;
                Timer = 0;
                break;

            case FsmAction.Step:
                float targetX;
                if (Unknown is UnknownMode.Default or UnknownMode.UnusedWithInputs)
                {
                    if (Engine.JoyPad.IsButtonPressed(Rayman3Input.ActorLeft))
                        targetX = Resolution.X - (RSMultiplayer.IsActive ? CameraOffset.Multiplayer : CameraOffset.Default);
                    else if (Engine.JoyPad.IsButtonPressed(Rayman3Input.ActorRight))
                        targetX = RSMultiplayer.IsActive ? CameraOffset.Multiplayer : CameraOffset.Default;
                    else
                        targetX = CameraOffset.Center;
                }
                else
                {
                    targetX = CameraOffset.Center;
                }

                TargetX = ScaleXValue(targetX);

                if (Unknown == UnknownMode.PendingReset)
                    TargetY = 70;
                else
                    TargetY = 80;

                Vector2 camDelta = Vector2.Zero;

                float diffX = LinkedObject.Position.X - TargetX;
                if (diffX > Position.X)
                {
                    if (diffX - Position.X < 3)
                        camDelta.X = LinkedObject.Position.X - PreviousLinkedObjectPosition.X;
                    else
                        camDelta.X = 3;
                }
                else if (diffX < Position.X)
                {
                    if (Position.X - diffX < 3)
                        camDelta.X = LinkedObject.Position.X - PreviousLinkedObjectPosition.X;
                    else
                        camDelta.X = -3;
                }

                float diffY = LinkedObject.Position.Y - TargetY;
                if (diffY - 48 > Position.Y)
                {
                    if (diffY - (Position.Y + 48) < 3)
                        camDelta.Y = LinkedObject.Position.Y - PreviousLinkedObjectPosition.Y;
                    else
                        camDelta.Y = 3;
                }
                else if (diffY - 48 < Position.Y)
                {

                    if (Position.Y + 48 - diffY < 3)
                        camDelta.Y = LinkedObject.Position.Y - PreviousLinkedObjectPosition.Y;
                    else
                        camDelta.Y = -3;
                }

                camDelta = VerticalShake(camDelta);

                // Clamp speed
                camDelta = new Vector2(Math.Clamp(camDelta.X, -7, 7), Math.Clamp(camDelta.Y, -7, 7));

                // Move camera
                Position += camDelta;

                PreviousLinkedObjectPosition = LinkedObject.Position;

                if (Timer == 0 && Unknown == UnknownMode.Default)
                    Timer = GameTime.ElapsedFrames;
                else if (Timer != 0 && Unknown is UnknownMode.Unused or UnknownMode.UnusedWithInputs)
                    Timer = 0;

                if (Unknown == UnknownMode.Default && GameTime.ElapsedFrames - Timer > 60)
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
}