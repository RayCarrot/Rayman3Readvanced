namespace GbaMonoGame.Rayman3;

public partial class BoulderMode7
{
    private const int RENDER_HEIGHT = 64;
    private const float POSITION_Z = 0;

    private const float ROTATION_SPEED = 3;
    private const float BOUNCE_SPEED = 8;
    private const float BOUNCE_ACCELERATION = 1 / 4f;

    private readonly Vector2 SQUASH_DELTA = new(-1 / 32f, 1 / 4f);
    private const float SQUASH_TARGET_Y = 2.5f;

    private readonly Vector2 PLAYER_SQUASH_SCALE = new(0.5f, 2);
    private const byte PLAYER_SQUASH_TIME = 100;
}