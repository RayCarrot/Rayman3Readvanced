using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3;

public partial class Grolgoth
{
    public readonly int GROUND_FALL_DOWN_RIGHT_POSITION_X = Rom.Platform switch
    {
        Platform.GBA => 200,
        Platform.NGage => 151,
        _ => throw new UnsupportedPlatformException(),
    };
    public readonly int GROUND_FALL_DOWN_LEFT_POSITION_X = Rom.Platform switch
    {
        Platform.GBA => 40,
        Platform.NGage => 25,
        _ => throw new UnsupportedPlatformException(),
    };
    public const int GROUND_INITIAL_POSITION_Y = -30;

    public const int GROUND_INITIAL_HEALTH = 5;

    public readonly object GROUND_CAMERA_TARGET = Vector2.Zero;
    
    public const int GROUND_DEPLOY_BOMBS_TIME = 5;
    public const int GROUND_SHOOT_ENERGY_SHOTS_TIME = 20;
    public const int GROUND_SHOOT_LASERS_TIME = 10;
    public const int GROUND_HIT_TIME = 30;

    public const byte GROUND_INITIAL_ATTACK_COUNT = 8;
    public const byte GROUND_HEALTH_5_ATTACK_COUNT = 4;
    public const byte GROUND_HEALTH_4_ATTACK_COUNT = 5;
    public const byte GROUND_HEALTH_3_ATTACK_COUNT = 6;
    public const byte GROUND_HEALTH_2_ATTACK_COUNT = 7;
    public const byte GROUND_HEALTH_1_ATTACK_COUNT = 8;
    public const byte GROUND_DEPLOY_BOMBS_ATTACK_COUNT = 1;
    public const byte GROUND_SHOOT_LASERS_ATTACK_COUNT = 4;
    public readonly byte GROUND_HEALTH_4_FALLING_BOMBS_ATTACK_COUNT = Rom.Platform switch
    {
        Platform.GBA => 4,
        Platform.NGage => 3,
        _ => throw new UnsupportedPlatformException(),
    };
    public readonly byte GROUND_HEALTH_3_FALLING_BOMBS_ATTACK_COUNT = Rom.Platform switch
    {
        Platform.GBA => 6,
        Platform.NGage => 5,
        _ => throw new UnsupportedPlatformException(),
    };
    public const byte GROUND_HEALTH_2_BIG_GROUND_BOMBS_ATTACK_COUNT = 1;
    public const byte GROUND_HEALTH_1_BIG_GROUND_BOMBS_ATTACK_COUNT = 2;

    public const float GROUND_LASER_PROJECTILE_OFFSET_X = 16;
    public const float GROUND_ENERGY_SHOT_HIGH_PROJECTILE_OFFSET_X = 64;
    public const float GROUND_ENERGY_SHOT_HIGH_PROJECTILE_OFFSET_Y = 48;
    public const float GROUND_ENERGY_SHOT_LOW_PROJECTILE_OFFSET_X = 16;
    public const float GROUND_ENERGY_SHOT_LOW_PROJECTILE_OFFSET_Y = 72;
    public const float GROUND_ENERGY_SHOT_SPEED_X = 2.5f;
    public const float GROUND_SMALL_BOMB_PROJECTILE_OFFSET_X = 40;
    public const float GROUND_SMALL_BOMB_PROJECTILE_OFFSET_Y = 48;
    public const float GROUND_BIG_BOMB_PROJECTILE_OFFSET_X = 48;
    public readonly float GROUND_BIG_BOMB_PROJECTILE_POSITION_Y = Rom.Platform switch
    {
        Platform.GBA => 112,
        Platform.NGage => 160,
        _ => throw new UnsupportedPlatformException()
    };

    public const int GROUND_FALL_SPEED = 4;

    public readonly object GROUND_LAND_CAMERA_SHAKE_LENGTH = 96;

    public const int AIR_INITIAL_HEALTH = 5;

    public const byte AIR_INITIAL_ATTACK_COUNT = 8;
    public const byte AIR_INVALID_ATTACK_COUNT = 0xFF;
    public readonly byte AIR_FALLING_BOMBS_ATTACK_COUNT = Rom.Platform switch
    {
        Platform.GBA => 10,
        Platform.NGage => 6,
        _ => throw new UnsupportedPlatformException(),
    };
    public const byte AIR_SHOOT_ONE_SIDE_ATTACK_COUNT = 3;
    public const byte AIR_SHOOT_DOUBLE_SIDE_ATTACK_COUNT = AIR_SHOOT_ONE_SIDE_ATTACK_COUNT * 2;

    public const float AIR_SHOT_PROJECTILE_RIGHT_POSITION_X = 443;
    public const float AIR_SHOT_PROJECTILE_LEFT_POSITION_X = 30;
    public const float AIR_SHOT_PROJECTILE_1_POSITION_Y = 25;
    public const float AIR_SHOT_PROJECTILE_2_POSITION_Y = 75;
    public const float AIR_SHOT_PROJECTILE_3_POSITION_Y = 125;
    public const float AIR_SHOT_PROJECTILE_4_POSITION_Y = 50;
    public const float AIR_SHOT_PROJECTILE_5_POSITION_Y = 100;
    public const float AIR_SHOT_PROJECTILE_6_POSITION_Y = 150;
    public const float AIR_MISSILE_PROJECTILE_OFFSET_X = 72;
    public const float AIR_MISSILE_PROJECTILE_OFFSET_Y = -64;

    public const int AIR_OFF_SCREEN_OFFSET_Y = -250;

    public const int AIR_FALL_DOWN_RIGHT_POSITION_X = 423;
    public const int AIR_FALL_DOWN_LEFT_POSITION_X = 50;

    public const int AIR_INTRO_CAMERA_MOVE_TIME = 60;
    public const int AIR_INTRO_CAMERA_MOVE_OFFSET_X = -60;
    public const int AIR_INTRO_CAMERA_MOVE_OFFSET_Y = 20;
    public const int AIR_INTRO_FALL_DOWN_TIME = 120;

    public const int AIR_SHOOT_MISSILE_DEFAULT_TIME = 300;
    public const int AIR_SHOOT_MISSILE_HEALTH_1_TIME = 75;

    public const int AIR_DEPLOY_FALLING_BOMBS_MIN_POSITION_Y = 75;
    public const int AIR_SHOOT_MIN_POSITION_X = 100;
    public const int AIR_SHOOT_MAX_POSITION_X = 380;

    public readonly object AIR_HIT_CAMERA_TARGET_Y = 80;
    public const int AIR_HIT_SOUND_TIME = 20;
    public const int AIR_HIT_DETECTION_BOX_BOTTOM_OFFSET = -30;
}