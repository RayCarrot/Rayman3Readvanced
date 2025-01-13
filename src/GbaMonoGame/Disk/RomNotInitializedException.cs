using System;

namespace GbaMonoGame;

public class RomNotInitializedException : Exception
{
    public RomNotInitializedException()
    {
    }

    public RomNotInitializedException(string message) : base(message)
    {
    }

    public RomNotInitializedException(string message, Exception inner) : base(message, inner)
    {
    }
}