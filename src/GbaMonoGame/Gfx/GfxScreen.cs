using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

/// <summary>
/// A graphics screen. This is the equivalent of a GBA background.
/// </summary>
public class GfxScreen
{
    public GfxScreen(int id)
    {
        Id = id;
    }

    /// <summary>
    /// The screen id, a value between 0 and 3.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The screen drawing priority, a value between 0 and 3, or -1 for always on top
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Indicates the overflow mode for the screen, if it should wrap its content or not.
    /// </summary>
    public bool Wrap { get; set; }

    public int CurrentWrapX { get; private set; }
    public int CurrentWrapY { get; private set; }

    /// <summary>
    /// Indicates the color mode for the screen, if it's 8-bit or 4-bit.
    /// </summary>
    public bool? Is8Bit { get; set; }

    /// <summary>
    /// The scrolled screen offset.
    /// </summary>
    public Vector2 Offset { get; set; }

    public RenderOptions RenderOptions { get; } = new();

    public float Alpha { get; set; }
    public float GbaAlpha
    {
        get => Alpha * 16;
        set => Alpha = value / 16;
    }

    /// <summary>
    /// Indicates if the screen is enabled and should be drawn.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The renderer to draw the screen with. This is used in place of tilemap and tileset
    /// data since the screen might need to be drawn differently in different situations.
    /// </summary>
    public IScreenRenderer Renderer { get; set; }

    public void Draw(GfxRenderer renderer, Color color)
    {
        if (Renderer == null)
            return;

        if (Rom.IsLoaded &&
            (Rom.Platform == Platform.GBA || Engine.Config.Tweaks.UseGbaEffectsOnNGage) && 
            RenderOptions.BlendMode != BlendMode.None)
            color = new Color(color, Alpha);

        // We can't wrap if the camera is in 3D
        if (Wrap && RenderOptions.WorldViewProj == null)
        {
            // Get the normal size of the background. This is used to wrapping.
            Vector2 size = Renderer.GetSize(this);

            // Get the actual area we render the background to as some backgrounds might render outside their normal size.
            Box renderBox = Renderer.GetRenderBox(this);

            // Get the camera bounds
            const float camMinX = 0;
            const float camMinY = 0;
            float maxResX = RenderOptions.RenderContext.Resolution.X;
            float maxResY = RenderOptions.RenderContext.Resolution.Y;

            // Get the background position and wrap it
            Vector2 wrappedPos = new(MathHelpers.Mod(-Offset.X, size.X), MathHelpers.Mod(-Offset.Y, size.Y));

            // Calculate the start and end positions to draw the background
            float startX = camMinX - size.X + (wrappedPos.X == 0 ? size.X : wrappedPos.X);
            float startY = camMinY - size.Y + (wrappedPos.Y == 0 ? size.Y : wrappedPos.Y);

            float fullWidth = maxResX - startX;
            float fullHeight = maxResY - startY;

            Vector2 wrappedEnd = new(fullWidth % size.X, fullHeight % size.Y);
            float endX = maxResX + size.X - (wrappedEnd.X == 0 ? size.X : wrappedEnd.X);
            float endY = maxResY + size.Y - (wrappedEnd.Y == 0 ? size.Y : wrappedEnd.Y);

            // Extend for the visible area if needed.
            // NOTE: This only accounts for if the render box is bigger than then the size, which we do to prevent pop-in.
            //       But it does not account for if it's smaller. If it's smaller, then this isn't fully optimized as we might
            //       be performing unnecessary draw calls.
            if (renderBox.Left < 0 && endX + renderBox.Left < maxResX)
                endX += size.X;
            if (renderBox.Top < 0 && endY + renderBox.Top < maxResY)
                endY += size.Y;
            if (renderBox.Right > size.X && startX + renderBox.Right > camMinX)
                startX -= size.X;
            if (renderBox.Bottom > size.Y && startY + renderBox.Bottom > camMinY)
                startY -= size.Y;

            // Draw the background to fill out the visible range
            CurrentWrapY = 0;
            for (float y = startY; y < endY; y += size.Y)
            {
                CurrentWrapX = 0;

                for (float x = startX; x < endX; x += size.X)
                {
                    Renderer?.Draw(renderer, this, new Vector2(x, y), color);
                    CurrentWrapX++;
                }

                CurrentWrapY++;
            }
        }
        else
        {
            Renderer?.Draw(renderer, this, -Offset, color);

            CurrentWrapX = 1;
            CurrentWrapY = 1;
        }
    }
}