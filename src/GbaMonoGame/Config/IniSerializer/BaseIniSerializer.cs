using System.Collections.Generic;

namespace GbaMonoGame;

public abstract class BaseIniSerializer
{
    public string CurrentSectionKey { get; set; }

    public T SerializeSectionObject<T>(T sectionObject)
        where T : IniSectionObject, new()
    {
        sectionObject ??= new T();

        string prevSectionKey = CurrentSectionKey;
        CurrentSectionKey = sectionObject.SectionKey;
        
        sectionObject.Serialize(this);

        CurrentSectionKey = prevSectionKey;

        return sectionObject;
    }

    public abstract T Serialize<T>(T value, string sectionKey, string valueKey);
    public T Serialize<T>(T value, string valueKey) => 
        Serialize(value, CurrentSectionKey, valueKey);
    
    public abstract Dictionary<TKey, TValue> SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string sectionKey);
    public Dictionary<TKey, TValue> SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) =>
        SerializeDictionary(dictionary, CurrentSectionKey);
}