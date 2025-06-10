using System;
using System.Collections.Generic;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GbaMonoGame.TgxEngine;

public class GfxTileKitManager
{
    private const int TileSize4bpp = 0x20;
    private const int TileSize8bpp = 0x40;

    // Limit the texture size for a map to 2048x2048 to prevent it being too big on some platforms
    private const int MaxTextureWidth = 2048 / Tile.Size;
    private const int MaxTextureHeight = 2048 / Tile.Size;

    public TileKit TileKit { get; set; }

    public byte[] StaticTileSet { get; set; }
    public byte[] DynamicTileSet8bpp { get; set; }
    public byte[] DynamicTileSet4bpp { get; set; }
    public int[] GameToVramMappingTable4bpp { get; set; }
    public int[] GameToVramMappingTable8bpp { get; set; }

    public Palette[] Palettes { get; set; }
    public int SelectedPaletteIndex { get; set; }
    public Palette SelectedPalette => Palettes[SelectedPaletteIndex];

    private static IScreenRenderer CreateTextureScreenRenderer(
        Pointer layerCachePointer,
        int cacheId,
        int maxCacheId,
        byte[] tileSet,
        int width,
        int height,
        MapTile[] tileMap,
        int baseTileIndex,
        bool is8Bit)
    {
        // If the map is too big then we need to split it into sections
        if (width > MaxTextureWidth || height > MaxTextureHeight)
        {
            List<MultiScreenRenderer.Section> sections = new();

            for (int y = 0; y < height; y += MaxTextureHeight)
            {
                for (int x = 0; x < width; x += MaxTextureWidth)
                {
                    int sectionWidth = Math.Min(MaxTextureWidth, width - x);
                    int sectionHeight = Math.Min(MaxTextureHeight, height - y);

                    Texture2D layerSectionTexture = Engine.TextureCache.GetOrCreateObject(
                        pointer: layerCachePointer,
                        id: (y * width + x) * maxCacheId + cacheId,
                        data: (
                            TileSet: tileSet,
                            Width: width,
                            Height: height,
                            X: x,
                            Y: y,
                            SectionWidth: sectionWidth,
                            SectionHeight: sectionHeight,
                            TileMap: tileMap,
                            BaseTileIndex: baseTileIndex,
                            Is8Bit: is8Bit),
                        createObjFunc: static data => new IndexedTiledTexture2D(
                            fullWidth: data.Width,
                            fullHeight: data.Height,
                            startX: data.X,
                            startY: data.Y,
                            width: data.SectionWidth,
                            height: data.SectionHeight,
                            tileSet: data.TileSet,
                            tileMap: data.TileMap,
                            baseTileIndex: data.BaseTileIndex,
                            is8Bit: data.Is8Bit));

                    sections.Add(new MultiScreenRenderer.Section(
                        screenRenderer: new TextureScreenRenderer(layerSectionTexture), 
                        position: new Vector2(x * Tile.Size, y * Tile.Size)));
                }
            }

            return new MultiScreenRenderer(sections.ToArray(), new Vector2(width * Tile.Size, height * Tile.Size));
        }
        else
        {
            Texture2D layerTexture = Engine.TextureCache.GetOrCreateObject(
                pointer: layerCachePointer,
                id: cacheId,
                data: (
                    TileSet: tileSet, 
                    Width: width, 
                    Height: height, 
                    TileMap: tileMap, 
                    BaseTileIndex: baseTileIndex, 
                    Is8Bit: is8Bit),
                createObjFunc: static data => new IndexedTiledTexture2D(
                    width: data.Width, 
                    height: data.Height, 
                    tileSet: data.TileSet, 
                    tileMap: data.TileMap, 
                    baseTileIndex: data.BaseTileIndex, 
                    is8Bit: data.Is8Bit));

            return new TextureScreenRenderer(layerTexture);
        }
    }

    public IScreenRenderer CreateTileMapRenderer(
        RenderOptions renderOptions, 
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
            byte[] tileSet = is8Bit ? DynamicTileSet8bpp : DynamicTileSet4bpp;

            TileMapScreenRenderer renderer = new(
                // Use the tilekit as the cache pointer since multiple layers can share the same tilekit and we're
                // caching per tile, but make the pointer differ depending on if it's 4-bit or 8-bit.
                cachePointer: TileKit.Offset + (is8Bit ? 1 : 0),
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
            IReadOnlyList<TileKitAnimation> animations = animatedTilekitManager?.GetUsedAnimations(this, TileKit, tileMap, baseTileIndex, is8Bit, false);

            // If it's animated then we create a separate texture for each animation frame
            if (animations?.Count > 0)
            {
                // Should hopefully never happen in the game. Otherwise we need to add support for this and find the lowest common denominator.
                if (animations.Count > 1)
                    throw new InvalidOperationException("Multiple animations for the same static layer is currently not supported");

                TileKitAnimation anim = animations[0];

                // Create one screen renderer per animation frame
                IScreenRenderer[] layerScreenRenderers = new IScreenRenderer[anim.TileKit.FramesCount];

                int tileSize = is8Bit ? TileSize8bpp : TileSize4bpp;

                byte[] sourceTileSet = is8Bit ? TileKit.Tiles8bpp : TileKit.Tiles4bpp;
                int[] mappingTable = is8Bit ? GameToVramMappingTable8bpp : GameToVramMappingTable4bpp;

                byte[] tileSet = new byte[StaticTileSet.Length];
                Array.Copy(StaticTileSet, tileSet, StaticTileSet.Length);

                for (int frame = 0; frame < layerScreenRenderers.Length; frame++)
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

                    layerScreenRenderers[frame] = CreateTextureScreenRenderer(
                        layerCachePointer: layerCachePointer, 
                        cacheId: frame, 
                        maxCacheId: layerScreenRenderers.Length, 
                        tileSet: tileSet, 
                        width: width, 
                        height: height, 
                        tileMap: tileMap, 
                        baseTileIndex: baseTileIndex, 
                        is8Bit: is8Bit);
                }

                MultiSelectableScreenRenderer renderer = new(layerScreenRenderers);
                animatedTilekitManager.AddTextureRenderer(renderer, anim.TileKit.Speed, anim.TileKit.FramesCount);
                return renderer;
            }
            // If it's not animated then we just add a single texture
            else
            {
                return CreateTextureScreenRenderer(
                    layerCachePointer: layerCachePointer, 
                    cacheId: 0, 
                    maxCacheId: 1, 
                    tileSet: StaticTileSet, 
                    width: width, 
                    height: height, 
                    tileMap: tileMap, 
                    baseTileIndex: baseTileIndex, 
                    is8Bit: is8Bit);
            }
        }
    }

    public void LoadTileKit(TileKit tileKit, TileMappingTable tileMappingTable, int vramLength8bpp, bool isAffine, int defaultPalette)
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

        // Set properties
        TileKit = tileKit;
        StaticTileSet = tileSet;
        DynamicTileSet8bpp = new byte[tileKit.Tiles8bpp.Length + TileSize8bpp];
        Array.Copy(tileKit.Tiles8bpp, 0, DynamicTileSet8bpp, TileSize8bpp, tileKit.Tiles8bpp.Length);
        DynamicTileSet4bpp = new byte[tileKit.Tiles4bpp.Length + TileSize4bpp];
        Array.Copy(tileKit.Tiles4bpp, 0, DynamicTileSet4bpp, TileSize4bpp, tileKit.Tiles4bpp.Length);
        GameToVramMappingTable4bpp = mappingTable4bpp;
        GameToVramMappingTable8bpp = mappingTable8bpp;
        Palettes = palettes;
        SelectedPaletteIndex = defaultPalette;
    }
}