using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Unload textures
public class TitleScreen : Frame
{
    public AnimationPlayer AnimationPlayer { get; set; }
    public TransitionsFX TransitionsFX { get; set; } // TODO: Fade-out while loading ROM

    public Cursor Cursor { get; set; }
    public TitleScreenGame[] Games { get; set; }
    public int SelectedGameIndex { get; set; }

    public override void Init()
    {
        AnimationPlayer = new AnimationPlayer(false, null);
        TransitionsFX = new TransitionsFX(true);

        Texture2D gbaBackground = Engine.ContentManager.Load<Texture2D>("GBA Title Screen");
        Texture2D nGageBackground = Engine.ContentManager.Load<Texture2D>("N-Gage Title Screen");

        Gfx.AddScreen(new GfxScreen(0)
        {
            IsEnabled = true,
            Priority = 1,
            Offset = Vector2.Zero,
            Renderer = new TextureScreenRenderer(gbaBackground),
        });
        Gfx.AddScreen(new GfxScreen(1)
        {
            IsEnabled = false,
            Priority = 1,
            Offset = Vector2.Zero,
            Renderer = new TextureScreenRenderer(nGageBackground),
        });

        Cursor = new Cursor();

        Games =
        [
            new TitleScreenGame(Platform.GBA, Cursor, new Vector2(65, 172)),
            new TitleScreenGame(Platform.NGage, Cursor, new Vector2(255, 172))
        ];

        SelectedGameIndex = 0;
        Games[0].SelectedIndex = 0;
    }

    public override void UnInit()
    {

    }

    public override void Step()
    {
        if (!Cursor.IsMoving && (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.Right)))
        {
            int prevSelectedGameIndex = SelectedGameIndex;
            SelectedGameIndex = SelectedGameIndex == 1 ? 0 : 1;

            Games[SelectedGameIndex].SelectedIndex = 0;
            Games[prevSelectedGameIndex].SelectedIndex = -1;

            Gfx.GetScreen(SelectedGameIndex).IsEnabled = true;
            Gfx.GetScreen(prevSelectedGameIndex).IsEnabled = false;
        }

        foreach (TitleScreenGame game in Games)
            game.Step();

        Cursor.Step();

        foreach (TitleScreenGame game in Games)
            game.Draw(AnimationPlayer);

        Cursor.Draw(AnimationPlayer);

        AnimationPlayer.Execute();
    }
}