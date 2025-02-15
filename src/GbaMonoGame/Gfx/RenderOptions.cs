using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

// Have this be a record so we get automatic equality comparisons implemented
public record RenderOptions
{
    public RenderContext RenderContext { get; set; }
    public Effect Shader { get; set; }
    public BlendMode BlendMode { get; set; }
    public PaletteTexture PaletteTexture { get; set; }
    public Matrix? WorldViewProj { get; set; }
}