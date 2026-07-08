namespace GbaMonoGame.Rayman3;

public partial class BoulderMode7
{
    public const int RENDER_HEIGHT = 64;
    public const float POSITION_Z = 0;

    public const float ROTATION_SPEED = 3;
    public const float BOUNCE_SPEED = 8;
    public const float BOUNCE_ACCELERATION = 1 / 4f;

    public readonly Vector2 SQUASH_DELTA = new(-1 / 32f, 1 / 4f);
    public const float SQUASH_TARGET_Y = 2.5f;

    public readonly Vector2 PLAYER_SQUASH_SCALE = new(0.5f, 2);
    public const byte PLAYER_SQUASH_TIME = 100;
}