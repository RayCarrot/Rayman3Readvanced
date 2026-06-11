using System;

namespace GbaMonoGame.Rayman3.J2me;

// Replaces java.lang.System
public static class System
{
    public static long currentTimeMillis()
    {
        // Translate frames to milliseconds based on the current framerate
        uint frames = GameTime.ElapsedFrames;
        return (long)(1000f / Engine.App.Framerate * frames);
    }

    public static void gc()
    {
        // Do nothing - we let the C# garbage collector run on its own
    }

    public static void arraycopy(Array src, int srcPos, Array dest, int destPos, int length)
    {
        Array.Copy(src, srcPos, dest, destPos, length);
    }

    public static void println(string s)
    {
        // Do nothing - this only gets called from unused functions
    }
}