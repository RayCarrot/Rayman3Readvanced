﻿using System;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class CameraMode7 : CameraActorMode7
{
    public CameraMode7(Scene2D scene) : base(scene)
    {
        IsWaterSki = false;
        Timer = 0;
        MainActorDistance = 55;
        DirectionDelta = Angle256.Zero;
    }

    public bool IsWaterSki { get; set; }
    public uint Timer { get; set; }
    public float MainActorDistance { get; set; }
    public Angle256 DirectionDelta { get; set; }
    public bool ResetPosition { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.CamMode7_Spin:
                State.MoveTo(Fsm_Spin);

                if (param is true)
                    ResetPosition = false;
                return true;

            case Message.CamMode7_Reset:
                // Waterski
                if (IsWaterSki)
                    State.MoveTo(Fsm_WaterSkiFollow);
                // Default
                else
                    State.MoveTo(Fsm_Follow);
                return true;

            default:
                return false;
        }
    }

    public override void Step()
    {
        base.Step();

        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;
        cam.Step();
    }

    public override void SetFirstPosition()
    {
        if (LinkedObject == null)
            throw new Exception("The camera has no linked actor");

        TgxCameraMode7 cam = (TgxCameraMode7)Scene.Playfield.Camera;

        Mode7Actor linkedActor = (Mode7Actor)LinkedObject;

        // Set the direction as the inverse of the direction of the linked actor
        cam.Direction = -linkedActor.Direction;

        // Set the camera position
        Vector2 directionalVector = cam.Direction.Inverse().ToDirectionalVector();
        cam.Position = new Vector2(
            x: linkedActor.Position.X - directionalVector.X * (MainActorDistance + 90),
            y: linkedActor.Position.Y + directionalVector.Y * (MainActorDistance + 90));

        State.SetTo(Fsm_Init);
    }
}