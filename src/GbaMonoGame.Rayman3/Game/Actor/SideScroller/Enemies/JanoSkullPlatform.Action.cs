namespace GbaMonoGame.Rayman3;

public partial class JanoSkullPlatform
{
    public new Action ActionId
    {
        get => (Action)base.ActionId;
        set => base.ActionId = (int)value;
    }

    public enum Action
    {
        Stationary = 0,
        Move_DownLeft = 1,
        DespawnDown = 2,
        SolidMove_Spawn = 3,
        SolidMove_Left = 4, // Unused
        SolidMove_Right = 5,
        Move_Up = 6, // Unused
        Move_Down = 7, // Unused
        Shake = 8,
        Despawn = 9,
        SolidMove_SpawnLeft = 10,
        Despawned = 11,
        Move_Left = 12,
        Collided = 13,
        FallDown = 14,
        SolidMove_Stationary = 15,
    }
}