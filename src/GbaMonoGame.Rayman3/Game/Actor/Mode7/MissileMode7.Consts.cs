namespace GbaMonoGame.Rayman3;

public partial class MissileMode7
{
    public const float COLLISION_HEIGHT = 16;

    public const float MIN_SPEED = 1 / 1024f;
    public const float MAX_SPEED = 7;

    public const float STRAFE_SPEED_FACTOR = 1 / 64f;

    public const float DEFAULT_FRICTION = 10 / 512f;
    public const float BRAKE_FRICTION = 24 / 512f;
    public const float FINISHED_FRICTION = 8 / 512f;

    public const float DEFAULT_ACCELERATION = 32 / 512f;
    public const float MAX_ACCELERATION = 40 / 512f;
    public const float MULTIPLAYER_DIST_ACCELERATION_FACTOR = 1 / 512f;

    public const float DIRECTION_SPEED = 2;
    public const float DIRECTION_ANGLE_RANGE = 80;

    public const float JUMP_HEIGHT_SPEED = 8;
    public const float JUMP_HEIGHT_DEACCELERATION = 3 / 8f;
    public const float JUMP_SCALE_X = 288 / 256f;
    public const float JUMP_SCALE_Y = 224 / 256f;
    public const float LAND_SCALE_X = 192 / 256f;
    public const float LAND_SCALE_Y = 284 / 256f;

    public const float RESTORE_SCALE_DELTA_X = 8 / 256f;
    public const float RESTORE_SCALE_DELTA_Y = -16 / 256f;

    public const float PITCH_FACTOR = 512;
    public const float DEFAULT_JUMP_PITCH_ADD = 1024;
    public const float MULTIPLAYER_JUMP_PITCH_ADD = 256;

    public const byte BOOST_TIME = 192;
    public const byte BOOST_SOUND_TIME = BOOST_TIME - 16;

    public const ushort INVULNERABILITY_TIME = 180;
    public const ushort MULTIPLAYER_FINISHED_INVULNERABILITY_TIME = 800;

    public const byte JUMP_SOUND_TIME = 15;
    public const byte WAHOO_SOUND_TIME = 15;

    public const ushort DEATH_FADE_OUT_TIME = 90;
    public const ushort DEATH_TIME = 120;

    public const ushort MULTIPLAYER_DEATH_NEXT_SPECTATE_TIME = 15;
    public const ushort MULTIPLAYER_DEATH_FADE_IN_TIME = 20;
    public const ushort MULTIPLAYER_DEATH_RESET_TIME = 35;

    public const ushort FINISHED_FADE_OUT_MIN_TIME = 300;
    public const ushort FINISHED_FADE_OUT_TIME = 974;
    public const ushort FINISHED_STOP_SOUNDS_TIME = 998;
    public const ushort FINISHED_TIME = 1000;
}