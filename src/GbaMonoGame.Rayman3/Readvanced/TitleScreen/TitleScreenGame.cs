using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TitleScreenGame : TitleScreenOptionsList
{
    public TitleScreenGame(RenderContext renderContext, Platform platform, Cursor cursor, Vector2 position) : base(renderContext, cursor, position)
    {
        Platform = platform;
    }

    public Platform Platform { get; }
}