using System;
using GbaMonoGame.Engine2d;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Rayman3;

// TODO: Can probably rewrite some of this to use floats and be smoother
public sealed partial class CameraSideScroller : CameraActor2D
{
    public CameraSideScroller(Scene2D scene) : base(scene)
    {
        State.SetTo(Fsm_Follow);

        PreviousLinkedObjectPosition = Vector2.Zero;

        if (LinkedObject != null)
            PreviousLinkedObjectPosition = LinkedObject.Position;

        Speed = Speed with { X = 0 };
        Timer = 0;
        TargetY = 120;
        FollowYMode = FollowMode.Follow;
        ShakeTargetTime = 0;

        HorizontalOffset = RSMultiplayer.IsActive ? CameraOffset.Multiplayer : CameraOffset.Default;
    }

    private static readonly float[] ShakeTable =
    [
        1.00f, 2.00f, 4.00f, 6.00f,
        6.00f, 6.00f, 6.00f, 6.00f,
        5.75f, 5.50f, 5.25f, 5.00f,
        4.75f, 4.50f, 4.25f, 4.00f,
        3.75f, 3.50f, 3.25f, 3.00f,
        2.75f, 2.50f, 2.25f, 2.00f,
        1.75f, 1.50f, 1.25f, 1.00f,
        0.75f, 0.50f, 0.25f, 0.00f
    ];

    private float ScaledHorizontalOffset => ScaleXValue(HorizontalOffset);
    private float ScaledTargetY => ScaleYValue(TargetY);

    public float HorizontalOffset { get; set; }
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public FollowMode FollowYMode { get; set; }
    public byte field16_0x2e { get; set; }
    public Vector2 PreviousLinkedObjectPosition { get; set; }
    public bool IsFacingRight { get; set; }
    public uint Timer { get; set; }
    public Vector2 Speed { get; set; }

    public Vector2 MoveTargetPos { get; set; }

    public int ShakeTargetTime { get; set; }
    public ushort ShakeTimer { get; set; }
    public byte UnknownShakeValue { get; set; }
    public bool HasStartedShake { get; set; }

    public bool Debug_FreeMoveCamera { get; set; } // Custom free move camera

    // Handle scaling by centering the target offsets within the new scaled view area
    private float ScaleXValue(float value) => value + 
                                              (Scene.Resolution.X - Rom.OriginalResolution.X) / 2;
    private float ScaleYValue(float value) => value +
                                              (Scene.Resolution.Y - Rom.OriginalResolution.Y) / 2;

    private void UpdateTargetX()
    {
        if (LinkedObject.IsFacingLeft)
            TargetX = Scene.Resolution.X - ScaledHorizontalOffset;
        else
            TargetX = ScaledHorizontalOffset;
    }

    private Vector2 VerticalShake(Vector2 speed)
    {
        if (ShakeTargetTime != 0)
        {
            ShakeTimer++;

            int index = (UnknownShakeValue % 128) * 2;

            if (ShakeTimer == (UnknownShakeValue + 1) * (ShakeTargetTime / 16))
            {
                UnknownShakeValue++;
                index = (UnknownShakeValue % 128) * 2;
            }
            else if (ShakeTimer > UnknownShakeValue * (ShakeTargetTime / 16))
            {
                index++;
            }
            
            index %= 256;

            // Check to stop the shake
            if (ShakeTargetTime - 1 <= ShakeTimer)
                ShakeTargetTime = 0;

            if (HasStartedShake || (ShakeTimer & 7) == 4)
            {
                HasStartedShake = true;

                // Down
                if ((ShakeTimer & 7) == 0)
                    return speed + new Vector2(0, ShakeTable[index]);
                // Up
                else if ((ShakeTimer & 7) == 4)
                    return speed + new Vector2(0, -ShakeTable[index]);
            }
        }

        return speed;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            // TODO: How can this be triggered? The captor can't send message to the camera...
            case Message.Captor_Trigger_SendMessageWithCaptorParam:
                throw new NotImplementedException();
                return true;

            case Message.Cam_CenterPositionX:
                field16_0x2e = 4;
                HorizontalOffset = CameraOffset.Center;
                return true;

            case Message.Cam_ResetPositionX:
                field16_0x2e = 1;
                return true;

            case Message.Cam_DoNotFollowPositionY:
                FollowYMode = FollowMode.DoNotFollow;
                TargetY = (int)param;
                return true;

            case Message.Cam_FollowPositionY:
                FollowYMode = FollowMode.Follow;
                TargetY = (int)param;
                return true;

            case Message.Cam_FollowPositionYUntilNearby:
                FollowYMode = FollowMode.FollowUntilNearby;
                TargetY = (int)param;
                return true;

            case Message.Cam_Shake:
                ShakeTargetTime = (int)param;
                HasStartedShake = false;
                ShakeTimer = 0;
                UnknownShakeValue = 0;
                return true;

            case Message.Cam_MoveToTarget:
                MoveTargetPos = (Vector2)param;

                if (MoveTargetPos.X < 0)
                    MoveTargetPos = MoveTargetPos with { X = 0 };
                if (MoveTargetPos.Y < 0)
                    MoveTargetPos = MoveTargetPos with { Y = 0 };

                Timer = 5;

                State.MoveTo(Fsm_MoveToTarget);
                return true;

            case Message.Cam_MoveToLinkedObject:
                float xOffset = LinkedObject.IsFacingLeft
                    ? Scene.Resolution.X - ScaledHorizontalOffset
                    : ScaledHorizontalOffset;
                float yOffset = TargetY;

                MoveTargetPos = new Vector2(LinkedObject.Position.X - xOffset, LinkedObject.Position.Y - yOffset);

                if (MoveTargetPos.X < 0)
                    MoveTargetPos = MoveTargetPos with { X = 0 };
                if (MoveTargetPos.Y < 0)
                    MoveTargetPos = MoveTargetPos with { Y = 0 };

                if (param is not true)
                    Timer = 6;

                State.MoveTo(Fsm_MoveToTarget);
                return true;

            case Message.Cam_SetPosition:
                Scene.Playfield.Camera.Position = (Vector2)param;
                return true;

            case Message.Cam_Lock:
                if (param is Vector2 pos)
                    Scene.Playfield.Camera.Position = pos;

                State.MoveTo(null);
                return true;

            case Message.Cam_Unlock:
                State.MoveTo(Fsm_Follow);
                return true;

            default:
                return false;
        }
    }

    public override void Step()
    {
        if (Debug_FreeMoveCamera)
        {
            if (InputManager.GetMouseState().RightButton == ButtonState.Pressed)
                Scene.Playfield.Camera.Position += InputManager.GetMousePositionDelta(Scene.RenderContext) * -1;
        }
        else
        {
            base.Step();
        }
    }

    public override void SetFirstPosition()
    {
        IsFacingRight = LinkedObject.IsFacingRight;

        Vector2 pos;
        if (LinkedObject.Position.X < ScaledHorizontalOffset && LinkedObject.IsFacingRight)
        {
            pos = new Vector2(0, LinkedObject.Position.Y);
        }
        else if (LinkedObject.Position.X < Scene.Resolution.X - ScaledHorizontalOffset && LinkedObject.IsFacingLeft)
        {
            pos = new Vector2(0, LinkedObject.Position.Y);
        }
        else
        {
            float xOffset;
            if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4)
            {
                HorizontalOffset = CameraOffset.Center;
                xOffset = -ScaledHorizontalOffset;
            }
            else
            {
                if (LinkedObject.IsFacingLeft)
                    xOffset = ScaledHorizontalOffset - Scene.Resolution.X;
                else
                    xOffset = -ScaledHorizontalOffset;
            }

            pos = LinkedObject.Position + new Vector2(xOffset, 0);
        }

        pos.Y = Math.Max(pos.Y - ScaleYValue(120), 0);

        Scene.Playfield.Camera.Position = pos;
        PreviousLinkedObjectPosition = LinkedObject.Position;
    }

    public override void DrawDebugLayout(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        base.DrawDebugLayout(debugLayout, textureManager);

        bool freeMove = Debug_FreeMoveCamera;
        ImGui.Checkbox("Free move (right mouse button)", ref freeMove);
        Debug_FreeMoveCamera = freeMove;

        ImGui.Text($"Speed: {Speed.X} x {Speed.Y}");
        ImGui.Text($"Target: {TargetX} x {TargetY}");
    }

    public enum FollowMode
    {
        DoNotFollow = 0,
        Follow = 1,
        FollowUntilNearby = 2,
    }
}