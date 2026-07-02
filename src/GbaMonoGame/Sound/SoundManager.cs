using SoLoud;

namespace GbaMonoGame;

public class SoundManager : ISoundManager
{
    public Soloud Soloud { get; private set; }

    public Soloud GetSoloud() => Soloud;

    public void Init()
    {
        Soloud = new Soloud();
        Soloud.init();
    }

    public void ReInit()
    {
        Soloud.init();
    }

    public void UnInit()
    {
        Soloud?.deinit();
        Soloud?.Dispose();
        Soloud = null;
    }

    public void Dispose()
    {
        UnInit();
    }
}