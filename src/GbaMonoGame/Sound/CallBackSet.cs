namespace GbaMonoGame;

public abstract class CallBackSet
{
    public abstract Vector2 GetObjectPosition(object obj);
    public abstract Vector2 GetMikePosition(object obj);
    public abstract int GetSwitchIndex();
}