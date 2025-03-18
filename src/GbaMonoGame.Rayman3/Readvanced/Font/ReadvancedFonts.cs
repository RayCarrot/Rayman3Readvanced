using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class ReadvancedFonts
{
    // TODO: Define remaining glyphs
    private static readonly Dictionary<char, Font.Glyph> _menuGlyphs = new()
    {
        [' '] = new(Rectangle.Empty, layoutOffset: 5),
        ['!'] = new(new Rectangle(16, 8, 5, 13)),
        ['"'] = new(new Rectangle(25, 8, 6, 5), renderOffset: new Vector2(0, -8)),
        ['#'] = new(new Rectangle(35, 10, 11, 10), renderOffset: new Vector2(0, -1)),
        ['$'] = new(new Rectangle(50, 7, 8, 15), renderOffset: new Vector2(0, 1)),
        ['%'] = new(new Rectangle(62, 8, 16, 14), renderOffset: new Vector2(0, 1)),
        ['&'] = new(new Rectangle(82, 8, 10, 13)),
        ['\''] = new(new Rectangle(96, 8, 4, 5), renderOffset: new Vector2(0, -8)),
        ['('] = new(new Rectangle(105, 8, 5, 15), renderOffset: new Vector2(0, 2)),
        [')'] = new(new Rectangle(115, 8, 6, 15), renderOffset: new Vector2(0, 2)),
        ['*'] = new(new Rectangle(125, 8, 7, 7), renderOffset: new Vector2(0, -6)),
        ['+'] = new(new Rectangle(137, 11, 8, 9), renderOffset: new Vector2(0, -1)),
        [','] = new(new Rectangle(150, 16, 5, 7), renderOffset: new Vector2(0, 2)),
        ['-'] = new(new Rectangle(159, 13, 6, 4), renderOffset: new Vector2(0, -4)),
        ['.'] = new(new Rectangle(169, 16, 4, 4), renderOffset: new Vector2(0, -1)),
        ['/'] = new(new Rectangle(178, 8, 7, 12), renderOffset: new Vector2(0, -1)),
        ['0'] = new(new Rectangle(190, 9, 9, 12)),

        ['1'] = new(new Rectangle(6, 28, 5, 12)),
        ['2'] = new(new Rectangle(16, 28, 8, 12)),
        ['3'] = new(new Rectangle(28, 28, 8, 12)),
        ['4'] = new(new Rectangle(41, 28, 10, 12)),
        ['5'] = new(new Rectangle(55, 28, 8, 12)),
        ['6'] = new(new Rectangle(68, 28, 8, 12)),
        ['7'] = new(new Rectangle(80, 28, 8, 13), renderOffset: new Vector2(0, 1)),
        ['8'] = new(new Rectangle(93, 28, 8, 12)),
        ['9'] = new(new Rectangle(106, 28, 8, 12)),
        [':'] = new(new Rectangle(118, 30, 5, 10)),
        [';'] = new(new Rectangle(127, 30, 5, 12), renderOffset: new Vector2(0, 2)),
        ['<'] = new(new Rectangle(136, 31, 7, 8), renderOffset: new Vector2(0, -1)),
        ['='] = new(new Rectangle(147, 31, 8, 7), renderOffset: new Vector2(0, -2)),
        ['>'] = new(new Rectangle(160, 31, 6, 8), renderOffset: new Vector2(0, -1)),
        ['?'] = new(new Rectangle(171, 27, 7, 13)),
        ['@'] = new(new Rectangle(182, 28, 13, 12)),

        ['A'] = new(new Rectangle(6, 47, 11, 12))
        {
            GlyphSpecificLayoutOffsets = new Dictionary<char, float>()
            {
                ['H'] = -1,
                ['N'] = -1,
                ['T'] = -2,
                ['V'] = -1,
            }
        },
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
                ['A'] = -2,
                ['C'] = -1,
                ['I'] = -1,
                ['L'] = -2,
            }
        },
        ['U'] = new(new Rectangle(74, 67, 9, 12)),
        ['V'] = new(new Rectangle(88, 67, 10, 12))
        {
            GlyphSpecificLayoutOffsets = new Dictionary<char, float>()
            {
                ['A'] = -2,
            }
        },
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

        ['x'] = new(new Rectangle(121, 108, 10, 10), renderOffset: new Vector2(0, 1)),
    };

    public static Font MenuYellow { get; private set; }
    public static Font MenuWhite { get; private set; }

    public static void Load()
    {
        MenuYellow = new Font(Engine.FixContentManager.Load<Texture2D>("Font_MenuYellow"), _menuGlyphs, 15);
        MenuWhite = new Font(Engine.FixContentManager.Load<Texture2D>("Font_MenuWhite"), _menuGlyphs, 15);
    }
}