using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

// Have this be a record so we get automatic equality comparisons implemented
public record struct RenderOptions
{
    public RenderContext RenderContext { get; init; }
    public Effect Shader { get; init; }
    public BlendMode BlendMode { get; init; }
    public PaletteTexture PaletteTexture { get; init; }
    public Matrix? WorldViewProj { get; init; }
    public bool UseDepthStencil { get; init; }
}