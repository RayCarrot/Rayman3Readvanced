using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3;

public partial class Hoodstormer
{
    public const float FLY_IN_DETECTION_DISTANCE_X = 220;
    public const float FLY_AWAY_DETECTION_DISTANCE_X = 360;

    public readonly float FLY_IN_SPEED_X = Rom.Platform switch
    {
        Platform.GBA => 1.5f,
        Platform.NGage => 1,
        _ => throw new UnsupportedPlatformException(),
    };
    public readonly float ATTACK_SPEED_X = Rom.Platform switch
    {
        Platform.GBA => 1.125F,
        Platform.NGage => 0.75F,
        _ => throw new UnsupportedPlatformException(),
    };
    public const float FLY_AWAY_SPEED_X = 2;

    public const float MISSILE_PROJECTILE_OFFSET_X = 58;
    public const float MISSILE_PROJECTILE_OFFSET_Y = -8;
}