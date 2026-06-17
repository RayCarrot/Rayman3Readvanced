using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class Sprite
{
    public Texture2D Texture { get; set; }
    public Rectangle TextureRectangle { get; set; }
    public Vector2 Position { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public int Priority { get; set; }

    public bool Center { get; set; }
    public AffineMatrix? AffineMatrix { get; set; }
    public Color Color { get; set; } = Color.White;
    public bool OverrideGfxColor { get; set; } // Needed for the curtains in the worldmap which are not effected by the palette fading

    public AlphaCoefficient Alpha { get; set; }

    public RenderOptions RenderOptions { get; set; }
    public Box? ScissorBox { get; set; }

    public void Reset()
    {
        Texture = null;
        TextureRectangle = default;
        Position = default;
        FlipX = false;
        FlipY = false;
        Priority = 0;
        Center = false;
        AffineMatrix = null;
        Color = Color.White;
        OverrideGfxColor = false;
        Alpha = default;
        RenderOptions = default;
        ScissorBox = null;
    }

    public void Draw(GfxRenderer renderer, Color color)
    {
        renderer.BeginSpriteRender(RenderOptions, ScissorBox);

        if (OverrideGfxColor)
            color = Color;
        else
            color = Color * color;
        
        if (RenderOptions.BlendMode != BlendMode.None)
            color = new Color(color, Alpha);

        Rectangle textureRectangle = TextureRectangle;
        if (textureRectangle == Rectangle.Empty)
            textureRectangle = Texture.Bounds;

        Vector2 center = new(textureRectangle.Width / 2f, textureRectangle.Height / 2f);

        SpriteEffects effects = SpriteEffects.None;
        if ((AffineMatrix?.FlipX ?? false) ^ FlipX)
            effects |= SpriteEffects.FlipHorizontally;
        if ((AffineMatrix?.FlipY ?? false) ^ FlipY)
            effects |= SpriteEffects.FlipVertically;

        float rotation;
        if (AffineMatrix != null)
        {
            rotation = AffineMatrix.Value.Rotation;
            if (FlipX ^ FlipY)
                rotation = -rotation;
        }
        else
        {
            rotation = 0;
        }

        Vector2 scale;
        if (AffineMatrix != null)
        {
            // We round the scale to the nearest pixels in order to reduce sub-pixel scaling. This
            // is to avoid issues in high resolution where there becomes a gap in-between two sprites
            // in an animation due to scale values in the animations not always exactly aligning.
            // This was especially noticeable for the cage icon animation in the overworlds and the
            // shadow of the Scaleman when landing.
            Vector2 scaleAdjustSize;
            if (Center)
                scaleAdjustSize = textureRectangle.Size.ToVector2() / 2f;
            else
                scaleAdjustSize = textureRectangle.Size.ToVector2();

            scale = Vector2.Round(scaleAdjustSize * AffineMatrix.Value.Scale) / scaleAdjustSize;
        }
        else
        {
            scale = Vector2.One;
        }

        renderer.Draw(
            texture: Texture, 
            position: Center ? Position + center : Position, 
            sourceRectangle: textureRectangle, 
            rotation: rotation, 
            origin: Center ? center : Vector2.Zero, 
            scale: scale, 
            effects: effects, 
            color: color);
    }
}