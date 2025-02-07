using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

/// <summary>
/// Manages the graphics to be drawn on screen. This is in place of GBA VRAM,
/// where screens are the background and sprites are the objects.
/// </summary>
public static class Gfx
{
    /// <summary>
    /// A texture which is a single uncolored 1x1 pixel. Useful for drawing shapes.
    /// </summary>
    public static Texture2D Pixel { get; private set; }

    /// <summary>
    /// The game screens. These are the equivalent of backgrounds on the GBA
    /// and there are always 4 of these.
    /// </summary>
    public static Dictionary<int, GfxScreen> Screens { get; } = [];

    /// <summary>
    /// The game sprites. These are the equivalent of objects on the GBA.
    /// Unlike the GBA there is no defined maximum sprite count.
    /// </summary>
    public static List<Sprite> Sprites { get; } = [];

    /// <summary>
    /// Same as <see cref="Sprites"/>, but for sprites which are added in
    /// last. Sometimes the game adds sprites at the end of OAM to make
    /// sure they get a different priority.
    /// </summary>
    public static List<Sprite> BackSprites { get; } = [];

    /// <summary>
    /// The screen effect to apply, or null if there is none. This is used
    /// in place for screen effects made using GBA features such as windows.
    /// </summary>
    public static ScreenEffect ScreenEffect { get; set; }

    /// <summary>
    /// The color to draw textures with. This can be used to simulate fading a palette.
    /// </summary>
    public static Color Color { get; set; } = Color.White;

    /// <summary>
    /// The screen color. On GBA this is set from background palette color 0, while on
    /// N-Gage it's unimplemented. Normally this is set to black.
    /// </summary>
    public static Color ClearColor { get; set; } = Color.Black;

    /// <summary>
    /// The fade coefficient, a value between 0 and 1. This is the equivalent to BLDY
    /// on GBA and is not implemented on N-Gage.
    /// </summary>
    public static float Fade { get; set; }
    
    /// <summary>
    /// Same as <see cref="Fade"/>, but using a range of 0 to 16.
    /// </summary>
    public static float GbaFade
    {
        get => Fade * 16;
        set => Fade = value / 16;
    }

    /// <summary>
    /// This defines how the <see cref="Fade"/> is applied and is the equivalent of
    /// BLDCNT on GBA. This is not implemented on N-Gage.
    /// </summary>
    public static FadeControl FadeControl { get; set; }

    private static void DrawFade(GfxRenderer renderer)
    {
        // TODO: Add config option to use GBA fading on N-Gage
        if (Rom.IsLoaded && Rom.Platform == Platform.GBA && FadeControl.Mode != FadeMode.None && Fade is > 0 and <= 1)
        {
            renderer.BeginRender(new RenderOptions()
            {
                RenderContext = Engine.GameRenderContext,
            });

            switch (FadeControl.Mode)
            {
                case FadeMode.AlphaBlending:
                    throw new NotImplementedException();

                case FadeMode.BrightnessIncrease:
                    renderer.DrawFilledRectangle(Vector2.Zero, Engine.GameRenderContext.Resolution, Color.White * Fade);
                    break;

                case FadeMode.BrightnessDecrease:
                    renderer.DrawFilledRectangle(Vector2.Zero, Engine.GameRenderContext.Resolution, Color.Black * Fade);
                    break;
            }
        }
    }

    public static void Load()
    {
        Pixel = new Texture2D(Engine.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        Pixel.SetData([Color.White]);
    }

    public static void AddScreen(GfxScreen screen) => Screens.Add(screen.Id, screen);
    public static GfxScreen GetScreen(int id) => Screens[id];
    public static void ClearScreens() => Screens.Clear();

    public static void AddSprite(Sprite sprite) => Sprites.Add(sprite);
    public static void AddBackSprite(Sprite sprite) => BackSprites.Add(sprite);
    public static void ClearSprites()
    {
        Sprites.Clear();
        BackSprites.Clear();
    }

    public static void SetScreenEffect(ScreenEffect screenEffect) => ScreenEffect = screenEffect;
    public static void ClearScreenEffect() => ScreenEffect = null;

    public static void Draw(GfxRenderer renderer)
    {
        // Draw clear color on GBA
        if (Rom.IsLoaded && Rom.Platform == Platform.GBA)
        {
            renderer.BeginRender(new RenderOptions()
            {
                RenderContext = Engine.GameRenderContext,
            });
            renderer.DrawFilledRectangle(Vector2.Zero, Engine.GameRenderContext.Resolution, ClearColor);
        }

        // Draw each game layer (3-0)
        for (int i = 3; i >= 0; i--)
        {
            // Draw screens
            foreach (GfxScreen screen in Screens.Values.Where(x => x.IsEnabled && x.Priority == i))
                screen.Draw(renderer, Color);

            if ((FadeControl.Flags & (FadeFlags)(1 << i)) != 0)
                DrawFade(renderer);

            // Draw sprites
            foreach (Sprite sprite in BackSprites.Where(x => x.Priority == i))
                sprite.Draw(renderer, Color);
            foreach (Sprite sprite in Sprites.Where(x => x.Priority == i).Reverse())
                sprite.Draw(renderer, Color);

            if ((FadeControl.Flags & (FadeFlags)(1 << (i + 4))) != 0)
                DrawFade(renderer);
        }

        // Draw screen fade if no special flag is set
        if (FadeControl.Flags == FadeFlags.Default)
            DrawFade(renderer);

        // TODO: Add option to use this on N-Gage
        // Draw the screen effect on GBA if there is one
        if (Rom.IsLoaded && Rom.Platform == Platform.GBA)
            ScreenEffect?.Draw(renderer);
    }
}