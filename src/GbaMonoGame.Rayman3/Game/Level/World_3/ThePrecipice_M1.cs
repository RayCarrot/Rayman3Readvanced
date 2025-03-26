using System;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class ThePrecipice_M1 : FrameSideScroller
{
    public ThePrecipice_M1(MapId mapId) : base(mapId)
    {
        GreenColor = 25 * Speed;
        AddColor = true;
        Unused1 = (short)Random.GetNumber(127);
    }

    private const int MinColor = 24;
    private const int MaxColor = 31;
    private const int Speed = 8;

    public byte GreenColor { get; set; }
    public bool AddColor { get; set; }

    public PaletteTexture[] PaletteTextures { get; set; }

    // Unused
    public short Unused1 { get; set; }
    public short Unused2 { get; set; }

    public override void Init()
    {
        base.Init();
        Unused2 = 0;

        // Get the original colors
        GfxTileKitManager tileKitManager = Scene.Playfield.GfxTileKitManager;
        Color[] originalColors = tileKitManager.SelectedPalette.Colors;
        int palLength = originalColors.Length;

        // Create an array for the new colors
        Color[] colors = new Color[palLength];
        Array.Copy(originalColors, colors, palLength);

        // Create a palette for each possible green color
        PaletteTextures = new PaletteTexture[(MaxColor - MinColor) * Speed + 1];
        for (int i = 0; i < PaletteTextures.Length; i++)
        {
            float green = (MinColor + i / (float)Speed) * (255 / 31f);
            colors[49] = colors[49] with { G = (byte)green };

            PaletteTextures[i] = new PaletteTexture(
                Texture: Engine.TextureCache.GetOrCreateObject(
                    pointer: tileKitManager.SelectedPalette.CachePointer,
                    id: 1 + i,
                    data: colors,
                    createObjFunc: static c => new PaletteTexture2D(c)),
                PaletteIndex: 0);
        }
    }

    public override void Step()
    {
        // NOTE: The game only update the palette every 8 frames, but we do it every frame
        if (AddColor)
        {
            GreenColor++;

            if (GreenColor >= MaxColor * Speed)
                AddColor = false;
        }
        else
        {
            GreenColor--;

            if (GreenColor <= MinColor * Speed)
                AddColor = true;
        }

        TgxPlayfield2D playfield = (TgxPlayfield2D)Scene.Playfield;
        foreach (TgxTileLayer tileLayer in playfield.TileLayers)
            tileLayer.Screen.RenderOptions.PaletteTexture = PaletteTextures[GreenColor - MinColor * Speed];

        base.Step();
    }
}