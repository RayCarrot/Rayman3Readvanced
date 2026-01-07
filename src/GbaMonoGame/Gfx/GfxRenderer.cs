using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class GfxRenderer
{
    #region Constructor

    public GfxRenderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _spriteRasterizerState = RasterizerState.CullNone;

        _defaultShader = Engine.FixContentManager.Load<Effect>(Assets.DefaultShader);

        _paletteShader = Engine.FixContentManager.Load<Effect>(Assets.PaletteShader);
        _paletteShaderPaletteTextureParam = _paletteShader.Parameters["PaletteTexture"];
        _paletteShaderPaletteIndexParam = _paletteShader.Parameters["PaletteIndex"];
        _paletteShaderPaletteHeightParam = _paletteShader.Parameters["PaletteHeight"];

        _paletteVertexShader = Engine.FixContentManager.Load<Effect>(Assets.PaletteVertexShader);
        _paletteVertexShaderPaletteTextureParam = _paletteVertexShader.Parameters["PaletteTexture"];
        _paletteVertexShaderPaletteIndexParam = _paletteVertexShader.Parameters["PaletteIndex"];
        _paletteVertexShaderPaletteHeightParam = _paletteVertexShader.Parameters["PaletteHeight"];
        _paletteVertexShaderWorldViewProjParam = _paletteVertexShader.Parameters["WorldViewProj"];

        _vertexShader = Engine.FixContentManager.Load<Effect>(Assets.VertexShader);
        _vertexShaderWorldViewProjParam = _vertexShader.Parameters["WorldViewProj"];

        _blendStates = new Dictionary<BlendMode, BlendState>()
        {
            // TODO: Should probably be opaque?
            [BlendMode.None] = BlendState.AlphaBlend,

            // Custom alpha blend since we don't use pre-multiplied colors
            [BlendMode.AlphaBlend] = new()
            {
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
            },

            // Overwrites pixels even if transparent
            [BlendMode.AlphaBlendOverwrite] = new()
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero
            },

            // Default additive blending
            [BlendMode.Additive] = BlendState.Additive
        }.ToFrozenDictionary();
    }

    #endregion

    #region Private Fields

    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly RasterizerState _spriteRasterizerState;
    private RenderOptions _spriteBatchRenderOptions;

    private readonly Effect _defaultShader;

    private readonly Effect _paletteShader;
    private readonly EffectParameter _paletteShaderPaletteTextureParam;
    private readonly EffectParameter _paletteShaderPaletteIndexParam;
    private readonly EffectParameter _paletteShaderPaletteHeightParam;

    private readonly Effect _paletteVertexShader;
    private readonly EffectParameter _paletteVertexShaderPaletteTextureParam;
    private readonly EffectParameter _paletteVertexShaderPaletteIndexParam;
    private readonly EffectParameter _paletteVertexShaderPaletteHeightParam;
    private readonly EffectParameter _paletteVertexShaderWorldViewProjParam;

    private readonly Effect _vertexShader;
    private readonly EffectParameter _vertexShaderWorldViewProjParam;

    private readonly FrozenDictionary<BlendMode, BlendState> _blendStates;

    #endregion

    #region Private Helpers

    private BlendState GetBlendState(BlendMode blendMode) => _blendStates[blendMode];

    #endregion

    #region Standard

    public void BeginSpriteRender(RenderOptions options)
    {
        // If we have new render options then we need to begin a new batch
        if (_spriteBatchRenderOptions != options)
        {
            // End previous batch
            if (_spriteBatchRenderOptions != null)
                _spriteBatch.End();

            // Set the new render options
            _spriteBatchRenderOptions = options;

            // Set the screen area to draw to
            Viewport viewport = options.RenderContext.Viewport;
            _graphicsDevice.Viewport = viewport;

            // Get the shader
            Effect shader;

            // If a shader is specified then we always use that
            if (options.Shader != null)
            {
                shader = options.Shader;
            }
            // If we have a palette texture then we need to use a palette shader
            else if (options.PaletteTexture != null)
            {
                // If we have a WorldViewProj matrix then we render it in 3D using a vertex shader
                if (options.WorldViewProj != null)
                {
                    shader = _paletteVertexShader;
                    _paletteVertexShaderPaletteTextureParam.SetValue(options.PaletteTexture.Texture);
                    _paletteVertexShaderPaletteIndexParam.SetValue(options.PaletteTexture.PaletteIndex);
                    _paletteVertexShaderPaletteHeightParam.SetValue(options.PaletteTexture.Texture.Height);
                    _paletteVertexShaderWorldViewProjParam.SetValue(options.WorldViewProj.Value);
                }
                // If there is no WorldViewProj matrix then we render it in 2D using the vertex shader from the SpriteBatch
                else
                {
                    shader = _paletteShader;
                    _paletteShaderPaletteTextureParam.SetValue(options.PaletteTexture.Texture);
                    _paletteShaderPaletteIndexParam.SetValue(options.PaletteTexture.PaletteIndex);
                    _paletteShaderPaletteHeightParam.SetValue(options.PaletteTexture.Texture.Height);
                }
            }
            // If we have a WorldViewProj matrix then we render it in 3D using a vertex shader
            else if (options.WorldViewProj != null)
            {
                shader = _vertexShader;
                _vertexShaderWorldViewProjParam.SetValue(options.WorldViewProj.Value);
            }
            // Render without a palette texture or custom shader
            else
            {
                shader = _defaultShader;
            }

            // Create a view matrix using the render scale. In the SpriteBatch this is multiplied by a projection matrix
            // which is calculated as: Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, -1).
            // Note that this gets ignored if we're using a vertex shader!
            Matrix view = Matrix.CreateScale(options.RenderContext.Scale);

#if DESKTOPGL
            // NOTE: We need this here for OpenGL since otherwise there's a very odd bug that happens with the palette
            //       shaders where the palette texture (index 1) doesn't get correctly invalidated, and thus it ends up
            //       re-using the same palette for more things than it should (but only the first frame the new texture
            //       is rendered). This fixes it.
            _graphicsDevice.Textures[1] = null;
#endif

            // Begin a new batch
            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                effect: shader,
                blendState: GetBlendState(options.BlendMode),
                depthStencilState: options.UseDepthStencil ? DepthStencilState.Default : DepthStencilState.None,
                transformMatrix: view,
                rasterizerState: _spriteRasterizerState);
        }
    }

    public void BeginMeshRender(RenderOptions options, RasterizerState rasterizerState)
    {
        // End previous batch
        if (_spriteBatchRenderOptions != null)
            _spriteBatch.End();

        _spriteBatchRenderOptions = null;

        // Set the screen area to draw to
        Viewport viewport = options.RenderContext.Viewport;
        _graphicsDevice.Viewport = viewport;

        // Set the graphics device values
        _graphicsDevice.BlendState = GetBlendState(options.BlendMode);
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = rasterizerState;
    }

    public void EndRender()
    {
        // End the current sprite batch if there is one
        if (_spriteBatchRenderOptions != null)
        {
            _spriteBatch.End();
            _spriteBatchRenderOptions = null;
        }
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