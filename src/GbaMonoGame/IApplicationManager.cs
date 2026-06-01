namespace GbaMonoGame;

public interface IApplicationManager
{
    bool IsActive { get; }
    public float Framerate { get; set; }

    void BeginLoad();
    void Exit();
}