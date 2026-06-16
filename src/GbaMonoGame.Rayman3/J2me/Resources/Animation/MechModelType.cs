namespace GbaMonoGame.Rayman3.J2me;

public enum MechModelType : byte
{
    Reset = 0,
    UseConstantSpeed = 1,
    None = 2,
    SetSpeedXY = 3,
    SetSpeedX_ResetSpeedY = 4,
    SetSpeedY_ResetSpeedX = 5,
    SetSpeedX = 6,
    SetSpeedY = 7,
    SetAccelerationXY_SetTargetSpeedXY = 8,
    SetAccelerationX_SetTargetSpeedX = 9,
    SetAccelerationY_SetTargetSpeedY = 10,
    SetSpeedXY_SetAccelerationXY_SetTargetSpeedXY = 11,
    SetSpeedX_SetAccelerationX_SetTargetSpeedX = 12,
    SetSpeedY_SetAccelerationY_SetTargetSpeedY = 13,
}