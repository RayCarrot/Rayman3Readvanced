using System;
using System.Collections.Generic;
using BinarySerializer;

namespace GbaMonoGame;

public class Cache<T>
    where T : class
{
    private Dictionary<Pointer, LocationCache<T>> Locations { get; } = new();

    public void RegisterObject(T cachableObject, Pointer pointer, long id)
    {
        if (!Locations.TryGetValue(pointer, out LocationCache<T> locationCache))
        {
            locationCache = new LocationCache<T>(pointer);
            Locations.Add(pointer, locationCache);
        }

        locationCache.RegisterObject(cachableObject, id);
    }

    public bool TryGetObject(Pointer pointer, long id, out T cachableObject)
    {
        if (!Locations.TryGetValue(pointer, out LocationCache<T> locationCache))
        {
            cachableObject = null;
            return false;
        }

        return locationCache.TryGetObject(id, out cachableObject);
    }

    public void SetObject(Pointer pointer, long id, T obj)
    {
        RegisterObject(obj, pointer, id);
    }

    public T GetOrCreateObject(Pointer pointer, long id, Func<T> createObjFunc)
    {
        if (TryGetObject(pointer, id, out T cachableObject))
            return cachableObject;

        cachableObject = createObjFunc();
        RegisterObject(cachableObject, pointer, id);
        return cachableObject;
    }

    public T GetOrCreateObject<U>(Pointer pointer, long id, U data, Func<U, T> createObjFunc)
    {
        if (TryGetObject(pointer, id, out T cachableObject))
            return cachableObject;

        cachableObject = createObjFunc(data);
        RegisterObject(cachableObject, pointer, id);
        return cachableObject;
    }

    public LocationCache<T> GetOrCreateLocationCache(Pointer pointer)
    {
        if (Locations.TryGetValue(pointer, out LocationCache<T> locationCache))
            return locationCache;

        locationCache = new LocationCache<T>(pointer);
        Locations.Add(pointer, locationCache);
        return locationCache;
    }

    public void Clear()
    {
        foreach (LocationCache<T> locationCache in Locations.Values)
            locationCache.Clear();
    }
}