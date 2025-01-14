using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

// Have this be a record so we get automatic equality comparisons implemented
public record struct RenderOptions
{
    public RenderOptions(RenderContext renderContext)
    {
        RenderContext = renderContext;
    }

    public RenderContext RenderContext { get; }
    public  Effect Shader { get; init; }
    public bool Alpha { get; init; }
    public PaletteTexture PaletteTexture { get; init; }
};