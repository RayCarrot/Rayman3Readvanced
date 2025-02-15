using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public static class ColorHelpers
{
    public static Color FromRGB555(int value)
    {
        const float factor = 31f;
        return new Color(
            (value & 0x1F) / factor,
            ((value >> 5) & 0x1F) / factor,
            ((value >> 10) & 0x1F) / factor);
    }
}