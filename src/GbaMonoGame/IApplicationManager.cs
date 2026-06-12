namespace GbaMonoGame;

public interface IApplicationManager
{
    bool IsActive { get; }
    public float Framerate { get; set; }

    public float DeltaTime => 1 / Framerate;
    public float GbaDeltaTime => 60 / Framerate;

    void BeginLoad();
    void Exit();
}