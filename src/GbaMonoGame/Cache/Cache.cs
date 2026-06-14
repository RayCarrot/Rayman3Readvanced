using System;
using System.Collections.Generic;

namespace GbaMonoGame;

public class Cache<TKey, TValue>
{
    private Dictionary<TKey, TValue> Objects { get; } = new();

    public void RegisterObject(TValue cachableObject, TKey id)
    {
        Objects[id] = cachableObject;
    }

    public bool TryGetObject(TKey id, out TValue cachableObject)
    {
        return Objects.TryGetValue(id, out cachableObject);
    }

    public TValue GetOrCreateObject(TKey id, Func<TValue> createObjFunc)
    {
        if (TryGetObject(id, out TValue cachableObject))
            return cachableObject;

        cachableObject = createObjFunc();
        RegisterObject(cachableObject, id);
        return cachableObject;
    }

    public TValue GetOrCreateObject<U>(TKey id, U data, Func<U, TValue> createObjFunc)
    {
        if (TryGetObject(id, out TValue cachableObject))
            return cachableObject;

        cachableObject = createObjFunc(data);
        RegisterObject(cachableObject, id);
        return cachableObject;
    }

    public int GetCount() => Objects.Count;

    public void Clear()
    {
        foreach (TValue cachableObject in Objects.Values)
        {
            if (cachableObject is IDisposable disposable)
                disposable.Dispose();
        }

        Objects.Clear();
    }
}