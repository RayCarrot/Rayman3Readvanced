using System.IO;

namespace GbaMonoGame.Rayman3.J2ME;

// NOTE: Unused in Readvanced, so just an empty dummy implementation

// Replaces javax.microedition.media.Player
public class Player
{
    // From javax.microedition.media.Manager.createPlayer()
    public Player(Stream stream, string type) { }
    
    public PLAYER_STATE getState()
    {
        return PLAYER_STATE.UNKNOWN;
    }

    public void setLoopCount(int count) { }

    // From javax.microedition.media.control.VolumeControl
    public void setVolumeLevel(int level) { }

    public void start() { }
    public void stop() { }

    public void addPlayerListener(object playerListener) { }
    public void prefetch() { }
    public void realize() { }

    public void close() { }
}