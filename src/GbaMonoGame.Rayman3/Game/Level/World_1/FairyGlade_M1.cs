using System;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class FairyGlade_M1 : FrameSideScroller
{
    public FairyGlade_M1(MapId mapId) : base(mapId) { }

    // NOTE: The game uses 16, but it only updates every 4 frames. We instead update every frame.
    private const int WaterGlowMaxValue = 16 * 4;

    public int WaterGlowValue { get; set; }
    public PaletteTexture[] WaterPaletteTextures { get; set; }

    public override void Init()
    {
        base.Init();

        if (Rom.Platform == Platform.GBA || Engine.Config.UseGbaEffectsOnNGage)
        {
            TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
            TextureScreenRenderer renderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
            cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(renderer.Texture, [32, 120, 227]);
        }
    }

    // Unused water glow code
    private void InitWaterGlow()
    {
        // Create a palette texture for each modified palette frame
        WaterPaletteTextures = new PaletteTexture[WaterGlowMaxValue + 1];

        // Get the original colors
        GfxTileKitManager tileKitManager = Scene.Playfield.GfxTileKitManager;
        Color[] originalColors = tileKitManager.SelectedPalette.Colors;
        int palLength = originalColors.Length;

        // Create the target colors for sub-palette 7 (starting from color 2)
        Color[] targetColors =
        [
            new RGB555Color(0x51ea).ToColor(),
            new RGB555Color(0x4dc9).ToColor(),
            new RGB555Color(0x4524).ToColor(),
            new RGB555Color(0x562c).ToColor(),
            new RGB555Color(0x4987).ToColor(),
            new RGB555Color(0x4586).ToColor(),
            new RGB555Color(0xdf).ToColor(), // Weird red color
            new RGB555Color(0x4a4c).ToColor(),
            new RGB555Color(0x49a7).ToColor(),
            new RGB555Color(0x5a4e).ToColor(),
            new RGB555Color(0x4565).ToColor(),
            new RGB555Color(0x49a8).ToColor(),
            new RGB555Color(0x4e90).ToColor(),
        ];

        // Create an array for the new colors
        Color[] colors = new Color[palLength];
        Array.Copy(originalColors, colors, palLength);

        // Create a palette texture for each 
        for (int value = 0; value < WaterPaletteTextures.Length; value++)
        {
            // Lerp the colors in sub-palette 7 (starting from color 2)
            for (int subPaletteIndex = 0; subPaletteIndex < targetColors.Length; subPaletteIndex++)
            {
                int fullPalIndex = 16 * 7 + 2 + subPaletteIndex;
                colors[fullPalIndex] = Color.Lerp(originalColors[fullPalIndex], targetColors[subPaletteIndex], value / (float)WaterGlowMaxValue);
            }

            WaterPaletteTextures[value] = new PaletteTexture(
                Texture: Engine.TextureCache.GetOrCreateObject(
                    pointer: tileKitManager.SelectedPalette.CachePointer,
                    id: value,
                    data: colors,
                    createObjFunc: static c => new PaletteTexture2D(c)),
                PaletteIndex: 0);
        }

        WaterGlowValue = 0;
    }

    private void StepWaterGlow()
    {
        // NOTE: The game only updates this every 4 frames

        TgxPlayfield2D playfield = (TgxPlayfield2D)Scene.Playfield;
        TgxTileLayer mainLayer = playfield.TileLayers[2];

        int index = WaterGlowValue <= WaterGlowMaxValue
            ? WaterGlowValue
            : WaterGlowMaxValue * 2 - WaterGlowValue;

        mainLayer.Screen.RenderOptions.PaletteTexture = WaterPaletteTextures[index];

        WaterGlowValue++;

        if (WaterGlowValue > WaterGlowMaxValue * 2)
            WaterGlowValue = 0;
    }
}