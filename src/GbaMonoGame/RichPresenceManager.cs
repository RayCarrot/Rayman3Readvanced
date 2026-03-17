using System;
using DiscordRPC;

namespace GbaMonoGame;

public class RichPresenceManager : IDisposable
{
    public RichPresenceManager()
    {
        DiscordClient = new DiscordRpcClient(AppId);
        StartTime = DateTime.UtcNow;
    }

    private const string AppId = "1483522842304712939";

    private DateTime StartTime { get; }
    private DiscordRpcClient DiscordClient { get; }

    public void Initialize()
    {
        if (DiscordClient.Initialize())
            SetIdlePresence();
    }

    public void SetIdlePresence()
    {
        DiscordClient.SetPresence(new RichPresence()
        {
            Timestamps = new Timestamps(StartTime)
        });
    }

    public void SetPresence(string presence)
    {
        DiscordClient.SetPresence(new RichPresence()
        {
            Timestamps = new Timestamps(StartTime),
            Details = presence,
        });
    }

    public void Dispose()
    {
        DiscordClient.Dispose();
    }
}