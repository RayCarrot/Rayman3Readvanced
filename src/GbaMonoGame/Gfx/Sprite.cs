using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class Sprite
{
    public Texture2D Texture { get; set; }
    public Rectangle TextureRectangle { get; set; }
    public PaletteTexture PaletteTexture { get; set; }
    public Vector2 Position { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public int Priority { get; set; }

    public bool Center { get; set; }
    public AffineMatrix? AffineMatrix { get; set; }
    public Color Color { get; set; } = Color.White;
    public bool OverrideGfxColor { get; set; } // Needed for the curtains in the worldmap which are not effected by the palette fading

    // TODO: There are multiple issues with how alpha is implemented here compared to on GBA. Most noticeably sprites should not effect each other. Very noticeable when setting it on Rayman and looking up - Rayman gets 4 eyes then! Although this might be how it is on N-Gage?
    public float? Alpha { get; set; }

    public RenderContext RenderContext { get; set; } = Engine.GameRenderContext;

    public void Draw(GfxRenderer renderer, Color color)
    {
        renderer.BeginRender(new RenderOptions(Alpha != null, PaletteTexture, RenderContext));

        if (OverrideGfxColor)
            color = Color;
        else
            color = new Color(Color.ToVector4() * color.ToVector4());
        
        if (Alpha != null)
            color = new Color(color, Alpha.Value);

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