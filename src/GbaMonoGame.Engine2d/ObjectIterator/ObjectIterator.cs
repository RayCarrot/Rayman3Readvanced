using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Engine2d;

// Allocation-free enumerable for iterating game objects. The game uses different iterator classes which inherit from each other
// to perform the filtering and specify the knots. But sine we want to avoid allocations we use a single struct with flags instead.
// Since all the filters are pretty common we use flags to specify them instead. We also don't implement the enumerable interfaces
// to avoid allocations when boxing the enumerator struct to the IEnumerator interface.
public readonly struct ObjectIterator<T>
    where T : GameObject
{
    public ObjectIterator(KnotManager knotManager, IteratorFlags flags, IteratorKnot knot)
    {
        _knotManager = knotManager;
        _flags = flags;
        _knot = knot;
    }

    private readonly KnotManager _knotManager;
    private readonly IteratorFlags _flags;
    private readonly IteratorKnot _knot;

    public Enumerator GetEnumerator() => new(_knotManager, _flags, _knot);

    public struct Enumerator
    {
        public Enumerator(KnotManager knotManager, IteratorFlags flags, IteratorKnot knot)
        {
            _knotManager = knotManager;
            _flags = flags;
            _knot = knot switch
            {
                IteratorKnot.All => null,
                IteratorKnot.Current => knotManager.CurrentKnot,
                IteratorKnot.Previous => knotManager.PreviousKnot,
                _ => throw new ArgumentOutOfRangeException(nameof(knot), knot, null)
            };
        }

        private readonly KnotManager _knotManager;
        private readonly IteratorFlags _flags;
        private readonly Knot _knot;

        private int _index;
        private T _current;

        public T Current => _current;

        private bool GetNextObject(int instanceId, IteratorFlags flags)
        {
            // Validate the type
            GameObjectType type = _knotManager.GetGameObjectType(instanceId);
            if ((type == GameObjectType.AlwaysActor && (flags & IteratorFlags.AlwaysActor) == 0) ||
                (type == GameObjectType.Actor && (flags & IteratorFlags.Actor) == 0) ||
                (type == GameObjectType.Captor && (flags & IteratorFlags.Captor) == 0))
                return false;

            // Get the object
            T obj = (T)_knotManager.GetGameObject(instanceId);

            // Validate the enabled state
            if ((flags & IteratorFlags.EnabledStateMask) != 0)
            {
                if (obj.IsEnabled && (flags & IteratorFlags.Enabled) == 0)
                    return false;
                if (!obj.IsEnabled && (flags & IteratorFlags.Disabled) == 0)
                    return false;
            }

            // Valid
            _current = obj;
            return true;
        }

        public bool MoveNext()
        {
            // If no knot then we iterate over all objects
            if (_knot == null)
            {
                // Iterate until we reach a valid object or the end
                int length = _knotManager.GameObjectsCount;
                while (_index < length)
                {
                    if (GetNextObject(_index, _flags))
                    {
                        _index++;
                        return true;
                    }
                    _index++;
                }
            }
            // If a knot is specified then we iterate the objects defined in the knot
            else
            {
                // Always actors are never in a knot, so we need to handle those first
                int index = _index;
                if ((_flags & IteratorFlags.AlwaysActor) != 0)
                {
                    int length = _knotManager.GameObjectsCount;
                    IteratorFlags flags = (_flags & ~IteratorFlags.ActorTypeMask) | IteratorFlags.AlwaysActor;
                    while (index < length)
                    {
                        if (GetNextObject(index, flags))
                        {
                            _index++;
                            return true;
                        }
                        index++;
                        _index++;
                    }

                    index = _index - length;
                }

                // Handle actors
                if ((_flags & IteratorFlags.Actor) != 0)
                {
                    int length = _knot.ActorsCount;
                    IteratorFlags flags = (_flags & ~IteratorFlags.ActorTypeMask) | IteratorFlags.Actor;
                    while (index < length)
                    {
                        if (GetNextObject(_knot.ActorIds[index], flags))
                        {
                            _index++;
                            return true;
                        }
                        index++;
                        _index++;
                    }

                    index = _index - length;
                }

                // Handle captors
                if ((_flags & IteratorFlags.Captor) != 0)
                {
                    int length = _knot.CaptorsCount;
                    IteratorFlags flags = (_flags & ~IteratorFlags.ActorTypeMask) | IteratorFlags.Captor;
                    while (index < length)
                    {
                        if (GetNextObject(_knot.CaptorIds[index], flags))
                        {
                            _index++;
                            return true;
                        }
                        index++;
                        _index++;
                    }
                }
            }

            // Finished
            _current = null;
            return false;
        }
    }
}