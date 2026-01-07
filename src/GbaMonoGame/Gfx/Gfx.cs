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
    private static bool[] _drawnSpriteLayers = new bool[4];

    /// <summary>
    /// A texture which is a single uncolored 1x1 pixel. Useful for drawing shapes.
    /// </summary>
    public static Texture2D Pixel { get; private set; }

    /// <summary>
    /// The game screens. These are the equivalent of backgrounds on the GBA
    /// and there are always 4 of these.
    /// </summary>
    public static List<GfxScreen> Screens { get; } = [];

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

    public static GbaRenderTarget[] SpriteRenderTargets { get; set; }
    public static FixedResolutionRenderContext SpriteRenderTargetRenderContext { get; set; }

    private static void DrawSpritesToRenderTargets(GfxRenderer renderer)
    {
        // Update the sprite render target render context resolution to match the full view port size
        // since each layer should be drawn to a texture which represents the entire screen
        SpriteRenderTargetRenderContext.SetResolution(Engine.GameViewPort.FullSize);

        // Draw each game layer (3-0, although the order technically doesn't matter here)
        for (int i = 3; i >= 0; i--)
        {
            // Check if this layer has any sprites to draw
            _drawnSpriteLayers[i] = false;
            for (int j = 0; j < BackSprites.Count; j++)
            {
                Sprite sprite = BackSprites[j];
                if (sprite.Priority == i && !sprite.RenderOptions.UseDepthStencil)
                {
                    _drawnSpriteLayers[i] = true;
                    break;
                }
            }
            if (!_drawnSpriteLayers[i])
            {
                for (int j = Sprites.Count - 1; j >= 0; j--)
                {
                    Sprite sprite = Sprites[j];
                    if (sprite.Priority == i && !sprite.RenderOptions.UseDepthStencil)
                    {
                        _drawnSpriteLayers[i] = true;
                        break;
                    }
                }
            }

            // Ignore if no sprites to draw for optimization
            if (!_drawnSpriteLayers[i])
                continue;

            // Get the render target
            GbaRenderTarget renderTarget = SpriteRenderTargets[i];

            // Update the size to match the full view port size
            renderTarget.SetSize(Engine.GameViewPort.FullSize.ToFloorPoint());

            // Begin rendering to the render target
            renderTarget.BeginRender();

            // Clear the buffer with transparency
            Engine.GraphicsDevice.Clear(Color.Transparent);

            // Draw the sprites. Make sure to change from AlphaBlend to AlphaBlendOverwrite since we don't want
            // to blend the sprites on the same layer. Also ignore any sprites using the depth stencil since we
            // can't draw them like this. This is only the case for Mode7 sprites and alpha blending isn't really
            // an issue there in the same way as in the other levels.
            for (int j = 0; j < BackSprites.Count; j++)
            {
                Sprite sprite = BackSprites[j];
                if (sprite.Priority == i && !sprite.RenderOptions.UseDepthStencil)
                {
                    if (sprite.RenderOptions.BlendMode == BlendMode.AlphaBlend)
                        sprite.RenderOptions.BlendMode = BlendMode.AlphaBlendOverwrite;

                    sprite.Draw(renderer, Color);
                }
            }
            for (int j = Sprites.Count - 1; j >= 0; j--)
            {
                Sprite sprite = Sprites[j];
                if (sprite.Priority == i && !sprite.RenderOptions.UseDepthStencil)
                {
                    if (sprite.RenderOptions.BlendMode == BlendMode.AlphaBlend)
                        sprite.RenderOptions.BlendMode = BlendMode.AlphaBlendOverwrite;

                    sprite.Draw(renderer, Color);
                }
            }

            // Force the current rendering to end (ending current sprite batches etc.) so everything gets drawn
            // to the render target before we remove it
            renderer.EndRender();

            // End rendering to the render target
            renderTarget.EndRender();
        }
    }

    private static void DrawGameLayer(GfxRenderer renderer, int layer)
    {
        // Draw screens
        for (int i = 0; i < Screens.Count; i++)
        {
            GfxScreen screen = Screens[i];
            if (screen.IsEnabled && screen.Priority == layer)
                screen.Draw(renderer, Color);
        }

        if ((FadeControl.Flags & (FadeFlags)(1 << layer)) != 0)
            DrawFade(renderer);

        if (Rom.IsLoaded && (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage))
        {
            // Draw sprites using depth stencil since they couldn't be drawn to the layer render target
            for (int j = 0; j < BackSprites.Count; j++)
            {
                Sprite sprite = BackSprites[j];
                if (sprite.Priority == layer && sprite.RenderOptions.UseDepthStencil)
                    sprite.Draw(renderer, Color);
            }
            for (int j = Sprites.Count - 1; j >= 0; j--)
            {
                Sprite sprite = Sprites[j];
                if (sprite.Priority == layer && sprite.RenderOptions.UseDepthStencil)
                    sprite.Draw(renderer, Color);
            }

            // Draw the sprite layer render target if it was drawn to
            if (_drawnSpriteLayers[layer])
            {
                renderer.BeginSpriteRender(new RenderOptions
                {
                    RenderContext = SpriteRenderTargetRenderContext,
                    BlendMode = BlendMode.AlphaBlend,
                });
                renderer.Draw(SpriteRenderTargets[layer].RenderTarget, Vector2.Zero, Color.White);
            }
        }
        else
        {
            // Draw sprites normally
            for (int j = 0; j < BackSprites.Count; j++)
            {
                Sprite sprite = BackSprites[j];
                if (sprite.Priority == layer)
                    sprite.Draw(renderer, Color);
            }
            for (int j = Sprites.Count - 1; j >= 0; j--)
            {
                Sprite sprite = Sprites[j];
                if (sprite.Priority == layer)
                    sprite.Draw(renderer, Color);
            }
        }

        if ((FadeControl.Flags & (FadeFlags)(1 << (layer + 4))) != 0)
            DrawFade(renderer);
    }

    private static void DrawFade(GfxRenderer renderer)
    {
        if ((!Rom.IsLoaded || Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage) && 
            FadeControl.Mode != FadeMode.None && 
            Fade is > 0 and <= 1)
        {
            renderer.BeginSpriteRender(new RenderOptions()
            {
                RenderContext = Engine.GameRenderContext,
            });

            switch (FadeControl.Mode)
            {
                case FadeMode.AlphaBlending:
                    throw new InvalidOperationException("Alpha blending should be handled per screen and object!");

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

        // Create the render targets for each sprite later. Make sure the surface format is set to Color
        // so that we preserve transparency!
        SpriteRenderTargets = 
        [
            new GbaRenderTarget(Engine.GraphicsDevice, SurfaceFormat.Color, DepthFormat.None),
            new GbaRenderTarget(Engine.GraphicsDevice, SurfaceFormat.Color, DepthFormat.None),
            new GbaRenderTarget(Engine.GraphicsDevice, SurfaceFormat.Color, DepthFormat.None),
            new GbaRenderTarget(Engine.GraphicsDevice, SurfaceFormat.Color, DepthFormat.None),
        ];

        // Create the render context for drawing the sprite render targets. This should match the full screen
        // size and we don't want to force it to fit within the game's viewport since any black space around it
        // would be included in the render target texture anyway.
        SpriteRenderTargetRenderContext = new FixedResolutionRenderContext(Engine.GameViewPort.FullSize, fitToGameViewPort: false);
    }

    public static void AddScreen(GfxScreen screen)
    {
        if (Screens.Any(x => x.Id == screen.Id))
            throw new Exception($"A screen with the id {screen.Id} has already been added");

        Screens.Add(screen);
    }
    public static GfxScreen GetScreen(int id) => Screens.FirstOrDefault(x => x.Id == id);
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

    public static void Clear()
    {
        ClearScreens();
        ClearSprites();
        ClearScreenEffect();
        Color = Color.White;
        ClearColor = Color.Black;
        Fade = 0;
        FadeControl = FadeControl.None;
    }

    public static void Draw(GfxRenderer renderer)
    {
        if (Rom.IsLoaded && (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage))
        {
            // First draw the sprites to a render target for each layer. This is because alpha has to be managed
            // separately for sprites in order to match GBA behavior (i.e. sprites on the same layer should
            // not blend with each other - instead they overwrite each others pixels).
            DrawSpritesToRenderTargets(renderer);
        }

        // Draw clear color on GBA
        if (Rom.IsLoaded && (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage))
        {
            renderer.BeginSpriteRender(new RenderOptions()
            {
                RenderContext = Engine.GameRenderContext,
            });
            renderer.DrawFilledRectangle(Vector2.Zero, Engine.GameRenderContext.Resolution, ClearColor);
        }

        // Draw each game layer (3-0)
        for (int i = 3; i >= 0; i--)
            DrawGameLayer(renderer, i);

        // Draw screen fade if no special flag is set
        if (FadeControl.Flags == FadeFlags.Default)
            DrawFade(renderer);

        // Draw the screen effect on GBA if there is one
        if (Rom.IsLoaded && (Rom.Platform == Platform.GBA || Engine.ActiveConfig.Tweaks.UseGbaEffectsOnNGage))
            ScreenEffect?.Draw(renderer);
    }
}