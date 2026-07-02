using System;
using SoLoud;

namespace GbaMonoGame;

public interface ISoundManager : IDisposable
{
    Soloud GetSoloud();
    void Init();
    void ReInit();
    void UnInit();
}