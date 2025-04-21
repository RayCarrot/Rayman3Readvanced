using System;
using BinarySerializer;

namespace GbaMonoGame.Engine2d;

public class MechModel
{
    public MechModel()
    {
        _initActions =
        [
            Reset,
            UseConstantSpeed,
            None,
            SetSpeedXY,
            SetSpeedX_ResetSpeedY,
            SetSpeedY_ResetSpeedX,
            SetSpeedX,
            SetSpeedY,
            SetAccelerationXY_SetTargetSpeedXY,
            SetAccelerationX_SetTargetSpeedX_ResetSpeedY,
            SetAccelerationY_SetTargetSpeedY_ResetSpeedX,
            SetAccelerationX_SetTargetSpeedX,
            SetAccelerationY_SetTargetSpeedY,
            SetSpeedXY_SetAccelerationXY_SetTargetSpeedXY,
            SetSpeedX_SetAccelerationX_SetTargetSpeedX_ResetSpeedY,
            SetSpeedY_SetAccelerationY_SetTargetSpeedY_ResetSpeedX,
            SetSpeedX_SetAccelerationX_SetTargetSpeedX,
            SetSpeedY_SetAccelerationY_SetTargetSpeedY,
            Mode7_SetSpeedXY,
            Mode7_SetSpeedX_ResetSpeedY,
            Mode7_SetSpeedY_ResetSpeedX,
            Mode7_SetSpeedX,
            Mode7_SetSpeedY,
            Mode7_SetAccelerationXY_SetTargetSpeedXY,
            Mode7_SetAccelerationX_SetTargetSpeedX_ResetSpeedY,
            Mode7_SetAccelerationY_SetTargetSpeedY_ResetSpeedX,
            Mode7_SetAccelerationX_SetTargetSpeedX,
            Mode7_SetAccelerationY_SetTargetSpeedY_ResetSpeedX, // NOTE: Bug in the game? Shouldn't this be Mode7_SetAccelerationY_SetTargetSpeedY?
            Mode7_SetSpeedXY_SetAccelerationXY_SetTargetSpeedXY,
            Mode7_SetSpeedX_SetAccelerationX_SetTargetSpeedX_ResetSpeedY,
            Mode7_SetSpeedY_SetAccelerationY_SetTargetSpeedY_ResetSpeedX,
            Mode7_SetSpeedX_SetAccelerationX_SetTargetSpeedX,
            Mode7_SetSpeedY_SetAccelerationY_SetTargetSpeedY,
            Mode7_UseConstantSpeed
        ];
    }

    private readonly Action<float[], int>[] _initActions;

    public Vector2 Speed { get; set; }
    public Vector2 Acceleration { get; set; }
    public Vector2 TargetSpeed { get; set; }
    public Func<MovableActor, Vector2> UpdateSpeedAction { get; set; }

    private Vector2 SetConstSpeedXY(MovableActor actor)
    {
        return Speed;
    }

    private Vector2 SetAcceleratedSpeedX(MovableActor actor)
    {
        Speed = Speed with { X = Speed.X + Acceleration.X };

        if (Acceleration.X <= 0)
        {
            if (Speed.X <= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                UpdateSpeedAction = SetConstSpeedXY;
            }
        }
        else
        {
            if (Speed.X >= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                UpdateSpeedAction = SetConstSpeedXY;
            }
        }

        return SetConstSpeedXY(actor);
    }

    private Vector2 SetAcceleratedSpeedY(MovableActor actor)
    {
        Speed = Speed with { Y = Speed.Y + Acceleration.Y };

        if (Acceleration.Y <= 0)
        {
            if (Speed.Y <= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                UpdateSpeedAction = SetConstSpeedXY;
            }
        }
        else
        {
            if (Speed.Y >= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                UpdateSpeedAction = SetConstSpeedXY;
            }
        }

        return SetConstSpeedXY(actor);
    }

    private Vector2 SetAcceleratedSpeedXY(MovableActor actor)
    {
        Speed += Acceleration;

        bool stopX = false;
        bool stopY = false;

        if (Acceleration.X <= 0)
        {
            if (Speed.X <= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                stopX = true;
            }
        }
        else
        {
            if (Speed.X >= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                stopX = true;
            }
        }

        if (Acceleration.Y <= 0)
        {
            if (Speed.Y <= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                stopY = true;
            }
        }
        else
        {
            if (Speed.Y >= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                stopY = true;
            }
        }

        if (stopX)
        {
            if (stopY)
            {
                UpdateSpeedAction = SetConstSpeedXY;
            }
            else
            {
                UpdateSpeedAction = SetAcceleratedSpeedY;
            }
        }
        else
        {
            if (stopY)
            {
                UpdateSpeedAction = SetAcceleratedSpeedX;
            }
        }

        return SetConstSpeedXY(actor);
    }

    private Vector2 Mode7_SetConstSpeedXY(MovableActor actor)
    {
        Mode7Actor mode7Actor = (Mode7Actor)actor;
        return MathHelpers.Rotate256(Speed, mode7Actor.Direction) * new Vector2(1, -1);
    }

    private Vector2 Mode7_SetAcceleratedSpeedX(MovableActor actor)
    {
        Speed = Speed with { X = Speed.X + Acceleration.X };

        if (Acceleration.X <= 0)
        {
            if (Speed.X <= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                UpdateSpeedAction = Mode7_SetConstSpeedXY;
            }
        }
        else
        {
            if (Speed.X >= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                UpdateSpeedAction = Mode7_SetConstSpeedXY;
            }
        }

        Mode7Actor mode7Actor = (Mode7Actor)actor;
        return MathHelpers.Rotate256(Speed, mode7Actor.Direction) * new Vector2(1, -1);
    }

    private Vector2 Mode7_SetAcceleratedSpeedY(MovableActor actor)
    {
        Speed = Speed with { Y = Speed.Y + Acceleration.Y };

        if (Acceleration.Y <= 0)
        {
            if (Speed.Y <= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                UpdateSpeedAction = Mode7_SetConstSpeedXY;
            }
        }
        else
        {
            if (Speed.Y >= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                UpdateSpeedAction = Mode7_SetConstSpeedXY;
            }
        }

        Mode7Actor mode7Actor = (Mode7Actor)actor;
        return MathHelpers.Rotate256(Speed, mode7Actor.Direction) * new Vector2(1, -1);
    }

    private Vector2 Mode7_SetAcceleratedSpeedXY(MovableActor actor)
    {
        Speed += Acceleration;

        bool stopX = false;
        bool stopY = false;

        if (Acceleration.X <= 0)
        {
            if (Speed.X <= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                stopX = true;
            }
        }
        else
        {
            if (Speed.X >= TargetSpeed.X)
            {
                Speed = Speed with { X = TargetSpeed.X };
                stopX = true;
            }
        }

        if (Acceleration.Y <= 0)
        {
            if (Speed.Y <= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                stopY = true;
            }
        }
        else
        {
            if (Speed.Y >= TargetSpeed.Y)
            {
                Speed = Speed with { Y = TargetSpeed.Y };
                stopY = true;
            }
        }

        if (stopX)
        {
            if (stopY)
            {
                UpdateSpeedAction = Mode7_SetConstSpeedXY;
            }
            else
            {
                UpdateSpeedAction = Mode7_SetAcceleratedSpeedY;
            }
        }
        else
        {
            if (stopY)
            {
                UpdateSpeedAction = Mode7_SetAcceleratedSpeedX;
            }
        }

        Mode7Actor mode7Actor = (Mode7Actor)actor;
        return MathHelpers.Rotate256(Speed, mode7Actor.Direction) * new Vector2(1, -1);
    }

    private void Reset(float[] mechParams, int offset)
    {
        Reset();
    }

    private void UseConstantSpeed(float[] mechParams, int offset)
    {
        UpdateSpeedAction = SetConstSpeedXY;
    }

    private void None(float[] mechParams, int offset) { }

    private void SetSpeedXY(float[] mechParams, int offset)
    {
        SetSpeedX(mechParams, offset + 0);
        SetSpeedY(mechParams, offset + 1);
    }

    private void SetSpeedX_ResetSpeedY(float[] mechParams, int offset)
    {
        SetSpeedX(mechParams, offset);
        Speed = Speed with { Y = 0 };
    }

    private void SetSpeedY_ResetSpeedX(float[] mechParams, int offset)
    {
        SetSpeedY(mechParams, offset);
        Speed = Speed with { X = 0 };
    }

    private void SetSpeedX(float[] mechParams, int offset)
    {
        Speed = Speed with { X = mechParams[offset] };
        UpdateSpeedAction = SetConstSpeedXY;
    }

    private void SetSpeedY(float[] mechParams, int offset)
    {
        Speed = Speed with { Y = mechParams[offset] };
        UpdateSpeedAction = SetConstSpeedXY;
    }

    private void SetAccelerationXY_SetTargetSpeedXY(float[] mechParams, int offset)
    {
        SetAccelerationX_SetTargetSpeedX(mechParams, offset + 0);
        SetAccelerationY_SetTargetSpeedY(mechParams, offset + 2);
        UpdateSpeedAction = SetAcceleratedSpeedXY;
    }

    private void SetAccelerationX_SetTargetSpeedX_ResetSpeedY(float[] mechParams, int offset)
    {
        SetAccelerationX_SetTargetSpeedX(mechParams, offset);
        Speed = Speed with { Y = 0 };
    }

    private void SetAccelerationY_SetTargetSpeedY_ResetSpeedX(float[] mechParams, int offset)
    {
        SetAccelerationY_SetTargetSpeedY(mechParams, offset);
        Speed = Speed with { X = 0 };
    }

    private void SetAccelerationX_SetTargetSpeedX(float[] mechParams, int offset)
    {
        Acceleration = Acceleration with { X = mechParams[offset + 0] };
        TargetSpeed = TargetSpeed with { X = mechParams[offset + 1] };
        UpdateSpeedAction = SetAcceleratedSpeedX;
    }

    private void SetAccelerationY_SetTargetSpeedY(float[] mechParams, int offset)
    {
        Acceleration = Acceleration with { Y = mechParams[offset + 0] };
        TargetSpeed = TargetSpeed with { Y = mechParams[offset + 1] };
        UpdateSpeedAction = SetAcceleratedSpeedY;
    }

    private void SetSpeedXY_SetAccelerationXY_SetTargetSpeedXY(float[] mechParams, int offset)
    {
        SetSpeedX_SetAccelerationX_SetTargetSpeedX(mechParams, offset + 0);
        SetSpeedY_SetAccelerationY_SetTargetSpeedY(mechParams, offset + 3);
        UpdateSpeedAction = SetAcceleratedSpeedXY;
    }

    private void SetSpeedX_SetAccelerationX_SetTargetSpeedX_ResetSpeedY(float[] mechParams, int offset)
    {
        SetSpeedX_SetAccelerationX_SetTargetSpeedX(mechParams, offset);
        Speed = Speed with { Y = 0 };
    }

    private void SetSpeedY_SetAccelerationY_SetTargetSpeedY_ResetSpeedX(float[] mechParams, int offset)
    {
        SetSpeedY_SetAccelerationY_SetTargetSpeedY(mechParams, offset);
        Speed = Speed with { X = 0 };
    }

    private void SetSpeedX_SetAccelerationX_SetTargetSpeedX(float[] mechParams, int offset)
    {
        Speed = Speed with { X = mechParams[offset + 0] };
        Acceleration = Acceleration with { X = mechParams[offset + 1] };
        TargetSpeed = TargetSpeed with { X = mechParams[offset + 2] };
        UpdateSpeedAction = SetAcceleratedSpeedX;
    }

    private void SetSpeedY_SetAccelerationY_SetTargetSpeedY(float[] mechParams, int offset)
    {
        Speed = Speed with { Y = mechParams[offset + 0] };
        Acceleration = Acceleration with { Y = mechParams[offset + 1] };
        TargetSpeed = TargetSpeed with { Y = mechParams[offset + 2] };
        UpdateSpeedAction = SetAcceleratedSpeedY;
    }

    private void Mode7_SetSpeedXY(float[] mechParams, int offset)
    {
        Speed = new Vector2(mechParams[offset + 0], mechParams[offset + 1]);
        UpdateSpeedAction = Mode7_SetConstSpeedXY;
    }

    private void Mode7_SetSpeedX_ResetSpeedY(float[] mechParams, int offset)
    {
        Speed = new Vector2(mechParams[offset], 0);
        UpdateSpeedAction = Mode7_SetConstSpeedXY;
    }

    private void Mode7_SetSpeedY_ResetSpeedX(float[] mechParams, int offset)
    {
        Speed = new Vector2(0, mechParams[offset]);
        UpdateSpeedAction = Mode7_SetConstSpeedXY;
    }

    private void Mode7_SetSpeedX(float[] mechParams, int offset)
    {
        Speed = Speed with { X = mechParams[offset] };
        UpdateSpeedAction = Mode7_SetConstSpeedXY;
    }

    private void Mode7_SetSpeedY(float[] mechParams, int offset)
    {
        Speed = Speed with { Y = mechParams[offset] };
        UpdateSpeedAction = Mode7_SetConstSpeedXY;
    }

    private void Mode7_SetAccelerationXY_SetTargetSpeedXY(float[] mechParams, int offset)
    {
        Acceleration = new Vector2(mechParams[offset + 0], mechParams[offset + 1]);
        TargetSpeed = new Vector2(mechParams[offset + 2], mechParams[offset + 3]);
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedXY;
    }

    private void Mode7_SetAccelerationX_SetTargetSpeedX_ResetSpeedY(float[] mechParams, int offset)
    {
        Acceleration = Acceleration with { X = mechParams[offset + 0] };
        TargetSpeed = TargetSpeed with { X = mechParams[offset + 1] };
        Speed = Speed with { Y = 0 };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedX;
    }

    private void Mode7_SetAccelerationY_SetTargetSpeedY_ResetSpeedX(float[] mechParams, int offset)
    {
        Acceleration = Acceleration with { Y = mechParams[offset + 0] };
        TargetSpeed = TargetSpeed with { Y = mechParams[offset + 1] };
        Speed = Speed with { X = 0 };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedY;
    }

    private void Mode7_SetAccelerationX_SetTargetSpeedX(float[] mechParams, int offset)
    {
        Acceleration = Acceleration with { X = mechParams[offset + 0] };
        TargetSpeed = TargetSpeed with { X = mechParams[offset + 1] };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedXY;
    }

    // Unused due to bug mentioned above
    private void Mode7_SetAccelerationY_SetTargetSpeedY(float[] mechParams, int offset)
    {
        Acceleration = Acceleration with { Y = mechParams[offset + 0] };
        TargetSpeed = TargetSpeed with { Y = mechParams[offset + 1] };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedXY;
    }

    private void Mode7_SetSpeedXY_SetAccelerationXY_SetTargetSpeedXY(float[] mechParams, int offset)
    {
        Speed = new Vector2(mechParams[offset + 0], mechParams[offset + 1]);
        Acceleration = new Vector2(mechParams[offset + 2], mechParams[offset + 3]);
        TargetSpeed = new Vector2(mechParams[offset + 4], mechParams[offset + 5]);
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedXY;
    }

    private void Mode7_SetSpeedX_SetAccelerationX_SetTargetSpeedX_ResetSpeedY(float[] mechParams, int offset)
    {
        Speed = new Vector2(mechParams[offset + 0], 0);
        Acceleration = Acceleration with { X = mechParams[offset + 1] };
        TargetSpeed = TargetSpeed with { X = mechParams[offset + 2] };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedX;
    }

    private void Mode7_SetSpeedY_SetAccelerationY_SetTargetSpeedY_ResetSpeedX(float[] mechParams, int offset)
    {
        Speed = new Vector2(0, mechParams[offset + 0]);
        Acceleration = Acceleration with { Y = mechParams[offset + 1] };
        TargetSpeed = TargetSpeed with { Y = mechParams[offset + 2] };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedY;
    }

    private void Mode7_SetSpeedX_SetAccelerationX_SetTargetSpeedX(float[] mechParams, int offset)
    {
        Speed = Speed with { X = mechParams[offset + 0] };
        Acceleration = Acceleration with { X = mechParams[offset + 1] };
        TargetSpeed = TargetSpeed with { X = mechParams[offset + 2] };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedXY;
    }

    private void Mode7_SetSpeedY_SetAccelerationY_SetTargetSpeedY(float[] mechParams, int offset)
    {
        Speed = Speed with { Y = mechParams[offset + 0] };
        Acceleration = Acceleration with { Y = mechParams[offset + 1] };
        TargetSpeed = TargetSpeed with { Y = mechParams[offset + 2] };
        UpdateSpeedAction = Mode7_SetAcceleratedSpeedXY;
    }

    private void Mode7_UseConstantSpeed(float[] mechParams, int offset)
    {
        UpdateSpeedAction = Mode7_SetConstSpeedXY;
    }

    public void Init(int type, FixedPointInt32[] mechParams)
    {
        float[] floatParams = new float[mechParams?.Length ?? 0];
        for (int i = 0; i < floatParams.Length; i++)
            floatParams[i] = mechParams![i];

        Init(type, floatParams);
    }

    public void Init(int type, float[] mechParams)
    {
        _initActions[type](mechParams, 0);
    }

    public void Init(int type)
    {
        _initActions[type](null, 0);
    }

    public void Reset()
    {
        Speed = Vector2.Zero;
        Acceleration = Vector2.Zero;
        TargetSpeed = Vector2.Zero;
        UpdateSpeedAction = SetConstSpeedXY;
    }
}