using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

// NOTE: This is a very large struct (120 bytes), but in this case it makes sense
//       due to its usage. A class would cause too many allocations each frame.
// Have this be a record so we get automatic equality comparisons implemented
public readonly record struct RenderOptions
{
    public RenderContext RenderContext { get; init; }
    public Effect Shader { get; init; }
    public BlendMode BlendMode { get; init; }
    public PaletteTexture? PaletteTexture { get; init; }
    public Matrix? WorldViewProj { get; init; }
    public bool UseDepthStencil { get; init; }
}