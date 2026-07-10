namespace GbaMonoGame.Rayman3;

public partial class Jano
{
    public const float LOW_OFFSET_Y = 200;
    public const float BASE_OFFSET_Y = 150;
    public const float HIGH_OFFSET_Y = 110;

    public const byte AMMO_PHASE_1 = 2;
    public const byte AMMO_PHASE_2 = 3;
    public const byte AMMO_PHASE_3 = 4;
    public const byte AMMO_SHOOT_MULTIPLE = 10;

    public const float SHOT_PROJECTILE_OFFSET_X = 64;
    public const float SHOT_PROJECTILE_OFFSET_Y = 32;
    public const float SHOT_PROJECTILE_LOW_SPEED_X = 2;
    public const float SHOT_PROJECTILE_LOW_SPEED_Y = -0.75f;
    public const float SHOT_PROJECTILE_BASE_SPEED_X = 2;
    public const float SHOT_PROJECTILE_BASE_SPEED_Y = 0;
    public const float SHOT_PROJECTILE_HIGH_SPEED_X = 2.5f;
    public const float SHOT_PROJECTILE_HIGH_SPEED_Y = 0.625f;

    public const float SKULL_PLATFORM_PROJECTILE_OFFSET_X = -32;
    public const float SKULL_PLATFORM_PROJECTILE_OFFSET_Y = 60;
    public const float SKULL_PLATFORM_SPEED_X = -1;

    public const ushort ATTACK_TIME = 60;
    public const ushort ATTACK_AGAIN_TIME = 30;
    public const ushort MOVE_CLOSER_TIME = 45;
    public const ushort COMPLETE_TIME = 90;

    public const float MIN_RAYMAN_DISTANCE_X = 50;
    public const float MIN_NOT_JUMPING_RAYMAN_DISTANCE_X = 120;
    public const float MAX_RAYMAN_DISTANCE_X = 170;

    public const float MOVE_CLOSER_SPEED_X = 2;
    public const float MOVE_AWAY_SPEED_X = 3;

    public const float CREATE_SKULL_PLATFORM_SPEED_X = 4;
    public const float CREATE_SKULL_PLATFORM_SPEED_Y = 2;
    public const float CREATE_SKULL_PLATFORM_MIN_DISTANCE_X = 150;
    public const ushort CREATE_SKULL_PLATFORM_TIME = 20;

    public const float SWITCH_SIDE_TARGET_Y = 20;
    public const float SWITCH_SIDE_SPEED_Y = 4;

    public const float ATTACK_SPEED_X = 4;

    public const float INTRO_START_POSITION_X = 375;
    public const float INTRO_SPEED_X = -2;
    public const float INTRO_TARGET_X = 480;
    public const ushort INTRO_GRIMACE_TIME = 30;
    public const ushort INTRO_END_TIME = 60;

    public const float PHASE_2_POSITION_X = 800;
    public const float PHASE_3_POSITION_X = 1320;
    public const float PHASE_4_POSITION_X = 1750;

    public const float MOVE_CLOSER_MIN_POSITION_X = 170;
    public const float MOVE_AWAY_MAX_POSITION_X = 1850;

    public const float CHECK_END_MIN_POSITION_X = 2050;
    public const float CHECK_END_SPEED_X = 2;
    public const float CHECK_END_TARGET_X = 1900;
    public const float CHECK_END_RAYMAN_MIN_POSITION_X = 1400;

    public const int PHASE_1_OFFSET_Y = 0;
    public const int PHASE_2_OFFSET_Y = -32;
    public const int PHASE_3_OFFSET_Y = -64;
    public const int PHASE_4_OFFSET_Y = -64;

    public const float ALPHA_BLEND_DELTA = 0.5f; // 1 step every other frame in the original game
    public const float ALPHA_BLEND_MIN = 4;
    public const float ALPHA_BLEND_MAX = AlphaCoefficient.MaxGbaValue;
}