using System.Collections.Frozen;

namespace GbaMonoGame.Editor;

public static class EditorData
{
    private static FrozenDictionary<int, ActorDefinition> ActorDefinitions { get; set; }

    public static void Init(ActorDefinition[] actorDefinitions)
    {
        ActorDefinitions = actorDefinitions.ToFrozenDictionary(x => x.ActorId);
    }
}