using BinarySerializer;

namespace GbaMonoGame.AnimEngine;

public class SpritePalettes
{
    public SpritePalettes(SpritePalettesResource resource) : this(resource.Palettes, resource.Offset) { }

    public SpritePalettes(PaletteResource[] palettes, Pointer cachePointer)
    {
        Palettes = new Palette[palettes.Length];
        for (int i = 0; i < Palettes.Length; i++)
        {
            PaletteResource paletteResource = palettes[i];
            Palettes[i] = Engine.PaletteCache.GetOrCreateObject(
                pointer: paletteResource.Offset,
                id: 0,
                data: paletteResource,
                createObjFunc: p => new Palette(p));
        }

        CachePointer = cachePointer;
    }

    public SpritePalettes(Palette[] palettes, Pointer cachePointer)
    {
        Palettes = palettes;
        CachePointer = cachePointer;
    }

    public Palette[] Palettes { get; }
    public Pointer CachePointer { get; }
}