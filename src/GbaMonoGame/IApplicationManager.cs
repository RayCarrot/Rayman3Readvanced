namespace GbaMonoGame;

public interface IApplicationManager
{
    bool IsActive { get; }

    void BeginLoad();
    void Exit();
}