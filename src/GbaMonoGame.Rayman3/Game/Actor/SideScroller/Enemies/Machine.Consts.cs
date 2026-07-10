namespace GbaMonoGame.Rayman3;

public partial class Machine
{
    public const int CANNON_OBJ_PRIORITY = 62;
    
    public const int CANNON_MAX_HEALTH = 3;

    public const ushort CANNON_INTRO_CAMERA_MOVE_TIME = 60;
    public const ushort CANNON_INTRO_FIRE_TIME = 130;
    public const ushort CANNON_INTRO_CAMERA_RETURN_TIME = 190;

    public const float CANNON_INTRO_CAMERA_TARGET_OFFSET_X = -60;
    public const float CANNON_INTRO_CAMERA_TARGET_OFFSET_Y = 20;

    public const ushort CANNON_UNINIT_TEXTBOX_TIME = 30;
    public const ushort CANNON_HEALTH_3_FIRE_TIME = 430;
    public const ushort CANNON_HEALTH_2_FIRE_TIME = 380;
    public const ushort CANNON_HEALTH_1_FIRE_TIME = 330;

    public const float CANNON_FALL_SPEED_Y = 2;

    public const float HELICOPTER_BOMB_OFFSET_X = -60;
    public const float HELICOPTER_BOMB_OFFSET_Y = 10;
    public const float EXPLOSION_OFFSET_X = -45;
    public const float EXPLOSION_OFFSET_Y = 0;

    // Matches the initial position on GBA, but they forgot to update this for the N-Gage version
    public const float CHECKPOINT_POSITION_X = 191;
    public const float CHECKPOINT_POSITION_Y = 136;

    public const int COG_WHEEL_INITIAL_OBJ_PRIORITY = 63; // Gets overriden
    public const int COG_WHEEL_OBJ_PRIORITY = 30;

    public const int COG_WHEEL_1_ROTATION_SPEED = 2;
    public const int COG_WHEEL_2_ROTATION_SPEED = -2;
    public const int COG_WHEEL_3_ROTATION_SPEED = 1;

    public const float COG_WHEEL_1_SCALE = 0x15000 / MathHelpers.FIXED_POINT_DIVISOR;
    public const float COG_WHEEL_2_SCALE = 0x10000 / MathHelpers.FIXED_POINT_DIVISOR;
    public const float COG_WHEEL_3_SCALE = 0x09999 / MathHelpers.FIXED_POINT_DIVISOR;

    public const float COG_WHEEL_FALL_SPEED_Y = 4;
}