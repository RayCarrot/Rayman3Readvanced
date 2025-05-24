using System;
using System.Collections.Generic;

namespace GbaMonoGame;

public class DisposableResources
{
    private readonly List<IDisposable> _resources = [];

    public void Register(IDisposable resource)
    {
        _resources.Add(resource);
    }

    public void DisposeAll()
    {
        foreach (IDisposable disposable in _resources)
            disposable.Dispose();

        _resources.Clear();
    }
}