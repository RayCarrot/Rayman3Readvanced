using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class IniDeserializer : BaseIniSerializer
{
    public IniDeserializer(string filePath)
    {
        if (File.Exists(filePath))
        {
            FileIniDataParser parser = new();
            Data = parser.ReadFile(filePath);
        }
    }

    private IniData Data { get; }

    private T ParseValue<T>(string stringValue)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)stringValue;
        }
        else if (typeof(T) == typeof(int?))
        {
            if (stringValue == String.Empty)
                return (T)(object)null;
            else
                return (T)(object)Int32.Parse(stringValue);
        }
        else if (typeof(T) == typeof(float))
        {
            return (T)(object)Single.Parse(stringValue, CultureInfo.InvariantCulture);
        }
        else if (typeof(T) == typeof(bool))
        {
            return (T)(object)Boolean.Parse(stringValue);
        }
        else if (typeof(T).IsEnum)
        {
            return (T)Enum.Parse(typeof(T), stringValue);
        }
        else if (typeof(T) == typeof(Vector2))
        {
            string[] values = stringValue.Split('x');
            return (T)(object)new Vector2(Single.Parse(values[0], CultureInfo.InvariantCulture), Single.Parse(values[1], CultureInfo.InvariantCulture));
        }
        else if (typeof(T) == typeof(Point))
        {
            string[] values = stringValue.Split('x');
            return (T)(object)new Point(Int32.Parse(values[0]), Int32.Parse(values[1]));
        }
        else
        {
            throw new InvalidOperationException($"Not implemented parsing values of type {typeof(T).Name}");
        }
    }

    public override T Serialize<T>(T value, string sectionKey, string valueKey)
    {
        if (Data == null || !Data.Sections.ContainsSection(sectionKey) || !Data[sectionKey].ContainsKey(valueKey))
            return value;

        string stringValue = Data[sectionKey][valueKey];

        try
        {
            return ParseValue<T>(stringValue);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to parse value '{stringValue}' for key '{valueKey}' in section '{sectionKey}': {e.Message}");
            return value;
        }
    }

    public override Dictionary<TKey, TValue> SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string sectionKey)
    {
        if (Data == null || !Data.Sections.ContainsSection(sectionKey))
            return dictionary;

        try
        {
            Dictionary<TKey, TValue> newDictionary = new();

            foreach (KeyData keyData in Data[sectionKey])
            {
                TKey key = ParseValue<TKey>(keyData.KeyName);
                TValue value = ParseValue<TValue>(keyData.Value);

                newDictionary[key] = value;
            }

            return newDictionary;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to parse dictionary in section '{sectionKey}': {e.Message}");
            return dictionary;
        }
    }
}