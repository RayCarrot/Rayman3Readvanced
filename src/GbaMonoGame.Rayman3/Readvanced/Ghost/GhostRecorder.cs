using System;
using System.Collections.Generic;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostRecorder
{
    public GhostRecorder(Scene2D scene, ActorType[] actorTypes)
    {
        Scene = scene;
        _actorTypes = actorTypes;
        _isMode7 = scene.Playfield is TgxPlayfieldMode7;
    }

    private const int InitialFramesCapacity = 1024;

    private readonly List<GhostFrame> _frames = new(InitialFramesCapacity);
    private readonly ActorType[] _actorTypes;
    private readonly bool _isMode7;

    public Scene2D Scene { get; }

    public void Step()
    {
        List<GhostActorFrame> actorFrames = [];
        foreach (BaseActor actor in new EnabledAlwaysActorIterator(Scene))
        {
            if (Array.IndexOf(_actorTypes, (ActorType)actor.Type) >= 0)
                actorFrames.Add(GhostActorFrame.FromActor(actor, _isMode7));
        }

        foreach (BaseActor actor in new EnabledActorIterator(Scene))
        {
            if (Array.IndexOf(_actorTypes, (ActorType)actor.Type) >= 0)
                actorFrames.Add(GhostActorFrame.FromActor(actor, _isMode7));
        }

        _frames.Add(new GhostFrame
        {
            Pre_IsMode7 = _isMode7,
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
            IsMode7 = _isMode7,
            Frames = _frames.ToArray()
        };
    }
}