using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class GfxRenderer
{
    #region Constructor

    public GfxRenderer(GraphicsDevice graphicsDevice)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _rasterizerState = new RasterizerState() { CullMode = CullMode.None };

        _paletteShader = Gfx.PaletteShader;
        _paletteTextureParam = _paletteShader.Parameters["PaletteTexture"];
        _paletteIndexParam = _paletteShader.Parameters["PaletteIndex"];
        _paletteHeightParam = _paletteShader.Parameters["PaletteHeight"];
    }

    #endregion

    #region Private Fields

    private readonly SpriteBatch _spriteBatch;
    private readonly RasterizerState _rasterizerState;
    private RenderOptions? _renderOptions;

    private readonly Effect _paletteShader;
    private readonly EffectParameter _paletteTextureParam;
    private readonly EffectParameter _paletteIndexParam;
    private readonly EffectParameter _paletteHeightParam;

    #endregion

    #region Standard

    public void BeginRender(RenderOptions options)
    {
        // If we have new render options then we need to begin a new batch
        if (_renderOptions != options)
        {
            // End previous batch
            if (_renderOptions != null)
                _spriteBatch.End();

            // Set the new render options
            _renderOptions = options;

            // Set the screen area to draw to
            _spriteBatch.GraphicsDevice.Viewport = options.RenderContext.Viewport;

            // Get the shader
            Effect shader = options.Shader;
            
            // If we have a palette texture specified then we use the palette shader and pass in the params
            if (shader == null && options.PaletteTexture != null)
            {
                shader = _paletteShader;
                _paletteTextureParam.SetValue(options.PaletteTexture.Texture);
                _paletteIndexParam.SetValue(options.PaletteTexture.PaletteIndex);
                _paletteHeightParam.SetValue(options.PaletteTexture.Texture.Height);
            }

            // Begin a new batch
            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                effect: shader,
                blendState: options.Alpha ? new BlendState
                {
                    ColorSourceBlend = Blend.SourceAlpha,
                    ColorDestinationBlend = Blend.InverseSourceAlpha,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.InverseSourceAlpha,
                } : null,
                transformMatrix: Matrix.CreateScale(options.RenderContext.Scale),
                rasterizerState: _rasterizerState);
        }
    }

    public void EndRender()
    {
        // Ignore if we never began a render batch
        if (_renderOptions == null) 
            return;
        
        _spriteBatch.End();
        _renderOptions = null;
    }

    #endregion

    #region Draw

    // NOTE: Previously each Draw call started by checking CurrentCamera.IsVisible to avoid drawing sprites off-screen. However, the
    //       engine itself handles it by "framing" objects and thus avoiding drawing anything off-screen, making this redundant.

    public void Draw(Texture2D texture, Vector2 position, Color? color = null)
    {
        _spriteBatch.Draw(texture, position, texture.Bounds, color ?? Color.White);
    }
    public void Draw(Texture2D texture, Vector2 position, Rectangle sourceRectangle, Color? color = null)
    {
        _spriteBatch.Draw(texture, position, sourceRectangle, color ?? Color.White);
    }

    public void Draw(Texture2D texture, Vector2 position, SpriteEffects effects, Color? color = null)
    {
        _spriteBatch.Draw(texture, position, texture.Bounds, color ?? Color.White, 0, Vector2.Zero, Vector2.One, effects, 0);
    }
    public void Draw(Texture2D texture, Vector2 position, Rectangle sourceRectangle, SpriteEffects effects, Color? color = null)
    {
        _spriteBatch.Draw(texture, position, sourceRectangle, color ?? Color.White, 0, Vector2.Zero, Vector2.One, effects, 0);
    }

    public void Draw(Texture2D texture, Vector2 position, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, Color? color = null)
    {
        _spriteBatch.Draw(texture, position, texture.Bounds, color ?? Color.White, rotation, origin, scale, effects, 0);
    }
    public void Draw(Texture2D texture, Vector2 position, Rectangle sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, Color? color = null)
    {
        _spriteBatch.Draw(texture, position, sourceRectangle, color ?? Color.White, rotation, origin, scale, effects, 0);
    }

    #endregion

    #region FillRectangle

    /// <summary>
    /// Draws a filled rectangle
    /// </summary>
    /// <param name="rect">The rectangle to draw</param>
    /// <param name="color">The color to draw the rectangle in</param>
    public void DrawFilledRectangle(Rectangle rect, Color color)
    {
        // Simply use the function already there
        _spriteBatch.Draw(Gfx.Pixel, rect, color);
    }

    /// <summary>
    /// Draws a filled rectangle
    /// </summary>
    /// <param name="rect">The rectangle to draw</param>
    /// <param name="color">The color to draw the rectangle in</param>
    /// <param name="angle">The angle in radians to draw the rectangle at</param>
    public void DrawFilledRectangle(Rectangle rect, Color color, float angle)
    {
        _spriteBatch.Draw(Gfx.Pixel, rect, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }

    /// <summary>
    /// Draws a filled rectangle
    /// </summary>
    /// <param name="location">Where to draw</param>
    /// <param name="size">The size of the rectangle</param>
    /// <param name="color">The color to draw the rectangle in</param>
    public void DrawFilledRectangle(Vector2 location, Vector2 size, Color color)
    {
        DrawFilledRectangle(location, size, color, 0.0f);
    }

    /// <summary>
    /// Draws a filled rectangle
    /// </summary>
    /// <param name="location">Where to draw</param>
    /// <param name="size">The size of the rectangle</param>
    /// <param name="angle">The angle in radians to draw the rectangle at</param>
    /// <param name="color">The color to draw the rectangle in</param>
    public void DrawFilledRectangle(Vector2 location, Vector2 size, Color color, float angle)
    {
        // stretch the pixel between the two vectors
        _spriteBatch.Draw(Gfx.Pixel,
            location,
            null,
            color,
            angle,
            Vector2.Zero,
            size,
            SpriteEffects.None,
            0);
    }

    /// <summary>
    /// Draws a filled rectangle
    /// </summary>
    /// <param name="x">The X coordinate of the left side</param>
    /// <param name="y">The Y coordinate of the upper side</param>
    /// <param name="w">Width</param>
    /// <param name="h">Height</param>
    /// <param name="color">The color to draw the rectangle in</param>
    public void DrawFilledRectangle(float x, float y, float w, float h, Color color)
    {
        DrawFilledRectangle(new Vector2(x, y), new Vector2(w, h), color, 0.0f);
    }

    /// <summary>
    /// Draws a filled rectangle
    /// </summary>
    /// <param name="x">The X coordinate of the left side</param>
    /// <param name="y">The Y coordinate of the upper side</param>
    /// <param name="w">Width</param>
    /// <param name="h">Height</param>
    /// <param name="color">The color to draw the rectangle in</param>
    /// <param name="angle">The angle of the rectangle in radians</param>
    public void DrawFilledRectangle(float x, float y, float w, float h, Color color, float angle)
    {
        DrawFilledRectangle(new Vector2(x, y), new Vector2(w, h), color, angle);
    }

    #endregion

    #region DrawRectangle

    /// <summary>
    /// Draws a rectangle with the thickness provided
    /// </summary>
    /// <param name="rect">The rectangle to draw</param>
    /// <param name="color">The color to draw the rectangle in</param>
    public void DrawRectangle(Rectangle rect, Color color)
    {
        DrawRectangle(rect, color, 1.0f);
    }

    /// <summary>
    /// Draws a rectangle with the thickness provided
    /// </summary>
    /// <param name="rect">The rectangle to draw</param>
    /// <param name="color">The color to draw the rectangle in</param>
    /// <param name="thickness">The thickness of the lines</param>
    public void DrawRectangle(Rectangle rect, Color color, float thickness)
    {
        DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.Right, rect.Y), color, thickness); // top
        DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Bottom), color, thickness); // left
        DrawLine(new Vector2(rect.X, rect.Bottom), new Vector2(rect.Right, rect.Bottom), color, thickness); // bottom
        DrawLine(new Vector2(rect.Right, rect.Y), new Vector2(rect.Right, rect.Bottom), color, thickness); // right
    }

    /// <summary>
    /// Draws a rectangle with the thickness provided
    /// </summary>
    /// <param name="location">Where to draw</param>
    /// <param name="size">The size of the rectangle</param>
    /// <param name="color">The color to draw the rectangle in</param>
    public void DrawRectangle(Vector2 location, Vector2 size, Color color)
    {
        DrawRectangle(new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, 1.0f);
    }

    /// <summary>
    /// Draws a rectangle with the thickness provided
    /// </summary>
    /// <param name="location">Where to draw</param>
    /// <param name="size">The size of the rectangle</param>
    /// <param name="color">The color to draw the rectangle in</param>
    /// <param name="thickness">The thickness of the line</param>
    public void DrawRectangle(Vector2 location, Vector2 size, Color color, float thickness)
    {
        DrawRectangle(new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);
    }

    #endregion

    #region DrawLine

    /// <summary>
    /// Draws a line from point1 to point2 with an offset
    /// </summary>
    /// <param name="x1">The X coordinate of the first point</param>
    /// <param name="y1">The Y coordinate of the first point</param>
    /// <param name="x2">The X coordinate of the second point</param>
    /// <param name="y2">The Y coordinate of the second point</param>
    /// <param name="color">The color to use</param>
    public void DrawLine(float x1, float y1, float x2, float y2, Color color)
    {
        DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, 1.0f);
    }

    /// <summary>
    /// Draws a line from point1 to point2 with an offset
    /// </summary>
    /// <param name="x1">The X coordinate of the first point</param>
    /// <param name="y1">The Y coordinate of the first point</param>
    /// <param name="x2">The X coordinate of the second point</param>
    /// <param name="y2">The Y coordinate of the second point</param>
    /// <param name="color">The color to use</param>
    /// <param name="thickness">The thickness of the line</param>
    public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness)
    {
        DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
    }

    /// <summary>
    /// Draws a line from point1 to point2 with an offset
    /// </summary>
    /// <param name="point1">The first point</param>
    /// <param name="point2">The second point</param>
    /// <param name="color">The color to use</param>
    public void DrawLine(Vector2 point1, Vector2 point2, Color color)
    {
        DrawLine(point1, point2, color, 1.0f);
    }

    /// <summary>
    /// Draws a line from point1 to point2 with an offset
    /// </summary>
    /// <param name="point1">The first point</param>
    /// <param name="point2">The second point</param>
    /// <param name="color">The color to use</param>
    /// <param name="thickness">The thickness of the line</param>
    public void DrawLine(Vector2 point1, Vector2 point2, Color color, float thickness)
    {
        // calculate the distance between the two vectors
        float distance = Vector2.Distance(point1, point2);

        // calculate the angle between the two vectors
        float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

        DrawLine(point1, distance, angle, color, thickness);
    }

    /// <summary>
    /// Draws a line from point1 to point2 with an offset
    /// </summary>
    /// <param name="point">The starting point</param>
    /// <param name="length">The length of the line</param>
    /// <param name="angle">The angle of this line from the starting point in radians</param>
    /// <param name="color">The color to use</param>
    public void DrawLine(Vector2 point, float length, float angle, Color color)
    {
        DrawLine(point, length, angle, color, 1.0f);
    }

    /// <summary>
    /// Draws a line from point1 to point2 with an offset
    /// </summary>
    /// <param name="point">The starting point</param>
    /// <param name="length">The length of the line</param>
    /// <param name="angle">The angle of this line from the starting point</param>
    /// <param name="color">The color to use</param>
    /// <param name="thickness">The thickness of the line</param>
    public void DrawLine(Vector2 point, float length, float angle, Color color, float thickness)
    {
        // stretch the pixel between the two vectors
        _spriteBatch.Draw(Gfx.Pixel,
            point,
            null,
            color,
            angle,
            Vector2.Zero, 
            new Vector2(length, thickness),
            SpriteEffects.None,
            0);
    }

    #endregion

    #region DrawPixel

    public void DrawPixel(float x, float y, Color color)
    {
        DrawPixel(new Vector2(x, y), color);
    }

    public void DrawPixel(Vector2 position, Color color)
    {
        _spriteBatch.Draw(Gfx.Pixel, position, color);
    }

    #endregion
}