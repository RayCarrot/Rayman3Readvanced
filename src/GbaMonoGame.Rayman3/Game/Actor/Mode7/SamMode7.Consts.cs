namespace GbaMonoGame.Rayman3;

public partial class SamMode7
{
    public const int RENDER_HEIGHT = 16;

    public const int TETHER_SPARKLE_OBJ_PRIORITY = 32;
    public const float TETHER_SPARKLE_BASE_Y = 24;

    public const float WATER_SPLASH_SPEED_FACTOR = -4;

    public const float DETECTION_DISTANCE = 18.98828125f; // 0x12FD00 fixed-point value

    public const float DIRECTION_SPEED = 2;

    public const float FINISHED_DEACCELERATION_X = 1 / 32f;

    public const ushort FINISHED_FADE_OUT_TIME = 140;
    public const ushort FINISHED_STOP_SOUNDS_TIME = 216;
    public const ushort FINISHED_TIME = 218;
}