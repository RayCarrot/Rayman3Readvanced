using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using BinarySerializer;

namespace GbaMonoGame.Rayman3;

public static class LevelFactory
{
    private static FrozenDictionary<int, CreateLevel> _levelCreations;

    public static void Init<T>(Dictionary<T, CreateLevel> levelCreations)
        where T : Enum
    {
        _levelCreations = levelCreations.ToFrozenDictionary(x => CastTo<int>.From(x.Key), x => x.Value);
    }

    public static void Init(Dictionary<int, CreateLevel> levelCreations)
    {
        _levelCreations = levelCreations.ToFrozenDictionary();
    }

    public static Frame Create(MapId mapId)
    {
        if (!_levelCreations.TryGetValue((int)mapId, out CreateLevel create))
            throw new Exception($"The level {mapId} is not defined");

        return create(mapId);
    }

    public delegate Frame CreateLevel(MapId mapId);
}