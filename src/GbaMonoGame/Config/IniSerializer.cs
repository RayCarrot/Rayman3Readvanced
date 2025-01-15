using System;
using System.Collections.Generic;
using System.Globalization;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class IniSerializer : BaseIniSerializer
{
    public IniSerializer()
    {
        Data = new IniData();
    }

    private IniData Data { get; }

    private string ValueToString<T>(T value)
    {
        if (value is string s)
        {
            return s;
        }
        else if (value is float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }
        else if (value is bool || typeof(T).IsEnum)
        {
            return value.ToString();
        }
        else if (value is Point point)
        {
            return $"{point.X}x{point.Y}";
        }
        else
        {
            throw new InvalidOperationException($"Not implemented values of type {typeof(T).Name}");
        }
    }

    public override T Serialize<T>(T value, string sectionKey, string valueKey)
    {
        Data[sectionKey][valueKey] = ValueToString(value);
        return value;
    }

    public override Dictionary<TKey, TValue> SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary,
        string sectionKey)
    {
        foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
            Data[sectionKey][ValueToString(keyValuePair.Key)] = ValueToString(keyValuePair.Value);

        return dictionary;
    }

    public void Save(string filePath)
    {
        FileIniDataParser parser = new();
        parser.WriteFile(filePath, Data);
    }
}