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

    /// <summary>
    /// Indicates the color mode for the screen, if it's 8-bit or 4-bit.
    /// </summary>
    public bool? Is8Bit { get; set; }

    /// <summary>
    /// The scrolled screen offset.
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// The render context to use when rendering.
    /// </summary>
    public RenderContext RenderContext { get; set; }

    public bool IsAlphaBlendEnabled { get; set; }
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

        // TODO: Add config option to use GBA fading on N-Gage
        if (Rom.IsLoaded && Rom.Platform == Platform.GBA && IsAlphaBlendEnabled)
            color = new Color(color, Alpha);

        if (Wrap)
        {
            // Get the normal size of the background. This is used to wrapping.
            Vector2 size = Renderer.GetSize(this);

            // Get the actual area we render the background to as some backgrounds might render outside their normal size.
            Box renderBox = Renderer.GetRenderBox(this);

            // Get the camera bounds
            const float camMinX = 0;
            const float camMinY = 0;
            float maxResX = RenderContext.Resolution.X;
            float maxResY = RenderContext.Resolution.Y;

            // Get the background position and wrap it
            Vector2 wrappedPos = new(MathHelpers.Mod(-Offset.X, size.X), MathHelpers.Mod(-Offset.Y, size.Y));

            // Calculate the start and end positions to draw the background
            float startX = camMinX - size.X + (wrappedPos.X == 0 ? size.X : wrappedPos.X);
            float startY = camMinY - size.Y + (wrappedPos.Y == 0 ? size.Y : wrappedPos.Y);

            float fullWidth = startX + maxResX;
            float fullHeight = startY + maxResY;

            Vector2 wrappedEnd = new(fullWidth % size.X, fullHeight % size.Y);
            float endX = maxResX + size.X - (wrappedEnd.X == 0 ? size.X : wrappedEnd.X);
            float endY = maxResY + size.Y - (wrappedEnd.Y == 0 ? size.Y : wrappedEnd.Y);

            // Extend for the visible area if needed.
            // NOTE: This only accounts for if the render box is bigger than then the size, which we do to prevent pop-in.
            //       But it does not account for if it's smaller. If it's smaller, then this isn't fully optimized as we might
            //       be performing unnecessary draw calls.
            if (renderBox.MinX < 0 && endX + renderBox.MinX < maxResX)
                endX += size.X;
            if (renderBox.MinY < 0 && endY + renderBox.MinY < maxResY)
                endY += size.Y;
            if (renderBox.MaxX > size.X && startX + renderBox.MaxX > camMinX)
                startX -= size.X;
            if (renderBox.MaxY > size.Y && startY + renderBox.MaxY > camMinY)
                startY -= size.Y;

            // Draw the background to fill out the visible range
            for (float y = startY; y < endY; y += size.Y)
            {
                for (float x = startX; x < endX; x += size.X)
                {
                    Renderer?.Draw(renderer, this, new Vector2(x, y), color);
                }
            }
        }
        else
        {
            Renderer?.Draw(renderer, this, -Offset, color);
        }
    }
}