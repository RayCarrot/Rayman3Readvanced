namespace GbaMonoGame.Rayman3;

public partial class BoulderMode7
{
    public const int RENDER_HEIGHT = 64;
    public const float POSITION_Z = 0;

    public const float ROTATION_SPEED = 3;
    public const float BOUNCE_SPEED = 8;
    public const float BOUNCE_ACCELERATION = 1 / 4f;

    public const float SQUASH_DELTA_X = -1 / 32f;
    public const float SQUASH_DELTA_Y = 1 / 4f;
    public const float SQUASH_TARGET_Y = 2.5f;

    public const float PLAYER_SQUASH_SCALE_X = 0.5f;
    public const float PLAYER_SQUASH_SCALE_Y = 2;
    public const byte PLAYER_SQUASH_TIME = 100;
}