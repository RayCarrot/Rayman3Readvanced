using System;
using BinarySerializer;

namespace GbaMonoGame.Editor;

public class ActorDefinition
{
    public ActorDefinition(int actorId, string name, ActorActionDefinition[] actions)
    {
        ActorId = actorId;
        Name = name;
        Actions = actions;
    }

    public int ActorId { get; }
    public string Name { get; }
    public ActorActionDefinition[] Actions { get; }
}

public class ActorDefinition<T> : ActorDefinition
    where T : Enum
{
    public ActorDefinition(T actorId, string name, ActorActionDefinition[] actions) : base(CastTo<int>.From(actorId), name, actions) { }
}