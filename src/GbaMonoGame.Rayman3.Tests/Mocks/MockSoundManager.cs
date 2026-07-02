using SoLoud;

namespace GbaMonoGame.Rayman3.Tests;

public class MockSoundManager : ISoundManager
{
    public Soloud GetSoloud() => null;
    public void Init() { }
    public void ReInit() { }
    public void UnInit() { }
    public void Dispose() { }
}