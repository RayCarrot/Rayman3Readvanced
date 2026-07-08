namespace GbaMonoGame.Rayman3;

public partial class RaymanMode7
{
    public const int RENDER_HEIGHT = 64;
    public const float POSITION_Z = 0;

    public const float CLOUDS_SCROLL_SPEED = 1 / 4f; // NOTE: The original game scrolls by 1 every 4 frames

    public const float DEFAULT_HORIZON = 67;
    public const float JUMP_HORIZON_FACTOR = 1 / 8f;
    public const float DEFAULT_TEXT_LAYER_SCROLL_Y = 11;

    public const float MOVE_SPEED = 1.375f;
    public const float SPEED_FACTOR = 1 / 60f;
    public const float SLOWDOWN_SPEED_FACTOR = 1 / 4f;

    public const float JUMP_HEIGHT_SPEED = 8;
    public const float JUMP_HEIGHT_DEACCELERATION = 3 / 8f;

    public const byte INVULNERABILITY_TIME = 100;

    public const float BASE_PITCH = 192;
    public const float SCREEN_PITCH_FACTOR = 16;

    public const byte DEATH_FOG_FADE_OUT_TIME = 80;
    public const float DEATH_FOG_FADE_OUT_FACTOR = 1 / 4f;
    public const byte DEATH_FADE_OUT_TIME = 90;
    public const byte DEATH_TIME = 167;
}