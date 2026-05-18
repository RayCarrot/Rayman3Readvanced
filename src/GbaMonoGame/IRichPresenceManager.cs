using System;

namespace GbaMonoGame;

public interface IRichPresenceManager : IDisposable
{
    void SetIdlePresence();
    void SetPresence(string presence);
}