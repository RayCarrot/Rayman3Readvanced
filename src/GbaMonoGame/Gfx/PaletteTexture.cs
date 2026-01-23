using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

// Have this be a record so we get automatic equality comparisons implemented
public record struct PaletteTexture(Texture2D Texture, int PaletteIndex);