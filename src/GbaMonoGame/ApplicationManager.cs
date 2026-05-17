using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class ApplicationManager
{
    public ApplicationManager(Game game)
    {
        _game = game;
    }

    private readonly Game _game;

    public void Exit()
    {
        _game.Exit();
    }
}