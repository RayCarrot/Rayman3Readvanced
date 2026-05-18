using System;
using DiscordRPC;

namespace GbaMonoGame;

public class DiscordRichPresenceManager : IRichPresenceManager
{
    public DiscordRichPresenceManager()
    {
        DiscordClient = new DiscordRpcClient(AppId);
        StartTime = DateTime.UtcNow;

        if (DiscordClient.Initialize())
            SetIdlePresence();
    }

    private const string AppId = "1483522842304712939";

    private DateTime StartTime { get; }
    private DiscordRpcClient DiscordClient { get; }

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