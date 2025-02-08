using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.TgxEngine;

public class GbaVram
{
    private GbaVram(byte[] tileSet, Palette[] palettes, int selectedPaletteIndex)
    {
        TileSet = tileSet;
        Palettes = palettes;
        SelectedPaletteIndex = selectedPaletteIndex;
    }

    private const int TileSize4bpp = 0x20;
    private const int TileSize8bpp = 0x40;

    public byte[] TileSet { get; }

    public Palette[] Palettes { get; }
    public int SelectedPaletteIndex { get; }
    public Palette SelectedPalette => Palettes[SelectedPaletteIndex];

    public static GbaVram AllocateStatic(TileKit tileKit, TileMappingTable tileMappingTable, int vramLength8bpp, bool isAffine, int defaultPalette)
    {
        int base8bpp = 512;

        // If affine then the game starts at 513 instead of 512 (it always sets 512 to a blank tile)
        if (isAffine)
        {
            base8bpp++;
            vramLength8bpp--;
        }

        // 8-bit tiles start at its base and go on for the specified length before wrapping around
        // to 0. 4-bit tiles start at 0. Any data after this is dynamic.
        byte[] tileSet = new byte[(base8bpp + vramLength8bpp) * TileSize8bpp];

        // Allocate 4-bit tiles
        int offset = 2; // First 0x40 bytes are always empty. For 8-bit that's one tile, but for 4-bit it's 2 tiles.
        for (int i = 0; i < tileMappingTable.Table4bpp.Length; i++)
        {
            int value = tileMappingTable.Table4bpp[i] - 1;
            Array.Copy(tileKit.Tiles4bpp, value * TileSize4bpp, tileSet, (i + offset) * TileSize4bpp, TileSize4bpp);
        }

        // Allocate 8-bit tiles
        for (int i = 0; i < tileMappingTable.Table8bpp.Length; i++)
        {
            offset = i < vramLength8bpp ? base8bpp : -vramLength8bpp + 1;
            int value = tileMappingTable.Table8bpp[i] - 1;
            Array.Copy(tileKit.Tiles8bpp, value * TileSize8bpp, tileSet, (i + offset) * TileSize8bpp, TileSize8bpp);
        }

        // Load palettes
        Palette[] palettes = new Palette[tileKit.Palettes.Length];

        for (int i = 0; i < palettes.Length; i++)
        {
            PaletteResource paletteResource = tileKit.Palettes[i].Palette;
            palettes[i] = Engine.PaletteCache.GetOrCreateObject(
                pointer: paletteResource.Offset,
                id: 0,
                data: paletteResource,
                createObjFunc: p => new Palette(p));
        }

        return new GbaVram(tileSet, palettes, defaultPalette);
    }
}