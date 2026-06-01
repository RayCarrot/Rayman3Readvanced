namespace GbaMonoGame;

public class ApplicationManager : IApplicationManager
{
    public ApplicationManager(GbaGame game)
    {
        _game = game;
    }

    private readonly GbaGame _game;

    public bool IsActive => _game.IsActive;
    public bool IsLoading { get; set; }
    
    public float Framerate
    {
        get => _game.Framerate;
        set => _game.SetFramerate(value);
    }

    public void BeginLoad()
    {
        IsLoading = true;
    }

    public void Exit()
    {
        _game.Exit();
    }
}