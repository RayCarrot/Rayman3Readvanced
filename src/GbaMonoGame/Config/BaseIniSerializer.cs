using System.Collections.Generic;

namespace GbaMonoGame;

public abstract class BaseIniSerializer
{
    public abstract T Serialize<T>(T value, string sectionKey, string valueKey);
    public abstract Dictionary<TKey, TValue> SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string sectionKey);
}