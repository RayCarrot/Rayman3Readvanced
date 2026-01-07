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

    public void Draw(GfxRenderer renderer, Color color)
    {
        renderer.BeginSpriteRender(RenderOptions);

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

        Vector2 scale = AffineMatrix?.Scale ?? Vector2.One;

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