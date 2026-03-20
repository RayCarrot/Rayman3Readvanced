using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace GbaMonoGame.Engine2d;

public static class ActorFactory
{
    private static FrozenDictionary<int, CreateActor> _actorCreations;
    private static Func<int, string> _getActorTypeNameFunc;

    public static void Init<T>(Dictionary<T, CreateActor> actorCreations, Func<int, string> getActorTypeNameFunc)
        where T : Enum
    {
        _actorCreations = actorCreations.ToFrozenDictionary(x => (int)(object)x.Key, x => x.Value);
        _getActorTypeNameFunc = getActorTypeNameFunc;
    }

    public static void Init(Dictionary<int, CreateActor> actorCreations, Func<int, string> getActorTypeNameFunc)
    {
        _actorCreations = actorCreations.ToFrozenDictionary();
        _getActorTypeNameFunc = getActorTypeNameFunc;
    }

    public static BaseActor Create(int instanceId, Scene2D scene, ActorResource actorResource)
    {
        if (!_actorCreations.TryGetValue(actorResource.Type, out CreateActor create))
            return new DummyActor(instanceId, scene, actorResource);

        return create(instanceId, scene, actorResource);
    }

    public static string GetActorTypeName(int actorType) => _getActorTypeNameFunc(actorType);

    public delegate BaseActor CreateActor(int instanceId, Scene2D scene, ActorResource actorResource);
}