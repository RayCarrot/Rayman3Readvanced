using System;
using System.Collections.Generic;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.TgxEngine;

public class GbaVram
{
    private GbaVram(byte[] tileSet, int[] gameToVramMappingTable4Bpp, int[] gameToVramMappingTable8Bpp, Palette[] palettes, int selectedPaletteIndex)
    {
        TileSet = tileSet;
        GameToVramMappingTable4bpp = gameToVramMappingTable4Bpp;
        GameToVramMappingTable8bpp = gameToVramMappingTable8Bpp;
        Palettes = palettes;
        SelectedPaletteIndex = selectedPaletteIndex;
    }

    private const int TileSize4bpp = 0x20;
    private const int TileSize8bpp = 0x40;

    public byte[] TileSet { get; }
    public int[] GameToVramMappingTable4bpp { get; }
    public int[] GameToVramMappingTable8bpp { get; }

    public Palette[] Palettes { get; }
    public int SelectedPaletteIndex { get; }
    public Palette SelectedPalette => Palettes[SelectedPaletteIndex];

    public IScreenRenderer CreateTileMapRenderer(
        RenderOptions renderOptions, 
        TileKit tileKit, 
        AnimatedTilekitManager animatedTilekitManager, 
        Pointer layerCachePointer,
        int width,
        int height,
        MapTile[] tileMap,
        int baseTileIndex,
        bool is8Bit,
        bool isDynamic)
    {
        // Create and set the palette texture
        renderOptions.PaletteTexture = new PaletteTexture(
            Texture: Engine.TextureCache.GetOrCreateObject(
                pointer: SelectedPalette.CachePointer,
                id: 0,
                data: SelectedPalette,
                createObjFunc: static p => new PaletteTexture2D(p)),
            PaletteIndex: 0);

        // If the tile map is dynamic then we create a tile map renderer for it and render it tile by tile. This is
        // usually used for the level maps. If it's static then we create a texture for the entire map. This is usually
        // used for backgrounds.
        if (isDynamic)
        {
            // If it's dynamic then we read the tiles directly from the tilekit rather than the allocated ones in VRAM
            byte[] tileSet = is8Bit ? tileKit.Tiles8bpp : tileKit.Tiles4bpp;

            TileMapScreenRenderer renderer = new(
                // Use the tilekit as the cache pointer since multiple layers can share the same tilekit and we're
                // caching per tile, but make the pointer differ depending on if it's 4-bit or 8-bit.
                cachePointer: tileKit.Offset + (is8Bit ? 1 : 0),
                width: width,
                height: height,
                tileMap: tileMap,
                tileSet: tileSet,
                is8Bit: is8Bit);

            // Add the renderer to the animated tilekit manager if one exists
            animatedTilekitManager?.AddTileMapRenderer(renderer, true);

            return renderer;
        }
        else
        {
            // Get the animations used by this layer
            IReadOnlyList<TileKitAnimation> animations = animatedTilekitManager?.GetUsedAnimations(this, tileKit, tileMap, baseTileIndex, is8Bit, false);

            // If it's animated then we create a separate texture for each animation frame
            if (animations?.Count > 0)
            {
                // Should hopefully never happen in the game. Otherwise we need to add support for this and find the lowest common denominator.
                if (animations.Count > 1)
                    throw new InvalidOperationException("Multiple animations for the same static layer is currently not supported");

                TileKitAnimation anim = animations[0];

                Texture2D[] layerTextures = new Texture2D[anim.TileKit.FramesCount];

                int tileSize = is8Bit ? TileSize8bpp : TileSize4bpp;

                byte[] sourceTileSet = is8Bit ? tileKit.Tiles8bpp : tileKit.Tiles4bpp;
                int[] mappingTable = is8Bit ? GameToVramMappingTable8bpp : GameToVramMappingTable4bpp;

                byte[] tileSet = new byte[TileSet.Length];
                Array.Copy(TileSet, tileSet, TileSet.Length);

                for (int frame = 0; frame < layerTextures.Length; frame++)
                {
                    // Update the animated tiles
                    if (frame != 0)
                    {
                        foreach (ushort tile in anim.TileKit.Tiles)
                        {
                            int srcTileIndex = tile - 1 + frame * anim.TileKit.TilesStep;
                            int dstTileIndex = mappingTable[tile - 1];

                            Array.Copy(sourceTileSet, srcTileIndex * tileSize, tileSet, dstTileIndex * tileSize, tileSize);
                        }
                    }

                    layerTextures[frame] = Engine.TextureCache.GetOrCreateObject(
                        pointer: layerCachePointer,
                        id: frame,
                        data: (TileSet: tileSet, Width: width, Height: height, TileMap: tileMap, BaseTileIndex: baseTileIndex, Is8Bit: is8Bit),
                        createObjFunc: static data => new IndexedTiledTexture2D(data.Width, data.Height, data.TileSet, data.TileMap, data.BaseTileIndex, data.Is8Bit));
                }

                MultipleTexturesScreenRenderer renderer = new(layerTextures);
                animatedTilekitManager.AddTextureRenderer(renderer, anim.TileKit.Speed, anim.TileKit.FramesCount);
                return renderer;
            }
            // If it's not animated then we just add a single texture
            else
            {
                Texture2D layerTexture = Engine.TextureCache.GetOrCreateObject(
                    pointer: layerCachePointer,
                    id: 0,
                    data: (TileSet: TileSet, Width: width, Height: height, TileMap: tileMap, BaseTileIndex: baseTileIndex, Is8Bit: is8Bit),
                    createObjFunc: static data => new IndexedTiledTexture2D(data.Width, data.Height, data.TileSet, data.TileMap, data.BaseTileIndex, data.Is8Bit));

                return new TextureScreenRenderer(layerTexture);
            }
        }
    }

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

        // Create tables for the inverse mapping (game -> vram)
        int[] mappingTable4bpp = new int[tileKit.Tiles4bpp.Length / TileSize4bpp];
        int[] mappingTable8bpp = new int[tileKit.Tiles8bpp.Length / TileSize8bpp];

        // Allocate 4-bit tiles
        int offset = 2; // First 0x40 bytes are always empty. For 8-bit that's one tile, but for 4-bit it's 2 tiles.
        for (int i = 0; i < tileMappingTable.Table4bpp.Length; i++)
        {
            int value = tileMappingTable.Table4bpp[i] - 1;
            Array.Copy(tileKit.Tiles4bpp, value * TileSize4bpp, tileSet, (i + offset) * TileSize4bpp, TileSize4bpp);
            mappingTable4bpp[value] = i + offset;
        }

        // Allocate 8-bit tiles
        for (int i = 0; i < tileMappingTable.Table8bpp.Length; i++)
        {
            offset = i < vramLength8bpp ? base8bpp : -vramLength8bpp + 1;
            int value = tileMappingTable.Table8bpp[i] - 1;
            Array.Copy(tileKit.Tiles8bpp, value * TileSize8bpp, tileSet, (i + offset) * TileSize8bpp, TileSize8bpp);
            mappingTable8bpp[value] = i + offset;
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

        return new GbaVram(tileSet, mappingTable4bpp, mappingTable8bpp, palettes, defaultPalette);
    }
}