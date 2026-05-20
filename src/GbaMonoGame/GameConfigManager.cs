using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GbaMonoGame;

public class GameConfigManager
{
    private readonly JsonSerializerOptions _configJsonOptions = new() { ReadCommentHandling = JsonCommentHandling.Skip };
    private readonly Dictionary<Type, object> _config = new();

    private T DeserializeConfig<T>(string filePath)
    {
        filePath = Path.Combine(Paths.AssetsDirectoryName, filePath);
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<T>(json, _configJsonOptions);
    }

    public void Load<T>(string filePath)
    {
        // Load the config
        _config[typeof(T)] = DeserializeConfig<T>(filePath);
    }

    public T Get<T>()
    {
        // Return the config
        if (_config.TryGetValue(typeof(T), out object config))
            return (T)config;

        throw new InvalidOperationException($"Config of type {typeof(T)} has not been loaded");
    }
}