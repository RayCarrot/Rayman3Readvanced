using System;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class CameraSideScroller : CameraActor2D
{
    public CameraSideScroller(Scene2D scene) : base(scene)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Follow);

        PreviousLinkedObjectPosition = Vector2.Zero;

        // Impossible condition - the linked object can't be set yet
        if (LinkedObject != null)
            PreviousLinkedObjectPosition = LinkedObject.Position;

        Speed = Speed with { X = 0 };
        Timer = 0;
        TargetY = 120;
        FollowYMode = FollowMode.Follow;
        ShakeLength = 0;

        HorizontalOffset = RSMultiplayer.IsActive ? CameraOffset.Multiplayer : CameraOffset.Default;
    }

    private static readonly float[] _shakeTable =
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
    public UnknownMode Unknown { get; set; } // What is this?
    public Vector2 PreviousLinkedObjectPosition { get; set; }
    public bool IsFacingRight { get; set; }
    public uint Timer { get; set; }
    public Vector2 Speed { get; set; }

    public Vector2 MoveTargetPos { get; set; }

    public int ShakeLength { get; set; }
    public ushort ShakeTimer { get; set; }
    public byte ShakeFrame { get; set; }
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
        if (ShakeLength != 0)
        {
            // Increment the timer
            ShakeTimer++;

            // The shake changes 16 times
            const int framesCount = 16;
            int shakeSpeed = ShakeLength / framesCount;

            int shakeTableIndex = ShakeFrame % 128 * 2;

            if (ShakeTimer == (ShakeFrame + 1) * shakeSpeed)
            {
                ShakeFrame++;
                shakeTableIndex = ShakeFrame % 128 * 2;
            }
            else if (ShakeTimer > ShakeFrame * shakeSpeed)
            {
                shakeTableIndex++;
            }
            
            shakeTableIndex %= 256;

            // Check to stop the shake
            if (ShakeTimer >= ShakeLength - 1)
                ShakeLength = 0;

            if (HasStartedShake || (ShakeTimer & 7) == 4)
            {
                HasStartedShake = true;

                // Down
                if ((ShakeTimer & 7) == 0)
                    return speed + new Vector2(0, _shakeTable[shakeTableIndex]);
                // Up
                else if ((ShakeTimer & 7) == 4)
                    return speed + new Vector2(0, -_shakeTable[shakeTableIndex]);
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
            // NOTE: This can't be triggered, cause the captor can't send messages to the camera
            case Message.Captor_Trigger_SendMessageWithCaptorParam:
                if (Timer != 7)
                {
                    Captor captor = (Captor)param;
                    Box captorBox = captor.GetCaptorBox();

                    MoveTargetPos = captorBox.Center - new Vector2(Scene.Resolution.X / 2, (int)(Scene.Resolution.X / 3));
                    Timer = 7;

                    State.MoveTo(_Fsm_MoveToTarget);
                }
                return true;

            case Message.Cam_CenterPositionX:
                Unknown = UnknownMode.PendingReset;
                HorizontalOffset = CameraOffset.Center;
                return true;

            // Seems to serve no purpose
            case Message.Cam_ResetUnknownMode:
                Unknown = UnknownMode.Default;
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
                if (!Engine.LocalConfig.Display.DisableCameraShake)
                {
                    ShakeLength = (int)param;
                    HasStartedShake = false;
                    ShakeTimer = 0;
                    ShakeFrame = 0;
                }
                return true;

            case Message.Cam_MoveToTarget:
                MoveTargetPos = (Vector2)param;

                // Scale the target based on the new resolution
                MoveTargetPos -= (Scene.Resolution - Rom.OriginalResolution) / 2;

                if (MoveTargetPos.X < 0)
                    MoveTargetPos = MoveTargetPos with { X = 0 };
                if (MoveTargetPos.Y < 0)
                    MoveTargetPos = MoveTargetPos with { Y = 0 };

                Timer = 5;

                State.MoveTo(_Fsm_MoveToTarget);
                return true;

            case Message.Cam_MoveToLinkedObject:
                float xOffset = LinkedObject.IsFacingLeft
                    ? Scene.Resolution.X - ScaledHorizontalOffset
                    : ScaledHorizontalOffset;
                float yOffset = ScaledTargetY;

                MoveTargetPos = new Vector2(LinkedObject.Position.X - xOffset, LinkedObject.Position.Y - yOffset);

                if (MoveTargetPos.X < 0)
                    MoveTargetPos = MoveTargetPos with { X = 0 };
                if (MoveTargetPos.Y < 0)
                    MoveTargetPos = MoveTargetPos with { Y = 0 };

                if (param is not true)
                    Timer = 6;

                State.MoveTo(_Fsm_MoveToTarget);
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
                State.MoveTo(_Fsm_Follow);
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
        if (LinkedObject == null)
            throw new Exception("The camera has no linked actor");

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

        ImGui.Text($"State: {State}");

        bool freeMove = Debug_FreeMoveCamera;
        ImGui.Checkbox("Free move (right mouse button)", ref freeMove);
        Debug_FreeMoveCamera = freeMove;

        ImGui.Text($"Speed: {Speed.X} x {Speed.Y}");
        ImGui.Text($"Target: {TargetX} x {ScaledTargetY}");
        ImGui.Text($"HorizontalOffset: {HorizontalOffset}");
        ImGui.Text($"ScaledHorizontalOffset: {ScaledHorizontalOffset}");
    }

    public enum FollowMode
    {
        DoNotFollow = 0,
        Follow = 1,
        FollowUntilNearby = 2,
    }

    public enum UnknownMode
    {
        Default = 1,
        Unused = 2,
        UnusedWithInputs = 3,
        PendingReset = 4,
    }
}