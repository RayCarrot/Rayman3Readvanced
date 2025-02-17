using System;
using System.Collections.Generic;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.TgxEngine;

public class AnimatedTilekitManager
{
    public AnimatedTilekitManager(AnimatedTileKit[] animatedTileKits)
    {
        Animations = new TileKitAnimation[animatedTileKits.Length];
        for (int i = 0; i < Animations.Length; i++)
            Animations[i] = new TileKitAnimation(animatedTileKits[i]);

        _dynamicTileRenderers = new List<TileMapScreenRenderer>();
        _textureAnimations = new List<TextureAnimation>();
    }

    private readonly List<TileMapScreenRenderer> _dynamicTileRenderers;
    private readonly List<TextureAnimation> _textureAnimations;

    public TileKitAnimation[] Animations { get; }

    /// <summary>
    /// Gets the animations used for the specified tile map
    /// </summary>
    public IReadOnlyList<TileKitAnimation> GetUsedAnimations(
        GfxTileKitManager tileKitManager, 
        TileKit tileKit, 
        MapTile[] tileMap, 
        int baseTileIndex, 
        bool is8Bit, 
        bool isDynamic)
    {
        if (isDynamic)
            throw new NotSupportedException("Getting used animations for dynamic layers is currently not supported");

        // Keep track of all used animations
        List<TileKitAnimation> animations = new();

        // Get the mapping table for game tiles to VRAM tiles
        int[] mappingTable = is8Bit ? tileKitManager.GameToVramMappingTable8bpp : tileKitManager.GameToVramMappingTable4bpp;

        // Check each animation
        foreach (TileKitAnimation anim in Animations)
        {
            // Check if the animation has the same bpp as the tile map
            if (anim.TileKit.Is8Bit == is8Bit)
            {
                // Keep track of all VRAM tiles used by the animation
                List<int> animatedVramTiles = new();
                foreach (ushort tile in anim.TileKit.Tiles)
                {
                    int value = mappingTable[tile - 1];
                    
                    // The value might be 0 if there is no mapping (i.e. the tile is only used dynamically)
                    if (value != 0)
                        animatedVramTiles.Add(value);
                }

                // Check if any of the animated VRAM tiles are used by the tile map
                if (animatedVramTiles.Count != 0)
                {
                    foreach (MapTile tile in tileMap)
                    {
                        if (animatedVramTiles.Contains(baseTileIndex + tile.TileIndex))
                        {
                            animations.Add(anim);
                            break;
                        }
                    }
                }
            }
        }

        return animations;
    }

    public void AddTileMapRenderer(TileMapScreenRenderer renderer, bool isDynamic)
    {
        if (!isDynamic)
            throw new NotSupportedException("Static tile maps can currently not be animated");

        _dynamicTileRenderers.Add(renderer);
    }

    public void AddTextureRenderer(MultiSelectableScreenRenderer renderer, int speed, int framesCount)
    {
        _textureAnimations.Add(new TextureAnimation(renderer, speed, framesCount));
    }

    public void Step()
    {
        // Update dynamic tile animations
        foreach (TileKitAnimation anim in Animations)
        {
            anim.Timer++;

            if (anim.Timer >= anim.TileKit.Speed)
            {
                anim.Frame++;
                anim.Timer = 0;

                if (anim.Frame >= anim.TileKit.FramesCount)
                    anim.Frame = 0;

                for (int i = 0; i < anim.TileKit.TilesCount; i++)
                {
                    foreach (TileMapScreenRenderer renderer in _dynamicTileRenderers)
                    {
                        if (renderer.Is8Bit == anim.TileKit.Is8Bit)
                            renderer.ReplaceTile(anim.TileKit.Tiles[i], anim.TileKit.Tiles[i] + anim.Frame * anim.TileKit.TilesStep);
                    }
                }
            }
        }

        // Update texture animations
        foreach (TextureAnimation anim in _textureAnimations)
        {
            anim.Timer++;

            if (anim.Timer >= anim.Speed)
            {
                anim.Frame++;
                anim.Timer = 0;

                if (anim.Frame >= anim.FramesCount)
                    anim.Frame = 0;

                anim.Renderer.SelectedScreenRenderer = anim.Frame;
            }
        }
    }

    private class TextureAnimation
    {
        public TextureAnimation(MultiSelectableScreenRenderer renderer, int speed, int framesCount)
        {
            Renderer = renderer;
            Speed = speed;
            FramesCount = framesCount;
        }

        public MultiSelectableScreenRenderer Renderer { get; }
        public int Speed { get; }
        public int FramesCount { get; }
        public int Timer { get; set; }
        public int Frame { get; set; }
    }
}