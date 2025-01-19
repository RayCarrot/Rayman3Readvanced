using System;
using System.Collections.Generic;
using BinarySerializer;

namespace GbaMonoGame;

public class LocationCache<T>
{
    public LocationCache(Pointer offset)
    {
        Offset = offset;
    }

    private Dictionary<long, T> Objects { get; } = new();

    public Pointer Offset { get; }

    public void RegisterObject(T cachableObject, long id)
    {
        Objects[id] = cachableObject;
    }

    public bool TryGetObject(long id, out T cachableObject)
    {
        return Objects.TryGetValue(id, out cachableObject);
    }

    public T GetOrCreateObject(long id, Func<T> createObjFunc)
    {
        if (TryGetObject(id, out T cachableObject))
            return cachableObject;

        cachableObject = createObjFunc();
        RegisterObject(cachableObject, id);
        return cachableObject;
    }

    public T GetOrCreateObject<U>(long id, U data, Func<U, T> createObjFunc)
    {
        if (TryGetObject(id, out T cachableObject))
            return cachableObject;

        cachableObject = createObjFunc(data);
        RegisterObject(cachableObject, id);
        return cachableObject;
    }

    public void Clear()
    {
        foreach (T cachableObject in Objects.Values)
        {
            if (cachableObject is IDisposable disposable)
                disposable.Dispose();
        }

        Objects.Clear();
    }
}