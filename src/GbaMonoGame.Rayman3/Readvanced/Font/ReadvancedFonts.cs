using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class ReadvancedFonts
{
    // TODO: Define remaining glyphs
    private static readonly Dictionary<char, Font.Glyph> _menuGlyphs = new()
    {
        [' '] = new(Rectangle.Empty, layoutOffset: 5),

        ['!'] = new(new Rectangle(16, 8, 5, 13)),

        ['1'] = new(new Rectangle(6, 28, 5, 12)),
        ['2'] = new(new Rectangle(16, 28, 8, 12)),
        ['3'] = new(new Rectangle(28, 28, 8, 12)),
        ['4'] = new(new Rectangle(41, 28, 10, 12)),

        ['A'] = new(new Rectangle(6, 47, 11, 12)),
        ['B'] = new(new Rectangle(21, 47, 9, 12)),
        ['C'] = new(new Rectangle(35, 47, 9, 12)),
        ['D'] = new(new Rectangle(48, 47, 9, 12)),
        ['E'] = new(new Rectangle(62, 47, 7, 12)),
        ['F'] = new(new Rectangle(74, 47, 7, 12)),
        ['G'] = new(new Rectangle(86, 47, 9, 13), renderOffset: new Vector2(0, 1)),
        ['H'] = new(new Rectangle(100, 47, 10, 12)),
        ['I'] = new(new Rectangle(114, 48, 5, 11)),
        ['J'] = new(new Rectangle(123, 47, 6, 15), renderOffset: new Vector2(0, 3)),
        ['K'] = new(new Rectangle(134, 47, 9, 13), renderOffset: new Vector2(0, 1)),
        ['L'] = new(new Rectangle(147, 47, 8, 12)),
        ['M'] = new(new Rectangle(160, 48, 11, 11)),
        ['N'] = new(new Rectangle(176, 47, 10, 12)),
        ['O'] = new(new Rectangle(190, 47, 11, 12)),

        ['P'] = new(new Rectangle(6, 67, 9, 12)),
        ['Q'] = new(new Rectangle(19, 67, 11, 14), renderOffset: new Vector2(0, 2)),
        ['R'] = new(new Rectangle(34, 67, 9, 12)),
        ['S'] = new(new Rectangle(48, 67, 7, 12)),
        ['T'] = new(new Rectangle(60, 67, 9, 12))
        {
            GlyphSpecificLayoutOffsets = new Dictionary<char, float>()
            {
                ['L'] = -2,
            }
        },
        ['U'] = new(new Rectangle(74, 67, 9, 12)),
        ['V'] = new(new Rectangle(88, 67, 10, 12)),
        ['W'] = new(new Rectangle(103, 67, 13, 12)),
        ['X'] = new(new Rectangle(120, 67, 12, 12)),
        ['Y'] = new(new Rectangle(136, 67, 10, 12))
        {
            GlyphSpecificLayoutOffsets = new Dictionary<char, float>()
            {
                ['A'] = -3,
            }
        },
        ['Z'] = new(new Rectangle(150, 67, 9, 12)),
    };

    public static Font MenuYellow { get; private set; }
    public static Font MenuWhite { get; private set; }

    public static void Load(ContentManager contentManager)
    {
        MenuYellow = new Font(contentManager.Load<Texture2D>("Font_MenuYellow"), _menuGlyphs, 15);
        MenuWhite = new Font(contentManager.Load<Texture2D>("Font_MenuWhite"), _menuGlyphs, 15);
    }
}