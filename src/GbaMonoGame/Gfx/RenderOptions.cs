namespace GbaMonoGame;

// Have this be a record so we get automatic equality comparisons implemented
public record struct RenderOptions(bool Alpha, PaletteTexture PaletteTexture, RenderContext RenderContext);