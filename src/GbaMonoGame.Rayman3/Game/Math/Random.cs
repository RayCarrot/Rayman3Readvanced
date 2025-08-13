namespace GbaMonoGame.Rayman3;

public static class Random
{
    private static uint _seed;

    public static uint GetSeed()
    {
        return _seed;
    }

    public static void SetSeed(uint seed)
    {
        _seed = seed;
    }

    public static int GetNumber(int max)
    {
        _seed = _seed * 0x19660d + 0x3c6ef35f;
        return MathHelpers.Mod((int)(_seed >> 0x10), max);
    }
}