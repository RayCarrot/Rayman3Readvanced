using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class ApplicationManager
{
    public ApplicationManager(Game game)
    {
        _game = game;
    }

    private readonly Game _game;

    public bool IsActive => _game.IsActive;
    public bool IsLoading { get; set; }

    public void BeginLoad()
    {
        IsLoading = true;
    }

    public void Exit()
    {
        _game.Exit();
    }
}