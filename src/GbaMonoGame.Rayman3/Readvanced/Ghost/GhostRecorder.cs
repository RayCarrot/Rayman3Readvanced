using System;
using System.Collections.Generic;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostRecorder
{
    public GhostRecorder(Scene2D scene, ActorType[] actorTypes)
    {
        Scene = scene;
        _actorTypes = actorTypes;
    }

    private const int InitialFramesCapacity = 1024;

    private readonly List<GhostFrame> _frames = new(InitialFramesCapacity);
    private readonly ActorType[] _actorTypes;

    public Scene2D Scene { get; }

    public void Step()
    {
        List<GhostActorFrame> actorFrames = [];
        foreach (BaseActor actor in new EnabledAlwaysActorIterator(Scene))
        {
            if (Array.IndexOf(_actorTypes, (ActorType)actor.Type) >= 0)
                actorFrames.Add(GhostActorFrame.FromActor(actor));
        }

        foreach (BaseActor actor in new EnabledActorIterator(Scene))
        {
            if (Array.IndexOf(_actorTypes, (ActorType)actor.Type) >= 0)
                actorFrames.Add(GhostActorFrame.FromActor(actor));
        }

        _frames.Add(new GhostFrame
        {
            Actors = actorFrames.ToArray(),
            Input = JoyPad.Current.KeyStatus
        });
    }

    public void Reset()
    {
        _frames.Clear();
    }

    public GhostMapData GetData(MapId mapId)
    {
        return new GhostMapData
        {
            MapId = mapId,
            Frames = _frames.ToArray()
        };
    }
}