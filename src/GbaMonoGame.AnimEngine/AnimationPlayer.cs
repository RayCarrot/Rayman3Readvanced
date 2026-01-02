using System;
using System.Collections.Generic;

namespace GbaMonoGame.AnimEngine;

public class AnimationPlayer
{
    public AnimationPlayer(bool is8Bit, Action<short> soundEventCallback)
    {
        Is8Bit = is8Bit;
        _soundEventCallback = soundEventCallback;

        _unsortedObjects = new List<AObject>();
        _sortedObjects = new List<AObject>();
    }

    private readonly Action<short> _soundEventCallback;
    private readonly List<AObject> _unsortedObjects;
    private readonly List<AObject> _sortedObjects;

    public bool Is8Bit { get; }

    public void SoundEventRequest(short soundId)
    {
        _soundEventCallback?.Invoke(soundId);
    }

    // NOTE: A bit unsure what to name this. Same as the normal Play function, but without sorting and drawn first.
    public void PlayFront(AObject obj)
    {
        _unsortedObjects.Insert(0, obj);
    }

    public void Play(AObject obj)
    {
        for (int i = 0; i < _sortedObjects.Count; i++)
        {
            if (_sortedObjects[i].BgPriority > obj.BgPriority || 
                (_sortedObjects[i].BgPriority == obj.BgPriority && _sortedObjects[i].ObjPriority > obj.ObjPriority) || 
                (_sortedObjects[i].BgPriority == obj.BgPriority && _sortedObjects[i].ObjPriority == obj.ObjPriority && _sortedObjects[i].YPriority >= obj.YPriority))
            {
                _sortedObjects.Insert(i, obj);
                return;
            }
        }

        _sortedObjects.Add(obj);
    }

    public void Execute()
    {
        foreach (AObject obj in _unsortedObjects)
            obj.Execute(_soundEventCallback);

        foreach (AObject obj in _sortedObjects)
            obj.Execute(_soundEventCallback);

        _unsortedObjects.Clear();
        _sortedObjects.Clear();
    }

    public void Clear()
    {
        _unsortedObjects.Clear();
        _sortedObjects.Clear();
    }
}